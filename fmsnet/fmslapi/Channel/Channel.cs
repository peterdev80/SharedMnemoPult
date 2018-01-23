using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using fmslapi.Channel.Transport;
using Timer = System.Threading.Timer;

namespace fmslapi.Channel
{
    #region Публичные делегаты
    /// <summary>
    /// Делегат события изменения состояния подключения канала
    /// </summary>
    /// <param name="args">Новое состояние подключения канала</param>
    public delegate void ChannelStateChanged(ChannelStateChangedStates args);
    #endregion

    /// <summary>
    /// Клиентский объект, представляющий подключение к каналу
    /// </summary>
    internal partial class Channel : IChannel, IChannel1
    {
        #region Частные поля
        private readonly ChannelParams _params;

        /// <summary>
        /// Режим канала
        /// </summary>
        private readonly ChannelMode _mode;

        /// <summary>
        /// Имя канала
        /// </summary>
        private readonly string _channel;

        /// <summary>
        /// Событие, происходящее по приходу данных в обычный канал
        /// </summary>
        private DataReceived _received;

        /// <summary>
        /// Событие изменения состояния подключения канала
        /// </summary>
        private readonly ChannelStateChanged _statechanged;        
        
        /// <summary>
        /// Событие изменения состояния подключения канала переменных
        /// </summary>
        private readonly ChannelStateChanged _vstatechanged;

        /// <summary>
        /// Событие, происходящее при внешнем изменении пакета переменных
        /// </summary>
        private readonly VariablesChanged _changed;

        /// <summary>
        /// Признак активности канала обмена
        /// </summary>
        private bool _active;

        /// <summary>
        /// Имя конечной точки канала
        /// </summary>
        private readonly string _endpoint;

        private Action<byte[]> _additionalsubscribers;

        private DataReceived _proxy;

        /// <summary>
        /// Порядковый номер отправляемого пакета
        /// </summary>
        private long _orderindex;

        private readonly UInt32 _mysenderid = (UInt32)Guid.NewGuid().GetHashCode();

        private Reorder.ReorderBase _reorder;

        private readonly ITransport _transport;

        protected Queue<object[]> _syncqueue;
        #endregion

        #region Конструкторы
        /// <summary>
        /// Создает новый клиентский объект канала
        /// </summary>
        public Channel(ChannelParams p)
        {
            _params = p;

            _changed = p.VariablesChanged;
            _endpoint = p.EndPoint;
            _statechanged = p.ChannelStateChanged;
            _vstatechanged = p.ChannelStateChanged;
            _mode = p.ChannelMode;
            _channel = p.Channel;
            _varmap = p.VarMap;

            /*if (_changed == null && _mode == ChannelMode.SnapshotVariables)
                _receive = false;*/

            if (_mode == ChannelMode.Variables)
                _statechanged = VarStateChanged;

            _transport = p.Manager.GetTransport();
            _transport.Received += ProceedIncoming;
            _transport.Closed += FinalizeClient;

            try
            {
                CheckConnect();
                // ReSharper disable once VirtualMemberCallInConstructor
                RaiseDelegate(_statechanged, ChannelStateChangedStates.FirstConnect);
            }
            catch(SystemException e)
            {
                if (!(e is TimeoutException || e is IOException))
                    throw;

                // ReSharper disable once VirtualMemberCallInConstructor
                RaiseDelegate(_statechanged, ChannelStateChangedStates.CantConnect);
            }
        }
        #endregion

        #region Частные методы

        private Timer _ustimer;
        private readonly TimeSpan _ustimerdelay = TimeSpan.FromSeconds(3);

        // ReSharper disable once InconsistentNaming
        private bool _rcvstatus
        {
            get
            {
                var r = _received;

                return (r != null && r.GetInvocationList().Length != 0) || _syncqueue != null;
            }
        }

        private void UpdateRcvStatus()
        {
            if (!_active)
                return;

            if (_mode != ChannelMode.Regular)
                return;

            if (_rcvstatus)
            {
                MessageToServer('Q', true);
                return;
            }

            // Отписка от получения данных из канала осуществляется с задержкой в 3 сек
            lock (this)
            {
                if (_ustimer == null)
                    _ustimer = new Timer(SendRcvStatus, null, _ustimerdelay, TimeSpan.Zero);
                else
                    _ustimer.Change(_ustimerdelay, TimeSpan.Zero);
            }
        }

