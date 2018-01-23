using System;
using System.IO;

namespace fmslapi.Channel
{
    /// <summary>
    /// Фиксированный интерфейс IChannel версии 1
    /// </summary>
    public interface IChannel1
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
        /// Покидает канал
        /// </summary>
        /// <remarks>
        /// После использования этого метода использование любых методов этого
        /// интерфейса будет приводить к исключению
        /// </remarks>
        void Leave();

        IManager1 ParentAPIManager { get; }

        /// <summary>
        /// Имя канала
        /// </summary>
        string Name { get; }
    }
}
