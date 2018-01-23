using System;
using System.IO;

namespace fmslapi.Channel
{
    /// <summary>
    /// Канал передач сообщений
    /// </summary>
    public interface IChannel : ICommonChannel
    {
        /// <summary>
        /// Отправляет сообщение в канал
        /// </summary>
        /// <param name="Message">Сообщение</param>
        void SendMessage(byte[] Message);

        void SendMessageStream(Stream Message);

        Stream SendMessageStream(string ToHost = "");

        /// <summary>
        /// Отправляет сообщение в канал
        /// </summary>
        /// <param name="Message">Сообщение</param>
        /// <param name="Length">Длина передаваемого фрагмента</param>
        void SendMessage(byte[] Message, int Length);

        /// <summary>
        /// Отправляет сообщение в канал
        /// </summary>
        /// <param name="Buffer">Указатель на область памяти</param>
        /// <param name="Length">Длина посылки</param>
        unsafe void SendMessage(byte* Buffer, int Length);

        /// <summary>
        /// Отправляет сообщение в канал
        /// </summary>
        /// <param name="Buffer">Указатель на область памяти</param>
        /// <param name="Length">Длина посылки</param>
        void SendMessage(IntPtr Buffer, int Length);

        /// <summary>
        /// Событие, возникающее при появлении новых данных в канале
        /// </summary>
        event DataReceived Received;

        /// <summary>
        /// Отправка сообщения конкретному хосту в канале
        /// </summary>
        /// <param name="Receiver">Имя получателя сообщения</param>
        /// <param name="Message">Сообщение</param>
        void SendMessageToReceiver(string Receiver, byte[] Message);

        /// <summary>
        /// Отправка сообщения конкретному хосту в канале
        /// </summary>
        /// <param name="Receiver">Имя получателя сообщения</param>
        /// <param name="Message">Сообщение</param>
        /// <param name="Length">Длина сообщения</param>
        void SendMessageToReceiver(string Receiver, IntPtr Message, int Length);

        /// <summary>
        /// Установка функции, контролирующей и фильтрующей поток принимаемых из канала сообщений
        /// </summary>
        /// <param name="Proxy">Метод, вызываемый для фильтрации</param>
        /// <returns>Метод, для передачи сообщения в дальнейшую обработку</returns>
        Action<ISenderChannel, byte[]> SetChannelReceiveProxy(DataReceived Proxy);

        Reorder.ReorderBase UsePacketOrder { get; set; }

        /// <summary>
        /// Проверка наличия удаленного хоста в канале
        /// </summary>
        /// <param name="Host">Имя хоста</param>
        /// <returns>Признак наличия хоста в канале</returns>
        bool IsHostInChannel(string Host);

        /// <summary>
        /// Синхронный прием данных из канала (методом опроса)
        /// </summary>
        bool SyncReceive { get; set; }

        /// <summary>
        /// Синхронный прием данных из канала (методом опроса)
        /// </summary>
        bool Receive(out ISenderChannel Sender, out ReceivedMessage Message);
    }
}
