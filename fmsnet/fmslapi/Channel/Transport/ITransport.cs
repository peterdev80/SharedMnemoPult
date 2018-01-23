using System;
using System.IO;

namespace fmslapi.Channel.Transport
{
    /// <summary>
    /// Интерфейс обмена данными между клиентами и fmsldr
    /// </summary>
    internal interface ITransport
    {
        /// <summary>
        /// Данные приняты
        /// </summary>
        event Action<Stream> Received;

        /// <summary>
        /// Канал закрыт
        /// </summary>
        event Action Closed;

        /// <summary>
        /// Попытка подключения к серверу
        /// </summary>
        /// <param name="Timeout">Таймаут подключения</param>
        /// <returns>Результат подключения</returns>
        bool TryConnect(TimeSpan Timeout);

        /// <summary>
        /// Разрешение асинхронного приема данных из канала
        /// </summary>
        void EnableReceive();

        /// <summary>
        /// Синхронный прием данных из канала
        /// </summary>
        /// <returns>Принятая посылка</returns>
        byte[] Read();

        /// <summary>
        /// Отправка данных серверу
        /// </summary>
        /// <param name="Data">Отправляемые данные</param>
        void Send(byte[] Data);

        /// <summary>
        /// Закрытие канала
        /// </summary>
        void Close();
    }
}
