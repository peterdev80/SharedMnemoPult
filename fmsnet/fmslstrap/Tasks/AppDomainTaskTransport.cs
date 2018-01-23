using System;
using fmslstrap.Pipe;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace fmslstrap.Tasks
{
    /// <summary>
    /// Связь с задачами через Cross App Domain Remoting
    /// </summary>
    internal class AppDomainTaskTransport : IClientTransport
    {
        #region Частные данные
        /// <summary>
        /// Метод отправки данных клиенту
        /// </summary>
        private readonly Action<byte[]> _s;

        /// <summary>
        /// Метод закрытия канала
        /// </summary>
        private readonly Action _close;

        /// <summary>
        /// Транспорт активен
        /// </summary>
        private bool _active;
        #endregion

        #region Конструкторы
        public AppDomainTaskTransport(object SendVirtualChannel, object ReceiveVirtualChannel)
        {
            Debug.Assert(SendVirtualChannel != null && ReceiveVirtualChannel != null);

            _s = Delegate.CreateDelegate(typeof(Action<byte[]>), SendVirtualChannel, "Send") as Action<byte[]>;
            _close = Delegate.CreateDelegate(typeof(Action), SendVirtualChannel, "Close") as Action;

            ReceiveVirtualChannel.GetType().GetEvent("Received").AddEventHandler(ReceiveVirtualChannel, new Action<byte[]>(OnReceive));

            _active = true;
        }
        #endregion

        #region События
        /// <summary>
        /// Приняты данные из виртуального канала
        /// </summary>
        public event Received Received;

        /// <summary>
        /// Канал закрыт клиентом
        /// </summary>
        public event Action Closed;

        /// <summary>
        /// Обновление статистики канала
        /// </summary>
        public event UpdateStatistics UpdateStats;
        #endregion

        #region Публичные методы
        /// <summary>
        /// Отправка данных клиенту
        /// </summary>
        /// <param name="Data">Данные для отправки</param>
        public void Send(byte[] Data)
        {
            if (!_active)
                return;

            ThreadPool.QueueUserWorkItem(x =>
                                         {
                                             _s(Data);

                                             UpdateStats?.Invoke(0, (uint)Data.Length);
                                         });
        }

        /// <summary>
        /// Закртытие канала сервером
        /// </summary>
        public void Close()
        {
            _active = false;

            _close();

            Closed?.Invoke();
        }

        #region Вспомогательные типы
        /// <summary>
        /// Накопитель ответов на запрос от клиента
        /// </summary>
        private class ReplyTank
        {
            private byte[] _data;
            private bool _hasdata;

            // ReSharper disable once ParameterHidesMember
            public void Add(byte[] Data)
            {
                Debug.Assert(_data == null);

                _data = Data;
                _hasdata = true;
            }

            public bool HasData => _hasdata;
            public byte[] Data => _data;
        }
        #endregion

        /// <summary>
        /// Обработка принятых данных
        /// </summary>
        /// <param name="Data">Принятые данные</param>
        private void OnReceive(byte[] Data)
        {
            if (!_active)
                return;

            if (Received == null)
                return;

            var r = new ReplyTank();

            Received(new MemoryStream(Data), r.Add);

            if (r.HasData)
                ThreadPool.QueueUserWorkItem(x => { Send(r.Data); });

            UpdateStats?.Invoke((uint)Data.Length, 0);
        }
        #endregion
    }
}
