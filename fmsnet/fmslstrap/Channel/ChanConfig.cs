using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;
using fmslstrap.CommandSocket.PeerCommands;

namespace fmslstrap.Channel
{
    /// <summary>
    /// Конфигурация канала обмена
    /// </summary>
    internal class ChanConfig
    {
        #region Делегаты
        /// <summary>
        /// Делегат подписки на получение данных в канале
        /// </summary>
        /// <param name="Channel">Канал, в который пришли данные</param>
        /// <param name="Packet">Принятый пакет</param>
        public delegate void DataReceived(ChanConfig Channel, DataPacket Packet);
        #endregion

        #region Конструкторы
        public ChanConfig(string ChannelName, ChannelSocket LocalEndPoint, ChannelType ChanType)
        {
            SubscribersCount = 0;
            _udp = LocalEndPoint;
            this.ChanType = ChanType;
            Name = ChannelName;

            var isadm = ChannelName.ToLowerInvariant() == "adm";

            if (_udp != null)
            {
                _udp.PreferTCP = isadm;
                _udp.DisableStreaming = isadm;
            }

            if (LocalEndPoint == null)
                return;

            var aep = (from a in SelfIPAddresses select new Tuple<string, IPEndPoint>(ChannelName, new IPEndPoint(a, LocalEndPoint.LocalEndPoint.Port))).ToArray();

            EndPointsList.AddHostEndpoints(Config.WorkstationName, aep);
        }

        private static bool CheckInterface(NetworkInterface Interface)
        {
            Logger.WriteLine("Network",
                $"CheckInterface: {Interface.Name}, {Interface.OperationalStatus}, {Interface.NetworkInterfaceType}", true);

            if (Interface.OperationalStatus != OperationalStatus.Up)
                return false;

            var it = Interface.NetworkInterfaceType;

            if (it == NetworkInterfaceType.Loopback || it == NetworkInterfaceType.Ppp)
                return false;

            if (!Config.GetBool("enablewifi") && it == NetworkInterfaceType.Wireless80211)
                return false;

            return true;
        }

        private static bool CheckNetwork(IPAddress Addr, Subnet[] Disabled, Subnet[] Enabled)
        {
            Logger.WriteLine("Network", $"CheckAddress: Address: {Addr}, AddressFamily: {Addr.AddressFamily}", true);

            if (Addr.AddressFamily != AddressFamily.InterNetwork)
                return false;

            if (Disabled.Any(s => s.IsAddressInSubnet(Addr)))
            {
                Logger.WriteLine("Network", $"CheckAddress: {Addr}. Deny by DisabledNetworks", true);
                return false;
            }

            if (Enabled.Any(s => s.IsAddressInSubnet(Addr)))
            {
                Logger.WriteLine("Network", $"CheckAddress: {Addr}. Allow by EnabledNetworks", true);
                return true;
            }

            Logger.WriteLine("Network", $"CheckAddress: {Addr}. en.Length = {Enabled.Length}", true);
            return Enabled.Length == 0;                                  // При пустом спиксе EnabledNetwork - разрешены все сети
        }

        static ChanConfig()
        {

            var en =
                Config.GetString("enablednetworks")
                      .Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => new Subnet(x)).ToArray();

            var dn =
                Config.GetString("disablednetworks")
                      .Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => new Subnet(x)).ToArray();
        
            foreach (var s in en)
                Logger.WriteLine("Network", $"EnabledNetwork: {s.Address}/{s.Mask}", true);

            foreach (var s in dn)
                Logger.WriteLine("Network", $"DisabledNetwork: {s.Address}/{s.Mask}", true);

            // Извлекает все подходящие и доступные на машине конечные точки IP
            var ip4s = (from intf in NetworkInterface.GetAllNetworkInterfaces()
                        where CheckInterface(intf)
                        from ip in intf.GetIPProperties().UnicastAddresses
                        where CheckNetwork(ip.Address, dn, en)
                        orderby intf.NetworkInterfaceType
                        select new { IP = ip, intf.NetworkInterfaceType }).ToArray();

            var cnt = ip4s.Length;

            SelfIPAddresses = new IPAddress[cnt];
            BroadcastAddresses = new IPEndPoint[cnt];

