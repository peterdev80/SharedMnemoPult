using System;
using System.IO;
using System.Diagnostics;

namespace fmslapi.Channel.Transport
{
    /// <summary>
    /// Канал связи с сервером через Cross App Domain Remoting
    /// </summary>
    internal class AppDomainTransport : ITransport
    {
        #region Частные данные
        /// <summary>
        /// Канал создан
        /// </summary>
        private bool _con;

        /// <summary>
        /// Объект междоменной связи
        /// </summary>
        private readonly object _glue;

        /// <summary>
        /// Метод отправки данных серверу
        /// </summary>
        private Action<byte[]> _send;
        #endregion

        #region События
        /// <summary>
        /// Данные приняты
        /// </summary>
        public event Action<Stream> Received;

        /// <summary>
        /// Канал закрыт
        /// </summary>
        public event Action Closed;
        #endregion

        #region Конструкторы
        public AppDomainTransport(object Glue)
        {
            _glue = Glue;
        }
        #endregion

        #region Публичные методы
        /// <summary>
        /// Подключение к серверу
        /// </summary>
        public bool TryConnect(TimeSpan Timeout) 
        {
            if (_con)
                return true;

            _con = true;

            _send = _glue.GetType().GetMethod("CreateVirtualChannel").Invoke(_glue, new object[] { new Action<byte[]>(Receive), new Action(Close) }) as Action<byte[]>;

            return true; 
        }

        /// <summary>
        /// Разрешение приема данных
        /// </summary>
        public void EnableReceive() { }

        /// <summary>
        /// Синхронный прием данных из канала
        /// </summary>
        /// <returns>Принятая посылка</returns>
        public byte[] Read()
        {
            return null;
        }

        /// <summary>
        /// Отправка данные
        /// </summary>
        /// <param name="Data">Данные для отправки</param>
        public void Send(byte[] Data) 
        {
            Debug.Assert(_send != null);

            _send(Data);
        }

        /// <summary>
        /// Закрытие канала
        /// </summary>
        public void Close()
        {
            Closed?.Invoke();
        }
        #endregion

        #region Частные методы
        /// <summary>
        /// Обработка принятых данных
        /// </summary>
        /// <param name="Data">Принятые данные</param>
        private void Receive(byte[] Data)
        {
            Received?.Invoke(new MemoryStream(Data));
        }
        #endregion
    }
}
