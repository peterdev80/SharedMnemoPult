using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Pipes;
using System.IO;
using fmsman.Formats;
using System.Windows.Threading;
using System.IO.MemoryMappedFiles;

namespace fmsman
{
    public class Connection
    {
        private NamedPipeClientStream _client;
        private NamedPipeClientStream _aclient;

        private event Action<DateTime, string, string> OnLog;
        
        private event Action<BinaryReader> OnPipe;
        private event Action<ulong> OnDeletePipe;

        private event Action<ulong, long, long, long, long, int> OnStats;

        private event Action<BinaryReader> OnHosts;

        public event Action<IList<string>, string> OnConfig;

        public event Action Disconnected;

        private readonly Dictionary<int, Action<VarEntry[]>> _onVars = new Dictionary<int, Action<VarEntry[]>>();

        private Dispatcher _hostsDispatcher;
        private Dispatcher _pipeDispatcher;

        public bool Connect()
        {
            _client = new NamedPipeClientStream(".", "fmschanpipe", PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);

            try
            {
                _client.Connect(0);
            }
            catch (TimeoutException) { return false; }
            catch (IOException) { return false; }

            _client.ReadMode = PipeTransmissionMode.Message;

            _aclient = new NamedPipeClientStream(PipeDirection.InOut, false, true, _client.SafePipeHandle);

            StartReceive();
            return true;
        }

        private void StartReceive()
        {
            var buffer = new byte[128];
            _client.BeginRead(buffer, 0, buffer.Length, ReadIncoming, buffer);
        }

        /// <summary>
        /// Чтение и обработка административных команд
        /// </summary>
        /// <param name="ar"></param>
        private void ReadIncoming(IAsyncResult ar)
        {
            var buffer = (byte[])ar.AsyncState;

            var readed = _client.EndRead(ar);

            if (readed == 0)
            {
                Disconnected?.Invoke();

                return;
            }

            // Проверка и дочитывание недочитанного остатка сообщения клиента
            IEnumerable<byte> addbufchain = buffer;
            var addbufcreated = false;
            while (!_client.IsMessageComplete)
            {
                addbufcreated = true;
                var addbuf = new byte[256];
                readed = _client.Read(addbuf, 0, addbuf.Length);

                if (readed == 0)
                {
                    Disconnected?.Invoke();

                    return;
                }

                addbufchain = addbufchain.Concat(addbuf);
            }

            if (addbufcreated)
                buffer = addbufchain.ToArray();

            StartReceive();

            var ms = new MemoryStream(buffer);
            var brdr = new BinaryReader(ms);

            var answer = brdr.ReadChar();

            #region Обработка лога
            if (answer == 'A')
            {
                var timestamp = DateTime.FromBinary(brdr.ReadInt64());
                var str = brdr.ReadString();
                var sender = brdr.ReadString();

                OnLog?.Invoke(timestamp, str, sender);
            }
            #endregion

            #region Обработка локальных клиентов
            if (answer == 'B')
            {
                var subc = brdr.ReadChar();

                // Субкоманда "Новый клиент"
                if (subc == 'A')
                {
                    if (OnPipe != null)
                        _pipeDispatcher.BeginInvoke(OnPipe, brdr);
                }

                // Субкоманда клиент отключен
                if (subc == 'B')
                {
                    var iid = brdr.ReadUInt64();

                    if (OnDeletePipe != null)
                        _pipeDispatcher.BeginInvoke(OnDeletePipe, iid);
                }
            }
            #endregion

            #region Обработка статистики локальных клиентов
            if (answer == 'C')
            {
                var cnt = brdr.ReadInt32();

                for (var i = 0; i < cnt; i++)
                {
                    var instance = brdr.ReadUInt64();
                    var sended = brdr.ReadInt64();
                    var sendedcnt = brdr.ReadInt64();
                    var received = brdr.ReadInt64();
                    var receivedcnt = brdr.ReadInt64();
                    var varcnt = brdr.ReadInt32();

                    if (OnStats != null)
                        _pipeDispatcher.BeginInvoke(OnStats, instance, sended, sendedcnt, received, receivedcnt, varcnt);
                }
            }
            #endregion

            #region Обработка хостов
            if (answer == 'D')
            {
                if (OnHosts != null)
                    _hostsDispatcher.BeginInvoke(OnHosts, brdr);
            }
            #endregion

            #region Обработка переменных
            if (answer == 'E')
            {
                var subans = brdr.ReadChar();


                // Исходный список переменных
                if (subans == 'A')
                {
                    var vars = new List<VarEntry>();

                    var token = brdr.ReadInt32();

                    var cnt = brdr.ReadInt32();
                    var shname = brdr.ReadString();

                    if (VarEntry.File == null)
                    {
                        VarEntry.File = MemoryMappedFile.OpenExisting(shname);
                        VarEntry.Accessor = VarEntry.File.CreateViewAccessor();
                    }

                    for (var i = 0; i < cnt; i++)
                    {
                        var ve = new VarEntry
                                     {
                                         VarIndex = brdr.ReadUInt32(),
                                         VarName = brdr.ReadString(),
                                         VarType = brdr.ReadString(),
                                         VarSize = brdr.ReadInt32(),
                                         ShOffset = brdr.ReadUInt32(),
                                         Comment = brdr.ReadString(),
                                         Connection = this
                                     };

                        vars.Add(ve);
                    }


                    var va = vars.ToArray();
                    if (va.Length == 0)
                        return;

                    var ov = _onVars[token];
                    ov(va);
                }
            }
            #endregion

            #region Обработка глобального списка изменений
            if (answer == 'F')
            {
                var cnt = brdr.ReadInt32();

                for (var i = 0; i < cnt; i++)
                {
                    var num = brdr.ReadUInt32();
                    GlobalWatcher.Raise(num);
                }
            }
            #endregion

            #region Обработка конфигурации
            if (answer == 'I')
            {
                var fc = brdr.ReadUInt16();

                var fs = new List<string>();

                for (var i = 0; i < fc; i++)
                    fs.Add(brdr.ReadString());

                var cfg = brdr.ReadString();

                OnConfig?.Invoke(fs, cfg);
            }
            #endregion
        }