            for (var i = 0; i < cnt; i++)
            {
                var ia = ip4s[i].IP.Address.GetAddressBytes();
                var ma = ip4s[i].IP.IPv4Mask.GetAddressBytes();

                for (int j = 0; j < ia.Length; j++)
                    ia[j] |= (byte)(~ma[j]);

                BroadcastAddresses[i] = new IPEndPoint(new IPAddress(ia), 3275);
                SelfIPAddresses[i] = ip4s[i].IP.Address;
            }
        }
        #endregion

        #region Частные данные
        /// <summary>
        /// Сокет канала
        /// </summary>
        private readonly ChannelSocket _udp;

        /// <summary>
        /// Кеш задержанных сообщений
        /// </summary>
        private static readonly List<Tuple<ChanConfig, DataPacket>> _delayedcache = new List<Tuple<ChanConfig, DataPacket>>();

        private bool _active = true;
        private int _subscribersCount;

        #endregion

        #region Публичные свойства
        public static IPAddress[] SelfIPAddresses;

        /// <summary>
        /// Адрес широковещательной рассылки в домене
        /// </summary>
        public static IPEndPoint[] BroadcastAddresses;

        private static readonly char[] Separator = { '|', ';' };

        /// <summary>
        /// Имя канала
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Тип канала
        /// </summary>
        public ChannelType ChanType { get; private set; }

        /// <summary>
        /// Количество подпискчиков канала
        /// </summary>
        public int SubscribersCount
        {
            get { return Interlocked.Add(ref _subscribersCount, 0); }
            private set { Interlocked.Exchange(ref _subscribersCount, value); }
        }

        /// <summary>
        /// Предпочитать TCP транспорт
        /// </summary>
        public bool PreferTCP
        {
            set
            {
                _udp.PreferTCP = value;
            }
        }
        #endregion

        #region Управление хостами канала
        public void AddSubscriber()
        {
            Interlocked.Increment(ref _subscribersCount);
        }

        public void RemoveSubscriber()
        {
            Interlocked.Decrement(ref _subscribersCount);
        }
        #endregion

        #region Отправка данных

        /// <summary>
        /// Отправляет сообщение в канал
        /// </summary>
        /// <param name="Message">Бинарное сообщение</param>
        /// <param name="ToHost">Отправка только на указанных хост, если указано</param>
        /// <param name="OrderID">Порядковый номер</param>
        public void SendMessage(byte[] Message, string ToHost = null, uint OrderID = 0)
        {
            SendMessage(new DataPacket { Data = Message, OrderID = OrderID, SenderID = 0 }, ToHost);
        }

        /// <summary>
        /// Формирует список сетевых конечных точек для отправки пакета и пересылает его
        /// локальным клиентам при необходимости
        /// </summary>
        /// <remarks>
        /// Данные в канале переменных никогда не возвращаются локально (при необходимости это 
        /// делается самим каналом переменных).
        /// </remarks>
        /// <param name="RP"></param>
        /// <param name="ToHost">Хост назначения пакета</param>
        /// <returns>Список сетевых конечных точек для отправки пакета</returns>
        private ICollection<EndPointEntry> CheckSendTargets(DataPacket RP, string ToHost)
        {
            if (!_active)
                return null;

            // Пакеты, не отправляемые в сеть
            var lclo = ChanType == ChannelType.Local ||
                       string.Equals(ToHost, Config.WorkstationName, StringComparison.InvariantCultureIgnoreCase);
            
            // Пакеты, принимаемые как вернувшиеся
            var lcl = ChanType != ChannelType.Variables &&
                      (string.IsNullOrEmpty(ToHost) ||
                       string.Equals(ToHost, Config.WorkstationName, StringComparison.InvariantCultureIgnoreCase));

            if (lcl)
            {
                RaiseOnDataReceived(RP);

                if (lclo)
                {
                    RP.RemoteReady(null);

                    return null;
                }
            }

            var tgt = GetTargets(ToHost);

            if (tgt.Count == 0)
            {
                // Отсылать удаленно некому
                // Сразу выставляем признак готовности
                RP.RemoteReady(null);

                return null;
            }

            return tgt;
        }

        /// <summary>
        /// Отправляет сообщение в канал
        /// </summary>
        /// <param name="Packet">Отправляемый пакет</param>
        /// <param name="ToHost">Отправка только на указанных хост, если указано</param>
        public void SendMessage(DataPacket Packet, string ToHost)
        {
            var tgts = CheckSendTargets(Packet, ToHost);

            if (tgts != null)
                _udp.Send(Packet, tgts);
        }

        /// <summary>
        /// Формирует список сетевых конечных точек для указанного хоста назначения
        /// </summary>
        /// <remarks>
        /// Если хост указан и найден возвращается его сетевая конечная точка.
        /// Если хост не указан или не найден возвращается список всех сетевых конечных точек участиников
        /// этого канала.
        /// </remarks>
        /// <param name="ToHost">Хост назначения</param>
        /// <returns>Список сетевых конечных точек</returns>
        private ICollection<EndPointEntry> GetTargets(string ToHost)
        {
            if (ToHost != null)
            {
                var h = EndPointsList.GetEndPoint(ToHost, Name).ToArray();

                if (h.Length > 0)
                    return new[] { h[0] };
            }

            return EndPointsList.GetByChannel(Name).Where(ep => ep.Host != Config.WorkstationName && !ep.DontSendTo).ToArray();
        }

        #endregion

        #region Прием данных
        /// <summary>
        /// Стартует прием данных в канал
        /// </summary>
        public void StartReceive()
        {
            _udp.BeginReceive(OnReceive);
        }

        /// <summary>
        /// Прием посылки
        /// </summary>
        private void OnReceive(DataPacket Packet)
        {
            if (!_active)
                return;

            var epe = EndPointsList.GetByEndPoint(Packet.ReceivedFrom);

            if (epe == null)
            {
                lock (_delayedcache)
                    _delayedcache.Add(new Tuple<ChanConfig, DataPacket>(this, Packet));

                return;
            }

            Packet.Sender = epe.Host ?? "*";

            if (Packet.IsStreamPacket)
            {
                Packet.SetSourceUpdateStats(epe.AddReceived);

                RaiseOnDataReceived(Packet);

                Packet.LocalReady(null);
            }
            else
            {
                epe.AddReceived(Packet.Data.Length);

                // Вызываем подписчиков канала
                RaiseOnDataReceived(Packet);
            }
        }
        #endregion

        #region События
        public event Action<ChanConfig, ChannelChangeEventArgs> OnChannelChange;

        private event DataReceived OnDataRcv;

        public event DataReceived OnDataReceived
        {
            add 
            {
                lock (this)
                {
                    OnDataRcv += value;
                    UpdateSendTo();
                }
            }
            
            remove 
            {
                lock (this)
                {
                    OnDataRcv -= value;
                    UpdateSendTo();
                }
            }
        }

        public event Action<ChanConfig> OnClose;
        #endregion

        #region Вызов событий
        public void RaiseOnDataReceived(DataPacket Packet)
        {
            OnDataRcv?.Invoke(this, Packet);
        }

        public void RaiseNewHostInChannel(string NewHost)
        {
            OnChannelChange?.Invoke(this, new ChannelChangeEventArgs { ChangeType = ChannelChangeType.AddHost, NewHost = NewHost });
        }
        #endregion

        #region Задержанные посылки
        /// <summary>
        /// Отправляет задержанные ранее сообщения получателям
        /// </summary>
        public static void SendDelayedDatagrams()
        {
            var enq = new List<Tuple<ChanConfig, DataPacket>>();

            lock (_delayedcache)
                foreach (var t in _delayedcache.Where(x => EndPointsList.GetByEndPoint(x.Item2.ReceivedFrom) != null).ToArray())
                {
                    _delayedcache.Remove(t);
                    enq.Add(t);

                    Logger.WriteLine(string.Format("SendDelayedDatagram from {0}", t.Item2));
                }

            foreach (var t in enq)
            {
                var rp = t.Item2;

                var epe = EndPointsList.GetByEndPoint(rp.ReceivedFrom);
                if (epe == null)
                    continue;
                
                rp.Sender = t.Item1.Name;

                epe.AddReceived(rp.Data.Length);

                t.Item1.RaiseOnDataReceived(rp);
            }
        }
        #endregion

        #region Вспомогательные методы
        /// <summary>
        /// Запрещает дальнейшую отправку данных в канал
        /// </summary>
        public void Close()
        {
            _active = false;

            _udp?.Close();

            OnClose?.Invoke(this);
        }

        private void UpdateSendTo()
        {
            var epes = EndPointsList.GetEndPoint(Config.WorkstationName, Name);

            if (epes.Length == 0)
                return;

            foreach(var epe in epes)
                epe.DontSendTo = OnDataRcv == null || OnDataRcv.GetInvocationList().Length == 0;

            CommandSocket.CommandSocket.SendCommand(PeerHostConfCmd.GetCommand());
        }
        #endregion
    }
}
