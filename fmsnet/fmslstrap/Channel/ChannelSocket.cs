using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace fmslstrap.Channel
{
    /// <summary>
    /// Реализация обмена данными через UDP сокет канала
    /// </summary>
    internal class ChannelSocket
    {
        #region Частные данные

        private readonly Socket _udp;
        private readonly Socket _tcp;

        /// <summary>
        /// Делегат обработки принятых данных
        /// </summary>
        private Action<DataPacket> _received;

        /// <summary>
        /// Список TCP сокетов для отправки данных
        /// </summary>
        private readonly Dictionary<EndPoint, TcpClient> _sendsockets = new Dictionary<EndPoint, TcpClient>();

        /// <summary>
        /// Предпочитать TCP транспорт
        /// </summary>
        private bool _prefertcp;

        /// <summary>
        /// Запрет передачи потока
        /// </summary>
        private bool _disablestreaming;

        #endregion

        #region Конструкторы

        /// <summary>
        /// Создает новый сокет
        /// </summary>
        public ChannelSocket()
        {
            var ipe = new IPEndPoint(IPAddress.Any, 0);

            // Открываем пару tcp/udp сокетов обязательно с равным номером порта
            while (true)
            {
                try
                {
                    _udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _udp.Bind(ipe);
                    _udp.EnableBroadcast = true;

                    ipe.Port = ((IPEndPoint)_udp.LocalEndPoint).Port;
                    _tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _tcp.Bind(ipe);
                    _tcp.Listen(64);

                    break;
                }
                catch (SocketException)
                {
                    if (_udp.IsBound)
                        _udp.Close();

                    if (_tcp.IsBound)
                        _tcp.Close();

                    _udp.Close();
                    ipe.Port++;
                }
            }
        }

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Предпочитать TCP транспорт
        /// </summary>
        public bool PreferTCP
        {
            set => _prefertcp = value;
        }

        /// <summary>
        /// Запрет передачи потока
        /// </summary>
        public bool DisableStreaming
        {
            set => _disablestreaming = value;
        }

        /// <summary>
        /// Локальная конечная точка канала
        /// </summary>
        public IPEndPoint LocalEndPoint => (IPEndPoint)_udp.LocalEndPoint;

        #endregion

        #region Отправка данных

        /// <summary>
        /// Отправка данных в сокет
        /// </summary>
        /// <param name="Message">Данные</param>
        /// <param name="EndPoints">Адрес отправки</param>
        public void Send(DataPacket Message, ICollection<EndPointEntry> EndPoints)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            var mis = Message.IsStreamPacket;
            var msl = Message.Length;

            if (mis || msl > 1250 || _prefertcp)
            {
                // Подготовка и отправка пакета TCP
                var separate = (Message.Length > 65535 || mis) && !_disablestreaming;

                wr.Write(Message.Length);
                wr.Write(Message.OrderID);
                wr.Write(Message.SenderID);
                wr.Write(separate);

                if (separate)
                {
                    // В базовый канал отправляется только уведомление
                    // Само сообщение будет передано через отдельное обратное TCP подключение

                    if (!mis)
                        Message.ConvertToStream();

                    var sps = SeparateSocket.Get(Message, EndPoints);
                    wr.Write(sps.Port);
                }
                else
                    wr.Write(Message.Data);

                var msg = ms.ToArray();

                foreach (var ep in EndPoints)
                    SendTcp(msg, ep);
            }
            else
            {
                // Подготовка и отправка пакета UDP
                wr.Write((UInt16)0xf00d);           // Метка пакета
                wr.Write(Message.OrderID);          // Порядковый номер
                wr.Write(Message.SenderID);
                wr.Write(Message.Data);             // Сообщение

                var msg = ms.ToArray();

                foreach (var ep in EndPoints)
                    try
                    {
                        var l = msg.Length;
                        _udp.SendTo(msg, l, SocketFlags.None, ep.EndPoint);
                        ep.AddSended(l);
                    }
                    catch (SocketException) { }
            }
        }

        /// <summary>
        /// Отправка данных в сокет TCP
        /// </summary>
        /// <param name="Message">Данные</param>
        /// <param name="EndPoint">Адрес отправки</param>
        private void SendTcp(byte[] Message, EndPointEntry EndPoint)
        {
            TcpClient ssock;

            var ep = EndPoint.EndPoint;

            lock (_sendsockets)
            {
                _sendsockets.TryGetValue(ep, out ssock);

                if (ssock == null)
                {
                    ssock = new TcpClient();
                    try
                    {
                        ssock.Connect(ep);

                        ssock.SendTimeout = 250;     // 250 мс

                        ssock.Client.Send(BitConverter.GetBytes((UInt16)((IPEndPoint)_udp.LocalEndPoint).Port));
                    }
                    catch (SocketException e)
                    {
                        Logger.WriteLine("ChannelSocket", $"Ошибка создания TCP сокета для передачи на конечную точку {ep}.\r\n{e.Message}");

                        try
                        {
                            ssock.Close();
                        }
                        catch (SocketException) { }

                        return;
                    }

                    var fb = new byte[1];
                    var ss = ssock.GetStream();
                    ss.BeginRead(fb, 0, fb.Length, ar =>
                        {
                            try
                            {
                                ss.EndRead(ar);
                                //throw new InvalidOperationException("Приняты данные из транспорта только для записи");
                            }
                            catch (IOException) { }
                            catch (SocketException) { }

                            lock (_sendsockets)
                                _sendsockets.Remove((EndPoint)ar.AsyncState);
                        }, ep);

                    _sendsockets[ep] = ssock;
                }
            }

            lock (ssock)
            {
                try
                {
                    ssock.Client.Send(Message);
                }
                catch (SocketException e)
                {
                    Logger.WriteLine("ChannelSocket", $"Ошибка передачи данных в TCP сокет при передаче на конечную точку {ep}.\r\n{e.Message}");

                    lock (_sendsockets)
                        _sendsockets.Remove(ep);
                }
            }

            EndPoint.AddSended(Message.Length);
        }

        #endregion

        #region Прием данных

        /// <summary>
        /// Инициация приема данных из сокета
        /// </summary>
        public void BeginReceive(Action<DataPacket> received)
        {
            Debug.Assert(received != null, "received != null");

            _received = received;
            StartUdpReceive();
            StartTcpAccept();
        }

        #endregion

        #region Частные методы

        #region Отправка в TCP

        private class TcpState
        {
            public NetworkStream NS;
            public BinaryReader RD;
            public Socket Side;
            public byte[] Buffer;
            public int RemotePort;
        }

        private void StartTcpReceive(TcpState State)
        {
            try
            {
                var side = State.Side;

                if (!side.Connected)
                    return;

                if (State.NS == null)
                {
                    State.NS = new NetworkStream(side, false);
                    State.RD = new BinaryReader(State.NS);
                }

                var buffer = State.Buffer;
                State.NS.BeginRead(buffer, 0, buffer.Length, TcpEndReceive, State);
            }
            catch (SocketException) { }
            catch (IOException) { }             // Падает при разрыве TCP соединения
        }

        /// <summary>
        /// Старт ожидания подключения TCP
        /// </summary>
        private void StartTcpAccept()
        {
            _tcp.BeginAccept(EndAccept, null);
        }

        private void EndAccept(IAsyncResult ar)
        {
            try
            {
                var side = _tcp.EndAccept(ar);

                var b = new byte[2];
                if (side.Receive(b) == 2)
                    StartTcpReceive(new TcpState
                                    {
                                        Side = side,
                                        RemotePort = BitConverter.ToUInt16(b, 0),
                                        Buffer = new byte[4]
                                    });
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { return; }
            StartTcpAccept();
        }

        /// <summary>
        /// Прием посылки по транспорту TCP
        /// </summary>
        /// <param name="ar"></param>
        private void TcpEndReceive(IAsyncResult ar)
        {
            var state = (TcpState)ar.AsyncState;
            var ns = state.NS;
            var rd = state.RD;
            var side = state.Side;
            var buffer = state.Buffer;
            var ipe = (IPEndPoint)side.RemoteEndPoint;
            ipe.Port = state.RemotePort;

            try
            {
                var readed = ns.EndRead(ar);

                if (readed == 0)
                {
                    ns.Close();
                    return;
                }

                var msize = BitConverter.ToInt32(buffer, 0);

                var orderid = rd.ReadUInt32();
                var senderid = rd.ReadUInt32();

                var separate = rd.ReadBoolean();

                DataPacket p;

                if (separate)
                {
                    var sps = rd.ReadUInt16();

                    var spe = new IPEndPoint(ipe.Address, sps);
                    var tc = new TcpClient();
                    tc.Connect(spe);

                    var tst = tc.GetStream();

                    // Передаем порт конечной точки (не временного канала) для идентификации на той стороне
                    tst.Write(BitConverter.GetBytes((UInt16)((IPEndPoint)_udp.LocalEndPoint).Port), 0, sizeof(UInt16));

                    p = new DataPacket { Size = msize };

                    p.RemoteReady(null);            // Принятый из сети пакет может быть переправлен только локально

                    p.SetSourceStream(tst, null);
                }
                else
                {
                    var msg = rd.ReadBytes(msize);
                    p = new DataPacket { Data = msg };
                }

                p.ReceivedFrom = ipe;
                p.OrderID = orderid;
                p.SenderID = senderid;

                _received(p);
            }
            catch (IOException)
            {
                ns.Close();
                return;
            }

            StartTcpReceive(state);
        }

        #endregion

        #region Отправка в UDP

        /// <summary>
        /// Стартует прием данных по UDP транспорту
        /// </summary>
        private void StartUdpReceive()
        {
            // ЭТО КОСТЫЛЬ!
            // он НУЖЕН!
            while (true)
            {
                try
                {
                    var buffer = new byte[2048];
                    EndPoint ipe = new IPEndPoint(IPAddress.Any, 0);
                    _udp.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref ipe, UdpEndReceiveFrom, buffer);
                    return;
                }
                catch (SocketException) { }
                catch (ObjectDisposedException) { return; }
            }
        }

        /// <summary>
        /// Принимает пакет данных по UDP транспорту
        /// </summary>
        /// <param name="ar"></param>
        private void UdpEndReceiveFrom(IAsyncResult ar)
        {
            var buffer = (byte[])ar.AsyncState;

            EndPoint ipe = new IPEndPoint(IPAddress.Any, 0);

            int readed;
            try
            {
                readed = _udp.EndReceiveFrom(ar, ref ipe);
            }
            catch (SocketException) { StartUdpReceive(); return; }
            catch (ObjectDisposedException) { return; }

            var ms = new MemoryStream(buffer);
            var rd = new BinaryReader(ms);

            var food = rd.ReadUInt16();         // Метка
            if (food == 0xf00d)
            {
                var orderid = rd.ReadUInt32();      // Порядковый номер пакета при отправке
                var senderid = rd.ReadUInt32();     // Уникальный отпечаток отправителя
                var l = readed - 10;                 // Длина полезной нагрузки пакета
                var b = new byte[l];
                ms.Read(b, 0, l);

                var rp = new DataPacket
                         {
                             Data = b,
                             OrderID = orderid,
                             SenderID = senderid,
                             ReceivedFrom = (IPEndPoint)ipe
                         };

                _received(rp);
            }

            StartUdpReceive();
        }

        #endregion

        #endregion

        #region Публичные методы

        /// <summary>
        /// Закрывает физические транспорты
        /// </summary>
        public void Close()
        {
            if (_udp.IsBound)
                _udp.Close();

            if (_tcp.IsBound)
                _tcp.Close();
        }

        #endregion
    }
}