        private void SendRcvStatus(object o)
        {
            lock (this)
            {
                _ustimer.Dispose();
                _ustimer = null;
            }

            if (!_active)
                return;

            // ReSharper disable once UnusedVariable
            var r = _received;

            MessageToServer('Q', _rcvstatus);
        }

        /// <summary>
        /// Подключение к fmsldr обязательно сериализуется
        /// </summary>
        private static readonly Mutex Connmutex = new Mutex(false);

        /// <summary>
        /// Попытка установки соединения
        /// </summary>
        public void CheckConnect()
        {
            try
            {
                Connmutex.WaitOne();

                if (_active)
                    return;

                _transport.TryConnect(TimeSpan.FromMilliseconds(20));

                var epname = string.IsNullOrEmpty(_endpoint) ? _params.Manager.EndPointName : string.Format("{0}-{1}", _params.Manager.EndPointName, _endpoint);

                var rcv = _received != null && _received.GetInvocationList().Length != 0;

                switch (_mode)
                {
                    case ChannelMode.Regular:
                        MessageToServer('H', (UInt16)Encoding.UTF8.CodePage, epname, _channel, rcv);
                        break;

                    case ChannelMode.Variables:
                        MessageToServer('I', (UInt16)Encoding.UTF8.CodePage, epname, _channel, _varmap, (Int64)0);
                        break;
                }

                _active = true;
            }
            finally
            {
                Connmutex.ReleaseMutex();
            }

            _transport.EnableReceive();
        }

        #region Поддержка ответа
        private class Reply : ISenderChannel
        {
            private readonly string _sendername;
            private readonly IChannel _chan;

            public Reply(string Sender, IChannel Channel)
            {
                _sendername = Sender;
                _chan = Channel;
            }

            void ISenderChannel.Reply(byte[] Message)
            {
                _chan.SendMessageToReceiver(_sendername, Message);
            }

            unsafe void ISenderChannel.Reply(byte* Buffer, int Length)
            {
                throw new NotImplementedException();
            }

            void ISenderChannel.Reply(IntPtr Buffer, int Length)
            {
                throw new NotImplementedException();
            }

            public void ReplyStream(Stream Data)
            {
                var s = ReplyStream();
                Data.CopyTo(s);
                s.Close();
            }

            public Stream ReplyStream()
            {
                return _chan.SendMessageStream(_sendername);
            }

            string ISenderChannel.Sender => _sendername;
        }
        #endregion

        /// <summary>
        /// Обработка серверной команды
        /// </summary>
        private void ProceedIncoming(Stream Data)
        {
            var rdr = new BinaryReader(Data);

            var cmdc = rdr.ReadChar();

            switch (cmdc)
            {
                #region Receive message
                // Прием байтового массива из обычного канала
                case 'R':
                    var sender = rdr.ReadString();
                    var orderid = rdr.ReadUInt32();     // Порядковый номер пакета при отправке
                    var senderid = rdr.ReadUInt32();    // Уникальная метка отправителя
                    var size = rdr.ReadUInt32();

                    if (size == 0)
                        break;

                    var buf = new byte[size];
                    rdr.Read(buf, 0, (int)size);

                    var p = new ReceivedMessage(orderid, sender, buf, senderid);

                    var ord = _reorder;

                    if (ord != null)
                    {
                        ord.OnNewPacket(p);
                        break;
                    }

                    RoutePacketToReceiver(p);
                    break;
                #endregion

                #region Receive variable value
                // Прием значения переменной
                case 'N':
                    var isinit = rdr.ReadBoolean();

                    var ntf = new HashSet<Variable>();

                    while (true)
                    {
                        var vindex = rdr.ReadInt32();
                        if (vindex == 0) break;

                        try
                        {
                            _varlistnlock.EnterReadLock();

                            if (_varlistn.TryGetValue(vindex, out var var))
                            {
                                var ut = var.UpdateTrigger;

                                if (ut == null || isinit)
                                {
                                    var.Set();
                                    ntf.Add(var);
                                }
                                else
                                    ut.AddChangedVariable(var);
                            }
                        }
                        finally
                        {
                            _varlistnlock.ExitReadLock();
                        }
                    }

                    if (ntf.Count == 0) break;

                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        foreach (var v in ntf)
                        {
                            try
                            {
                                v.RaiseVariableChanged(isinit);
                            }
                            catch (TargetInvocationException) { }
                        }

                        RaiseDelegate(_changed, isinit, ntf.ToArray<IVariable>());
                    });
                    break;
                #endregion