        public void SendCmd(byte[] cmd)
        {
            try
            {
                _aclient.Write(cmd, 0, cmd.Length);
            }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
        }

        /// <summary>
        /// Отсылает команду получения журнала
        /// </summary>
        /// <param name="OnLog">Событие, происходящее при получении очередной записи журнала</param>
        public void RetreiveLog(Action<DateTime, string, string> OnLog)
        {
            this.OnLog += OnLog;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('A');
            SendCmd(ms.ToArray());
        }

        /// <summary>
        /// Отсылает команду получения списка локальных клиентов
        /// </summary>
        /// <param name="OnPipe">Событие, происходящее при получении очередной записи списка</param>
        /// <param name="OnDeletePipe"></param>
        /// <param name="Dispatcher"></param>
        public void RetreivePipes(Action<BinaryReader> OnPipe, Action<ulong> OnDeletePipe, Dispatcher Dispatcher)
        {
            this.OnPipe += OnPipe;
            this.OnDeletePipe += OnDeletePipe;
            _pipeDispatcher = Dispatcher;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('B');
            SendCmd(ms.ToArray());
        }

        public void RetreivePipeStatistics(Action<ulong, long, long, long, long, int> OnStats)
        {
            this.OnStats = OnStats;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('C');
            SendCmd(ms.ToArray());
        }

        public void RetreiveHosts(Dispatcher Dispatcher, Action<BinaryReader> OnHosts)
        {
            _hostsDispatcher = Dispatcher;

            if (OnHosts != null)
                this.OnHosts += OnHosts;

            RetreiveHosts();
        }

        public void RetreiveHosts()
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('D');
            SendCmd(ms.ToArray());
        }

        public void RetreivePipeVars(int token, Action<VarEntry[]> OnVars, ulong instance)
        {
            _onVars[token] = OnVars;

            RetreivePipeVars(token, instance);
        }

        public void RetreivePipeVars(int token, ulong instance)
        {

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('E');
            wr.Write('A');
            wr.Write(token);
            wr.Write(instance);
            SendCmd(ms.ToArray());
        }

        public void RetreiveConfig()
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('I');

            SendCmd(ms.ToArray());
        }

        public void DetachVarMap(int token)
        {
            _onVars.Remove(token);
        }

        public void AttachToGlobalChangesList()
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('F');
            wr.Write('A');
            SendCmd(ms.ToArray());
        }
        
        public void RetreiveGlobalChangesList()
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('F');
            wr.Write('B');
            SendCmd(ms.ToArray());
        }

        public void SendVarAsChanged(uint VarIndex)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('G');
            wr.Write(VarIndex);
            SendCmd(ms.ToArray());
        }

        internal void Disconnect()
        {
            _client.Dispose();
        }
    }
}
