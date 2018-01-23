using System;
using System.IO;

namespace fmslapi.Channel
{
    /// <summary>
    /// Отправитель сообщения
    /// </summary>
    public interface ISenderChannel
    {
        string Sender
        {
            get;
        }

        /// <summary>
        /// Отправляет сообщение отправителю
        /// </summary>
        /// <param name="Message">Сообщение</param>
        void Reply(byte[] Message);

        /// <summary>
        /// Отправляет сообщение в отправителю
        /// </summary>
        /// <param name="Buffer">Указатель на область памяти</param>
        /// <param name="Length">Длина посылки</param>
        unsafe void Reply(byte* Buffer, int Length);

        /// <summary>
        /// Отправляет сообщение в отправителю
        /// </summary>
        /// <param name="Buffer">Указатель на область памяти</param>
        /// <param name="Length">Длина посылки</param>
        void Reply(IntPtr Buffer, int Length);

        void ReplyStream(Stream Data);

        Stream ReplyStream();
    }
}