                #region Receive Variable Index
                // Прием индекса переменной
                case 'V':
                    lock (_waitingvars)
                    {
                        EventWaitHandle evt;

                        var vindex = rdr.ReadInt32();
                        var vname = rdr.ReadString();

                        if (vindex == -1)
                        {
                            if (_waitingvars.TryGetValue(vname, out evt))
                            {
                                var vvar = _varlist[vname];
                                vvar.Index = vindex;
                                vvar.VariableType = VariableType.Unknown;
                                evt.Set();
                            }

                            break;
                        }
                        
                        var vtype = rdr.ReadChar();
                        var shmemname = rdr.ReadString();
                        var sharedoffs = rdr.ReadInt32();
                        var fdthreshold = rdr.ReadUInt16();

                        if (_waitingvars.TryGetValue(vname, out evt))
                        {
                            var vvar = _varlist[vname];
                            vvar.Index = vindex;
                            vvar.Threshold = Convert.ToSingle(Math.Pow(10, -fdthreshold));

                            VariableType vt;

                            switch (vtype)
                            {
                                case 'W': vt = VariableType.WatchDog; break;
                                case 'B': vt = VariableType.Boolean; break;
                                case 'I': vt = VariableType.Int32; break;
                                case 'L': vt = VariableType.Long; break;
                                case 'F': vt = VariableType.Single; break;
                                case 'D': vt = VariableType.Double; break;
                                case 'C': vt = VariableType.Char; break;
                                case 'K': vt = VariableType.KMD; break;
                                case 'S': vt = VariableType.String; break;
                                case 'A': vt = VariableType.ByteArray; break;
                                default: vt = VariableType.Unknown; break;
                            }

                            vvar.VariableType = vt;

                            if (sharedoffs != -1)
                                vvar.AssignToSharedMemory(shmemname, sharedoffs);

                            evt.Set();
                        }
                    }

                    break;
                #endregion

                #region Receive message stream
                case 'Q':
                    sender = rdr.ReadString();
                    orderid = rdr.ReadUInt32();             // Порядковый номер пакета при отправке
                    senderid = rdr.ReadUInt32();
                    size = rdr.ReadUInt32();
                    var rpipename = rdr.ReadString();

                    var cds = new ChannelDataStream(rpipename, (int)size);

                    var m = new ReceivedMessage(orderid, sender, cds, senderid);
                    
                    RoutePacketToReceiver(m);

                    if (!m.StreamWasAffected)
                        cds.Close();

                    break;
                #endregion
            }

            var ms = Data as MemoryStream;

