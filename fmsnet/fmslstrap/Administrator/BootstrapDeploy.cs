using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace fmslstrap.Administrator
{
    /// <summary>
    /// Распространение сборки обмена
    /// </summary>
    public static class BootstrapDeploy
    {
        #region Частные данные
        /// <summary>
        /// TCP сокет
        /// </summary>
        private static readonly TcpListener _tcp;

        /// <summary>
        /// Упакованное тело сборки и отладочной информации
        /// </summary>
        public static byte[] Strap = new byte[0];
        #endregion

        #region Конструкторы
        static BootstrapDeploy()
        {
            _tcp = new TcpListener(IPAddress.Any, 0);
            _tcp.Start();

            StartAccept();   
        }
        #endregion

        #region Упаковка сборки и отладочной информации
        public static void InitData(byte[] Assm, byte[] PDB)
        {
            var ms = new MemoryStream();
            var gz = new GZipStream(ms, CompressionMode.Compress);
            var wr = new BinaryWriter(gz);

            var la = Assm?.Length ?? 0;
            var pa = PDB?.Length ?? 0;

            wr.Write(la);
            wr.Write(pa);

            // ReSharper disable once AssignNullToNotNullAttribute
            if (la > 0) wr.Write(Assm);
            // ReSharper disable once AssignNullToNotNullAttribute
            if (pa > 0) wr.Write(PDB);
            gz.Flush();
            gz.Close();

            Strap = ms.ToArray();
        }
        #endregion

        #region Сетевой обмен
        private static void StartAccept()
        {
            _tcp.BeginAcceptTcpClient(AcceptClient, null);
        }

        private static void AcceptClient(IAsyncResult ar)
        {
            var client = _tcp.EndAcceptTcpClient(ar);
            StartAccept();

            var stream = client.GetStream();

            stream.Write(Strap, 0, Strap.Length);
            stream.Flush();
            stream.Close();
        }
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Локальный порт распространителя сборки
        /// </summary>
        public static int LocalPort
        {
            get
            {
                return ((IPEndPoint)_tcp.LocalEndpoint).Port;
            }
        }

        /// <summary>
        /// Наличие актуальных данных для распространения
        /// </summary>
        public static bool HasData
        {
            get
            {
                return Strap.Length > 0;
            }
        }
        #endregion
    }
}
