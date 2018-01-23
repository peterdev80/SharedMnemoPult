using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Glue
{
    /// <summary>
    /// Виртуальный канал для организации связи клиент-сервер через Cross App Domain Remoting
    /// </summary>
    public class VirtualChannel : GlueBase
    {
        #region События
        /// <summary>
        /// Данные приняты
        /// </summary>
        public event Action<byte[]> Received;

        /// <summary>
        /// Канал закрыт
        /// </summary>
        public event Action Closed;
        #endregion

        #region Конструкторы
        public VirtualChannel()
        {
        }
        #endregion

        #region Публичные методы
        /// <summary>
        /// Отправка данных
        /// </summary>
        /// <param name="Data">Отправляемые данные</param>
        public void Send(byte[] Data)
        {
            if (Received != null)
                Received(Data);
        }

        /// <summary>
        /// Закрытие канала
        /// </summary>
        public void Close()
        {
            if (Closed != null)
                Closed();
        }
        #endregion
    }
}
