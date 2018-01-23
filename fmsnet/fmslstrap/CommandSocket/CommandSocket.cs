using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using fmslstrap.Channel;

namespace fmslstrap.CommandSocket
{
    /// <summary>
    /// Управляющий сокет
    /// </summary>
    public static class CommandSocket
    {
        #region Вспомогательные типы
        private class ReceivedCommand
        {
            public byte[] Datagram;
            public IPEndPoint From;
        }
        #endregion

        #region Частные данные
        /// <summary>
        /// Внутренний список известных системе команд
        /// </summary>
        private static readonly Dictionary<char, BaseCommand> _cmdlist = new Dictionary<char, BaseCommand>();
        
        /// <summary>
        /// UDP сокет для отсылки и приема административных команд
        /// </summary>
        private static UdpClient _udp;
        
        /// <summary>
        /// Количество текущих выполняемых команд
        /// </summary>
        private static int _activecmdcnt;

        /// <summary>
        /// Событие, определяющее что в настоящий момент времени нет исполняющихся команд
        /// </summary>
        private static readonly ManualResetEvent _cmdstate = new ManualResetEvent(true);

        /// <summary>
        /// Разрешение приема и обработки команд
        /// </summary>
        private static bool _enabled = true;

        /// <summary>
        /// Уникальный сессионный отпечаток
        /// </summary>
        private static UInt32 _mysenderhash;
        #endregion

        #region Конструкторы
        /// <summary>
        /// Создает административный сокет
        /// </summary>
        public static void Create()
        {
            _mysenderhash = (UInt32)Guid.NewGuid().GetHashCode();

            var ipe = new IPEndPoint(IPAddress.Any, 3275);
            _udp = new UdpClient(ipe) {EnableBroadcast = true};

            _cmdlist.Add('A', new PeerCommands.BootstrapDiscoveryCmd());
            _cmdlist.Add('B', new PeerCommands.PeerMembershipReqCmd());
            _cmdlist.Add('C', new PeerCommands.PeerHostConfCmd());
            _cmdlist.Add('E', new PeerCommands.NeighDiscCmd());

            var aep = (from a in ChanConfig.SelfIPAddresses select new ReceivedEndPoint { Channel = "$ADM", EndPoint = new IPEndPoint(a, 3275) }).ToArray();

            EndPointsList.UpdateHostEndpoints(Config.WorkstationName, aep, false);

            StartReceive();
        }
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Уникальный сессионный отпечаток
        /// </summary>
        public static UInt32 MySenderHash
        {
            get { return _mysenderhash; }
        }

        /// <summary>
        /// Возвращает порт административного сокета этого хоста
        /// </summary>
        public static int MyAdmPort
        {
            get
            {
// ReSharper disable PossibleNullReferenceException
                return ((IPEndPoint) _udp.Client.LocalEndPoint).Port;
// ReSharper restore PossibleNullReferenceException
            }
        }
        #endregion

        #region Публичные методы
        /// <summary>
        /// Запрещает дальнейшую обработку административных команд
        /// </summary>
        /// <remarks>
        /// Используется при подготовке к завершению процесса.
        /// Блокируется до окончания выполнения всех команд, находящихся в процессе выполнения
        /// </remarks>
        internal static void DisableCommandProcessing()
        {
            _enabled = false;

            // Ожидаем пока все выполненняемые команды завершатся
            _cmdstate.WaitOne();
        }
        #endregion

        #region Обработка команд
        /// <summary>
        /// Обработка принятых команд
        /// </summary>
        private static void CommandParser(object State)
        {
            try
            {
                _cmdstate.Reset();
                Interlocked.Increment(ref _activecmdcnt);

                if (!_enabled)
                    return;

                var cmd = (ReceivedCommand)State;

                var ms = new MemoryStream(cmd.Datagram);
                if (ms.Length == 0)
                    return;

                var rdr = new BinaryReader(ms);

                var cc = (char)rdr.ReadByte();

                BaseCommand bc;

                if (!_cmdlist.TryGetValue(cc, out bc))
                    return;

                string ll;

                bc.Invoke(rdr, cmd.From, out ll);

                if (!string.IsNullOrWhiteSpace(ll))
                    Logger.WriteLine("peer", string.Format("<<<({0}) : {1}", cmd.From, ll));
            }
            finally
            {
                if (Interlocked.Decrement(ref _activecmdcnt) == 0)
                    _cmdstate.Set();
            }
        }
        #endregion

        #region Сетевой обмен
        /// <summary>
        /// Инициализация приема команд
        /// </summary>
        private static void StartReceive()
        {
            while (true)
            {
                try
                {
                    _udp.BeginReceive(Received, null);
                    return;
                }
                catch (SocketException) { }
            }
        }

        /// <summary>
        /// Прием команды и постановка в очередь исполнения
        /// </summary>
        /// <param name="res">Интерфейс асинхронного вызова</param>
        private static void Received(IAsyncResult res)
        {
            try
            {
                var ipe = new IPEndPoint(IPAddress.Any, 0);
                var binary = _udp.EndReceive(res, ref ipe);

                if (_enabled)
                    ThreadPool.QueueUserWorkItem(CommandParser, new ReceivedCommand { Datagram = binary, From = ipe });
            }
            catch (SocketException) { }
            finally
            {
                if (_enabled)
                    StartReceive();
            }
        }

        /// <summary>
        /// Отправляет команду произвольному хосту
        /// </summary>
        /// <param name="Command">Команда</param>
        /// <param name="EndPoint">Адресат</param>
        public static void SendCommand(byte[] Command, IPEndPoint EndPoint)
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    _udp.Send(Command, Command.Length, EndPoint);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e);
                }                
            }
        }

        /// <summary>
        /// Отправляет команду
        /// </summary>
        /// <param name="Command">Команда</param>
        public static void SendCommand(byte[] Command)
        {
            var l = ChanConfig.BroadcastAddresses.Union(EndPointsList.GetHostsEndPoints());

            foreach (var ipe in l)
                SendCommand(Command, ipe);
        }
        #endregion
    }
}
