using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using fmslstrap.Channel;
using fmslstrap.Variables;
using System.Diagnostics;
using fmslstrap.Interface;

namespace fmslstrap.Pipe
{
    /// <summary>
    /// Обслуживает подключение Win32/Native клиентов к каналам
    /// через механизм именованных каналов :)
    /// </summary>
    internal partial class PipeManager
    {
        #region Частные данные
        private event Action<PipeManager> OnPipeFinalize;

        private static event Action<PipeManager> OnCreatePipe;

        /// <summary>
        /// Счетчик экземпляров сервера
        /// </summary>
        private static ulong _instancecnt = 1;

        /// <summary>
        /// Идентификатор экземпляра сервера
        /// </summary>
        private readonly ulong _instanceid;

        /// <summary>
        /// Имя конечной точки клиента
        /// </summary>
        private string _endpointname;

        /// <summary>
        /// Подключенный канал
        /// </summary>
        private ChanConfig _channel;

        /// <summary>
        /// Список активных клиентов
        /// </summary>
        private static readonly List<PipeManager> ActivePipes = new List<PipeManager>();
        
        /// <summary>
        /// Потоковая блокировка списка активных клиентов
        /// </summary>
        private static readonly ReaderWriterLockSlim ActivePipesLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Делегат приема данных в канале
        /// </summary>
        private ChanConfig.DataReceived _rcvhandler;

        /// <summary>
        /// Кэш пришедших, но еще невостребованных сообщений канала
        /// </summary>
        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly Dictionary<UInt32, byte[]> _pipemsgcache = new Dictionary<UInt32, byte[]>();

        /// <summary>
        /// Имя канала переменных
        /// </summary>
        private string _channelname;

        /// <summary>
        /// Событие ожидания завершения подключения к каналу
        /// </summary>
        private readonly EventWaitHandle _joincomplete = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// Карта обработчиков команд
        /// </summary>
        private readonly Dictionary<char, Func<BinaryReader, byte[]>> _cmds = new Dictionary<char, Func<BinaryReader, byte[]>>();

        /// <summary>
        /// Связь с клиентом
        /// </summary>
        private readonly IClientTransport _transport;

        private static readonly string fmsldrbasepath =
            // ReSharper disable once PossibleNullReferenceException
            AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("fmsldr")).Location;
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Количество принятых в транспортном канале байт
        /// </summary>
        public long ReceivedAmount;

        /// <summary>
        /// Количество принятых в транспортном канале посылок
        /// </summary>
        public long ReceivedCount;

        /// <summary>
        /// Количество отправленных в транспортный канал байт
        /// </summary>
        public long SendedAmount;

        /// <summary>
        /// Количество отправленных в транспортный канал посылок
        /// </summary>
        public long SendedCount;
        #endregion

        #region Конструкторы
        private PipeManager(IClientTransport Transport)
        {
            _transport = Transport;

            try
            {
                ActivePipesLock.EnterWriteLock();

                ActivePipes.Add(this);
            }
            finally
            {
                ActivePipesLock.ExitWriteLock();
            }

            _instanceid = _instancecnt++;

            _cmds.Add('B', GetBox);
            _cmds.Add('C', SendVariablesPack);
            _cmds.Add('D', RegisterVariable);
            _cmds.Add('E', GetVarMap);
            _cmds.Add('H', JoinChannel);
            _cmds.Add('I', JoinVariableChannel);
            _cmds.Add('K', GetVariablesMap);
            _cmds.Add('L', SavePersistentVariable);
            _cmds.Add('M', RemovePersistentVariable);
            _cmds.Add('N', MakeSnapshot);
            _cmds.Add('O', RestoreSnapshot);
            _cmds.Add('P', ResetWatchDogs);
            _cmds.Add('Q', UpdateRcvStatus);

            _transport.Received += Received;
            _transport.UpdateStats += OnStats;
            _transport.Closed += Closed;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms, Encoding.UTF8);
            wr.Write((byte)'M');
            wr.Write(Manager.ClientAPIVersion);
            wr.Write(Config.DomainName);
            wr.Write(Manager.VersionString);

            // Путь к актуальной версии fmslapi
            // ReSharper disable once AssignNullToNotNullAttribute
            var ap = Path.Combine(Path.GetDirectoryName(fmsldrbasepath), "fmslapi.dll");
            wr.Write(File.Exists(ap) ? ap : "");