            if (ms != null)
                _additionalsubscribers?.Invoke(ms.ToArray());
        }

        internal void RoutePacketToReceiver(ReceivedMessage Packet)
        {
            var px = _proxy;                        // Надо, т.к. _proxy может измениться в процессе
            var rply = new Reply(Packet.Sender, this);
            if (px != null)
                px(rply, Packet);
            else
                RaiseDelegate(_received, rply, Packet);
        }

        /// <summary>
        /// Завершает сессию клиента
        /// </summary>
        private void FinalizeClient()
        {
            UsePacketOrder = null;     // Нужно освободить объекты ожидания

            _active = false;
            RaiseDelegate(_statechanged, ChannelStateChangedStates.Disconnected);

            lock (_waitingvars)
            {
                foreach (var ew in _waitingvars.Values)
                {
                    ew.Set();
                }
                _waitingvars.Clear();
            }
        }
        #endregion

        #region Защищенные методы
        /// <summary>
        /// Отправка сообщения серверу
        /// </summary>
        /// <param name="pars">Команда и параметры</param>
        public void MessageToServer(params object[] pars)
        {
            var ms = new MemoryStream();
            var wrtr = new BinaryWriter(ms, Encoding.UTF8);

            foreach (var p in pars)
            {
                if (p is char) { wrtr.Write((char)p); continue; }
                if (p is bool) { wrtr.Write((bool)p); continue; }
                if (p is Int32) { wrtr.Write((Int32)p); continue; }
                if (p is UInt16) { wrtr.Write((UInt16)p); continue; }
                if (p is Int64) { wrtr.Write((Int64)p); continue; }
                if (p is UInt32) { wrtr.Write((UInt32)p); continue; }
                if (p is string) { wrtr.Write((string)p); continue; }
                if (p is byte[]) { wrtr.Write((byte[])p); continue; }
                if (p is byte) { wrtr.Write((byte)p); continue; }
                if (p is Guid) { wrtr.Write(((Guid)p).ToByteArray()); continue; }
                if (p is IVariable[])
                {
                    foreach (var v in p as IVariable[])
                        wrtr.Write((UInt16)v.Index);

                    continue;
                }

                throw new ArgumentException(string.Format(@"Неподдерживаемый тип команды {0}", p.GetType()));
            }

            MessageToServer(ms.ToArray());
        }

        /// <summary>
        /// Отправка сообщения серверу
        /// </summary>
        public void MessageToServer(byte[] Message)
        {
            _transport.Send(Message);
        }
        #endregion

        #region IChannel
        /// <summary>
        /// Отправка данных в канал
        /// </summary>
        /// <param name="Message">Данные</param>
        public void SendMessage(byte[] Message)
        {
            SendMessage(Message, Message.Length);
        }

        public void SendMessageStream(Stream Message)
        {
            var ms = Message as MemoryStream;
            if (ms != null)
            {
                if (ms.Length <= 8192)
                {
                    SendMessage(ms.ToArray());
                    return;
                }
                
                ms.Seek(0, SeekOrigin.Begin);
            }

            using (var ss = SendMessageStream())
                Message.CopyTo(ss);
        }

        public Stream SendMessageStream(string ToHost = "")
        {
            if (!_active)
                return null;

            var ss = new ChannelSendStream();

            var order = (uint)(Interlocked.Increment(ref _orderindex) - 1);

            MessageToServer('S', order, _mysenderid, ToHost, ss.PipeName);

            return ss;
        }

        /// <summary>
        /// Отправка данных в канал
        /// </summary>
        /// <param name="Message">Данные</param>
        /// <param name="Length">Длина передаваемого фрагмента</param>
        public void SendMessage(byte[] Message, int Length)
        {
            if (!_active)
                return;

            if (Message.Length == 0)
                return;

            var order = (uint)(Interlocked.Increment(ref _orderindex) - 1);

            MessageToServer('A', order, _mysenderid, Length,
                            Message.Length == Length ? Message : Message.Take(Length).ToArray());
        }

        /// <summary>
        /// Отправка сообщения конкретному хосту в канале
        /// </summary>
        /// <param name="Receiver">Имя получателя сообщения</param>
        /// <param name="Message">Сообщение</param>
        /// <param name="Length">Длина сообщения</param>
        public void SendMessageToReceiver(string Receiver, IntPtr Message, int Length)
        {
            var bf = new byte[Length];
            Marshal.Copy(Message, bf, 0, Length);

            SendMessageToReceiver(Receiver, bf);
        }

        /// <summary>
        /// Отправка сообщения конкретному хосту в канале
        /// </summary>
        /// <param name="Receiver">Имя получателя сообщения</param>
        /// <param name="Message">Сообщение</param>
        public void SendMessageToReceiver(string Receiver, byte[] Message)
        {
            if (!_active)
                return;

            var len = Message.Length;

            if (len == 0)
                return;

            var order = (uint)(Interlocked.Increment(ref _orderindex) - 1);

            MessageToServer('G', Receiver, order, _mysenderid, len, Message);
        }

        /// <summary>
        /// Отправляет сообщение в канал
        /// </summary>
        /// <param name="Buffer">Указатель на область памяти</param>
        /// <param name="Length">Длина посылки</param>
        public unsafe void SendMessage(byte *Buffer, int Length)
        {
            var bf = new byte[Length];
            Marshal.Copy(new IntPtr(Buffer), bf, 0, Length);

            SendMessage(bf);
        }

        /// <summary>
        /// Отправляет сообщение в канал
        /// </summary>
        /// <param name="Buffer">Указатель на область памяти</param>
        /// <param name="Length">Длина посылки</param>
        public unsafe void SendMessage(IntPtr Buffer, int Length)
        {
            SendMessage((byte*)Buffer.ToPointer(), Length);
        }

        /// <summary>
        /// Выход из канала
        /// </summary>
        public void Leave()
        {
            if (!_active)
                return;
            
            _active = false;
            
            MessageToServer('F');
        }

        public event DataReceived Received
        {
            add 
            { 
                _received += value;
                UpdateRcvStatus();
            }

            remove
            {
                lock (this)
                {
                    if (_received != null)
                        // ReSharper disable once DelegateSubtraction
                        _received -= value;
                }

                UpdateRcvStatus();
            }
        }

        public IManager ParentAPIManager => _params.Manager;

        /// <summary>
        /// Имя канала
        /// </summary>
        public string Name => _channel;

        /// <summary>
        /// Сброс в неактивное состояние всех сторожевых переменных выбранной таблицы
        /// </summary>
        public void ResetWatchDogs()
        {
            MessageToServer('P', (UInt16)0);
        }

        /// <summary>
        /// Сброс в неактивное состояние всех сторожевых переменных выбранной таблицы
        /// </summary>
        public void ResetWatchDogs(IEnumerable<IWatchDogVariable> Variables)
        {
            var va = Variables.ToArray();

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write((byte)'P');
            wr.Write((UInt16)va.Length);
            foreach (var v in va)
                wr.Write((UInt16)v.Index);

            MessageToServer(ms.ToArray());
        }
        #endregion

        #region Фильтрация сообщений
        public Action<ISenderChannel, byte[]> SetChannelReceiveProxy(DataReceived Proxy)
        {
            _proxy = Proxy;
            return SchedulePacketToReceiver;
        }

        private void SchedulePacketToReceiver(ISenderChannel Sender, byte[] Message)
        {
            RaiseDelegate(_received, Sender, Message);
        }
        #endregion

        #region Восстановление последовательности пакетов
        /// <summary>
        /// Переупорядочивать принятые пакеты для соответствия 
        /// полю OrderID
        /// </summary>
        public Reorder.ReorderBase UsePacketOrder
        {
            get => _reorder;
            set 
            {
                if (value == null && _reorder != null)
                {
                    _reorder.DetachChannel();
                    _reorder = null;
                    return;
                }

                _reorder = value;

                _reorder?.AttachChannel(this);
            }
        }

        public bool IsHostInChannel(string Host)
        {
            return ParentAPIManager.IsHostInChannel(_channel, Host);
        }

        #endregion

        #region Синхронный прием

        public bool SyncReceive
        {
            get => _syncqueue != null;

            set
            {
                if (value)
                {
                    if (_received != null || _syncqueue != null)
                        throw new ArgumentException("Синхронный прием недоступен");

                    // ReSharper disable once InconsistentlySynchronizedField
                    _syncqueue = new Queue<object[]>();
                }
                else 
                    // ReSharper disable once InconsistentlySynchronizedField
                    _syncqueue = null;

                UpdateRcvStatus();
            }
        }

        public bool Receive(out ISenderChannel Sender, out ReceivedMessage Message)
        {
            Sender = null;
            Message = null;

            lock (_syncqueue)
            {
                if (_syncqueue.Count == 0)
                    return false;

                var p = _syncqueue.Dequeue();

                Sender = p[0] as ISenderChannel;
                Message = p[1] as ReceivedMessage;
            }

            Debug.Assert(Sender != null);
            Debug.Assert(Message != null);

            return true;
        }
        #endregion

        public virtual void RaiseDelegate(Delegate Target, params object[] pars)
        {
            var sq = _syncqueue;

            if (sq != null)
                lock (sq)
                {
                    if (pars.Length == 2 && pars[0] is ISenderChannel && pars[1] is ReceivedMessage)
                        sq.Enqueue(pars);

                    return;
                }

            if (Target == null)
                return;

            try
            {
                Target.DynamicInvoke(pars);
            }
            catch (TargetInvocationException ex)
            {
                Exception x = ex;
                var sb = new StringBuilder();
                while (x != null)
                {
                    sb.AppendLine(x.ToString());
                    sb.AppendLine(x.Message);
                    sb.AppendLine(x.StackTrace);
                    sb.AppendLine("-----------------\r\n\r\n\r\n\r\n");
                    x = x.InnerException;
                }

                File.AppendAllText(Environment.ExpandEnvironmentVariables(@"%AllUsersProfile%\FMS700\excpts.txt"), sb.ToString());

                MessageBox.Show(sb.ToString());
            }
        }

        internal VariablesChanged VariablesChanged => _changed;

        #region IChannel1

        IManager1 IChannel1.ParentAPIManager => ParentAPIManager as IManager1;

        #endregion

    }
}
