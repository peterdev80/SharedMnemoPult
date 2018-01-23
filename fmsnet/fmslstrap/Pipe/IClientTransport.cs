using System;
using System.IO;

namespace fmslstrap.Pipe
{
    internal delegate void UpdateStatistics(uint Received, uint Sended);

    internal delegate void Received(Stream Data, Action<byte[]> Reply);

    internal interface IClientTransport
    {
        /// <summary>
        /// Данные приняты
        /// </summary>
        event Received Received;

        /// <summary>
        /// Канал закрылся
        /// </summary>
        event Action Closed;

        /// <summary>
        /// Отправка данных
        /// </summary>
        /// <param name="Data">Данные для отправки</param>
        void Send(byte[] Data);

        /// <summary>
        /// Закрыть канал
        /// </summary>
        void Close();

        /// <summary>
        /// Обновление статистики
        /// </summary>
        event UpdateStatistics UpdateStats;
    }
}
