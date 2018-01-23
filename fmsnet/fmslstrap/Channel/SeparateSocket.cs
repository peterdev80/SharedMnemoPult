using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace fmslstrap.Channel
{
    /// <summary>
    /// Временный TCP сокет для передачи потока
    /// </summary>
    internal class SeparateSocket
    {
        #region Частные данные
        /// <summary>
        /// Список активных TCP сокетов (для предотвращения преждевременной их сборки)
        /// </summary>
        // ReSharper disable once CollectionNeverQueried.Local
        private static readonly List<SeparateSocket> List = new List<SeparateSocket>();

        /// <summary>
        /// Регулятор максимального количества активных TCP сокетов
        /// </summary>
        private static readonly Semaphore _wh = new Semaphore(4, 4);

        /// <summary>
        /// TCP сокет
        /// </summary>
        private readonly TcpListener _tcp;

        /// <summary>
        /// Список клиентов, которые должны бы но еще не подключились
        /// </summary>
        private readonly List<EndPointEntry> _waitclients;

        private readonly DataPacket _data;
        #endregion

        private SeparateSocket(DataPacket Data, IEnumerable<EndPointEntry> Targets)
        {
            lock (List)
                List.Add(this);

            _waitclients = new List<EndPointEntry>(Targets);

            _data = Data;

            _tcp = new TcpListener(IPAddress.Any, 0);
            _tcp.Start();

            StartAccept();
        }

        private void StartAccept()
        {
            _tcp.BeginAcceptTcpClient(AcceptClient, null);
        }

        public static SeparateSocket Get(DataPacket Data, ICollection<EndPointEntry> EndPoints)
        {
            _wh.WaitOne();

            return new SeparateSocket(Data, EndPoints);
        }

        public UInt16 Port
        {
            get { return (UInt16)((IPEndPoint)_tcp.LocalEndpoint).Port; }
        }

        private void AcceptClient(IAsyncResult ar)
        {
            try
            {
                var client = _tcp.EndAcceptTcpClient(ar);

                var tst = client.GetStream();
                var bfi = new byte[2];
                tst.Read(bfi, 0, bfi.Length);

                var ipe = new IPEndPoint(((IPEndPoint)client.Client.RemoteEndPoint).Address, BitConverter.ToUInt16(bfi, 0));

                var epe = _waitclients.FirstOrDefault(e => e.EndPoint.Equals(ipe));

                if (epe != null)
                {
                    _waitclients.RemoveAt(0);
                    _data.AddTargetStream(tst, epe.AddSended);
                }

                if (_waitclients.Count > 0)
                {
                    StartAccept();
                    return;
                }

                _tcp.Stop();

                _data.RemoteReady(TransferComplete);
            }
            catch (IOException) { }
            catch (SocketException) { }
        }

        /// <summary>
        /// Передача потока завершена
        /// </summary>
        private void TransferComplete()
        {
            _wh.Release();

            lock (List)
                List.Remove(this);
        }
    }
}