            _transport.Send(ms.ToArray());
        }

        public static void New(IClientTransport Transport)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new PipeManager(Transport);
        }
        #endregion

        #region Статистика канала
        /// <summary>
        /// Обновление статистики
        /// </summary>
        private void OnStats(uint Received, uint Sended)
        {
            if (Received != 0)
            {
                Interlocked.Add(ref SendedAmount, Received);
                Interlocked.Increment(ref SendedCount);
            }

            if (Sended != 0)
            {
                Interlocked.Add(ref ReceivedAmount, Sended);
                Interlocked.Increment(ref ReceivedCount);
            }
        }
        #endregion

        #region Прием/обработка клиентских команд
        /// <summary>
        /// Обработка пакета данных клиента
        /// </summary>
        /// <param name="Data">Принятые данные</param>
        /// <param name="Reply">Метод отправки ответа на запрос</param>
        private void Received(Stream Data, Action<byte[]> Reply)
        {
            var bmsgrdr = new BinaryReader(Data, Encoding.UTF8);

            // Читаем команду
            var cmd = bmsgrdr.ReadChar();

            #region Команда Send bytes
            if (cmd == 'A' || cmd == 'G')
            {
                SendBytes(cmd, bmsgrdr);
                return;
            }
            #endregion

            #region Команда Send Stream

            if (cmd == 'S')
            {
                SendStream(bmsgrdr);
                return;
            }

            #endregion

            #region Команда Disconnect
            if (cmd == 'F')
            {
                // Команда Disconnect
                _transport.Close();
                //return false;
                return;
            }
            #endregion

            #region Команда Emergency Exit
            if (cmd == 'X')
            {
                InterfaceManager.PressExit();
                //return false;
                return;
            }
            #endregion

            #region Блок команд администрирования
            if (cmd == 'Z')
            {
                ProceedAdministration(Data);
                return;
            }
            #endregion

            #region Парсинг командной строки
            if (cmd == 'Y')
            {
                CmdLine.Execute(Data);
                return;
            }
            #endregion

            if (_cmds.TryGetValue(cmd, out var cp))
            {
                var r = cp(bmsgrdr);

                if (r != null)
                    Reply(r);
            }
        }

        private void SendBytes(char cmd, BinaryReader rdr)
        {
            string rcvr = null;
            if (cmd == 'G')
                rcvr = rdr.ReadString();

            var order = rdr.ReadUInt32();
            var sender = rdr.ReadUInt32();
            var size = rdr.ReadInt32();
            var buf = rdr.ReadBytes(size);

            _joincomplete.WaitOne();

            // Посылаем пакет в сеть

            _channel?.SendMessage(new DataPacket(_instanceid) { Data = buf, OrderID = order, SenderID = sender }, rcvr);
        }

        /// <summary>
        /// Отправка в канал потока данных
        /// </summary>
        private void SendStream(BinaryReader rdr)
        {
            var order = rdr.ReadUInt32();
            var sender = rdr.ReadUInt32();
            var rcvr = rdr.ReadString();
            var pn = rdr.ReadString();

            _joincomplete.WaitOne();

            var ps = new NamedPipeClientStream(".", pn, PipeDirection.In);
            try
            {
                ps.Connect();
            }
            catch (InvalidOperationException)
            {
                Debug.Fail("fmslstrap: in SendStream Pipe.Connect failed");
// ReSharper disable HeuristicUnreachableCode
                return;
// ReSharper restore HeuristicUnreachableCode
            }

            var rp = new DataPacket(_instanceid) { OrderID = order, SenderID = sender };
            rp.SetSourceStream(ps, s => Interlocked.Add(ref SendedAmount, s));

            _channel.SendMessage(rp, rcvr);

            ThreadPool.QueueUserWorkItem(x => rp.LocalReady(null), null);

            Interlocked.Increment(ref SendedCount);
        }
        
        private byte[] GetBox(BinaryReader rdr)
        {
            var num = rdr.ReadUInt32();
            byte[] buf;

            lock (_pipemsgcache)
            {
                try
                {
                    buf = _pipemsgcache[num];
                    _pipemsgcache.Remove(num);
                }
                catch (KeyNotFoundException)
                {
                    buf = new byte[1];
                }
            }

            return buf;
        }

        private byte[] JoinChannel(BinaryReader rdr)
        {
            lock (this)
            {
                var codepage = rdr.ReadUInt16();
                var eprd = new BinaryReader(rdr.BaseStream, Encoding.GetEncoding(codepage));
                _endpointname = CheckEndPointName(eprd.ReadString());
                _channelname = eprd.ReadString();
                var rcv = eprd.ReadBoolean();
                _rcvhandler = rcv ? new ChanConfig.DataReceived(OnNativeChanReceive) : null;

                var ct = ChannelType.Regular;
                var isadmloc = _channelname.ToLowerInvariant() == "admloc";

                if (isadmloc)
                {
                    ct = ChannelType.Local;
                    _rcvhandler = OnNativeChanReceive;
                }

                _channel = Manager.SubscribeToChannel(_channelname, _rcvhandler, ct);

                OnCreatePipe?.Invoke(this);

                _joincomplete.Set();
            }

            return null;
        }

        private byte[] UpdateRcvStatus(BinaryReader rdr)
        {
            lock (this)
            {
                var rcv = rdr.ReadBoolean();

                if (_channel.ChanType != ChannelType.Regular)
                    return null;

                if (rcv)
                {
                    if (_rcvhandler != null)
                        return null;

                    _rcvhandler = OnNativeChanReceive;
                    _channel.OnDataReceived += OnNativeChanReceive;
                }
                else
                {
                    _rcvhandler = null;
                    _channel.OnDataReceived -= OnNativeChanReceive;
                }
            }

            return null;
        }
        #endregion

        #region Внутренние методы
        private void Closed()
        {
            _transport.Received -= Received;
            _transport.UpdateStats -= OnStats;
            _transport.Closed -= Closed;

            try
            {
                ActivePipesLock.EnterWriteLock();
                ActivePipes.Remove(this);
            }
            finally
            {
                ActivePipesLock.ExitWriteLock();
            }

            if (_rcvhandler != null)
                Manager.UnSubscribeFromChannel(_channelname, _rcvhandler);

            if (!string.IsNullOrEmpty(_varchanname))
                VariablesManager.LeaveVariableChannel(_varchanname, OnVarChanged);

            OnPipeFinalize?.Invoke(this);
        }
        #endregion

        #region Отправка команды клиенту

        /// <summary>
        /// Отправляет команду клиенту
        /// </summary>
        /// <param name="Message">Команда</param>
        // ReSharper disable once UnusedMember.Local
        private void MessageToClient(string Message)
        {
            var msg = Encoding.UTF8.GetBytes(Message + Environment.NewLine);
            MessageToClient(msg, msg.Length);
        }

        /// <summary>
        /// Отправляет команду клиенту
        /// </summary>
        /// <param name="Message">Команда</param>
        private void MessageToClient(byte[] Message)
        {
            _transport.Send(Message);
        }

        /// <summary>
        /// Отправляет команду клиенту
        /// </summary>
        /// <param name="pars">Команда и параметры</param>
        public void MessageToClient(params object[] pars)
        {
            var ms = new MemoryStream();
            var bwr = new BinaryWriter(ms);

            foreach (var p in pars)
            {
                if (p == null)
                    continue;

                if (p is char) { bwr.Write((char)p); continue; }
                if (p is bool) { bwr.Write((bool)p); continue; }
                if (p is Int32) { bwr.Write((Int32)p); continue; }
                if (p is UInt32) { bwr.Write((UInt32)p); continue; }
                if (p is string) { bwr.Write((string)p); continue; }
                if (p is byte[]) { bwr.Write((byte[])p); continue; }
                if (p is byte) { bwr.Write((byte)p); continue; }
                if (p is Guid) { bwr.Write(((Guid)p).ToByteArray()); continue; }

                throw new ArgumentException($@"Неподдерживаемый тип команды {p.GetType()}");
            }

            MessageToClient(ms.ToArray());
        }
        #endregion

        #region Внутренние вспомогательные процедуры
        /// <summary>
        /// Проверяет имя клиентов на дублирование
        /// </summary>
        /// <param name="Name">Имя клиента</param>
        /// <returns>Уникальное имя клиента</returns>
        private static string CheckEndPointName(string Name)
        {
            try
            {
                ActivePipesLock.EnterReadLock();

                var name = Name;
                if (ActivePipes.Any(pipe => pipe._endpointname == name))
                    Name += ("_" + Guid.NewGuid());
            }
            finally
            {
                ActivePipesLock.ExitReadLock();
            }

            return Name;
        }
        #endregion

        #region Обработчики внешних событий
        /// <summary>
        /// Событие, происходящее при приеме посылки в канал из сети
        /// </summary>
        /// <param name="Channel">Канал, из которого приняты данные</param>
        /// <param name="Packet">Принятый пакет</param>
        /// <remarks>
        /// Прием данных для нативных клиентов
        /// </remarks>
        private void OnNativeChanReceive(ChanConfig Channel, DataPacket Packet)
        {
            if (Packet.CreatorInstanceID == _instanceid)
                return;

            var msg = Packet.Data;

            if (msg != null)
                MessageToClient('R', Packet.Sender, Packet.OrderID, Packet.SenderID, (UInt32)msg.Length, msg);
            else
            {
                var pn = Guid.NewGuid().ToString();
                var srv = new NamedPipeServerStream(pn, PipeDirection.Out, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 262144, 262144);

                Packet.AddTargetStream(srv, l => Interlocked.Add(ref ReceivedAmount, l));
                
                MessageToClient('Q', Packet.Sender, Packet.OrderID, Packet.SenderID, Packet.Size, pn);
            }
        }

        #endregion

        #region Публичные вспомогательные процедуры
        /// <summary>
        /// Закрывает все клиентские подключения
        /// </summary>
        public static void ShutdownAllPipes()
        {
            PipeTransport.ShutdownAllPipes();
        }
        #endregion
    }
}
