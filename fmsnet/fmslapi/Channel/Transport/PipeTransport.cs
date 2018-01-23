using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace fmslapi.Channel.Transport
{
    /// <summary>
    /// Связь с сервером с использованием именованных каналов
    /// </summary>
    internal class PipeTransport : ITransport
    {
        #region Частные данные
        /// <summary>
        /// Асинхронный объект связи с сервером
        /// </summary>
        /// <remarks>
        /// Асинхронно происходит чтение команд сервера. Хотя и используются блокирующие методы - 
        /// чтение всеравно происходит асинхронно.
        /// </remarks>
        private readonly NamedPipeClientStream _client;

        /// <summary>
        /// Синхронный объект связи с сервером
        /// </summary>
        /// <remarks>
        /// Все операции отсылки данных и команд производятся синхронно
        /// </remarks>
        private NamedPipeClientStream _aclient;

        /// <summary>
        /// Потоковая блокировка записи в канал
        /// </summary>
        private readonly Mutex _writermutex = new Mutex(false);

        /// <summary>
        /// Данные приняты
        /// </summary>
        public event Action<Stream> Received;

        /// <summary>
        /// Канал закрыт
        /// </summary>
        public event Action Closed;

        private bool _isasync;
        #endregion

        #region Констукторы
        public PipeTransport()
        {
            _client = new NamedPipeClientStream(".", "fmschanpipe", PipeDirection.InOut,
                                   PipeOptions.Asynchronous | PipeOptions.WriteThrough);
        }
        #endregion

        #region Публичные методы
        /// <summary>
        /// Попытка подключения к серверу
        /// </summary>
        /// <param name="Timeout">Таймаут ожидания подключения</param>
        /// <returns>Результат подключения</returns>
        public bool TryConnect(TimeSpan Timeout)
        {
            var cnt = 10;            // Количество неудачных попыток подключения перед перевыбросом исключения

            var tmo = (int)Timeout.TotalMilliseconds;

            while (true)
            {
                try
                {
                    _client.Connect(tmo);

                    _aclient = new NamedPipeClientStream(PipeDirection.InOut, false, true, _client.SafePipeHandle);

                    _client.ReadMode = PipeTransmissionMode.Message;

                    return true;
                }
                catch (Exception)
                {
                    if (--cnt == 0)
                        return false;

                    if (cnt >= 5) 
                        continue;

                    Thread.Sleep(20);

                    if (tmo == 0)
                        tmo = 20;
                }
            }
        }

        /// <summary>
        /// Разрешение приема данных
        /// </summary>
        public void EnableReceive()
        {
            _isasync = true;
            StartReceive();
        }

        /// <summary>
        /// Синхронный прием данных из канала
        /// </summary>
        /// <returns>Принятая посылка</returns>
        public byte[] Read()
        {
            if (_isasync)
                return null;

            var buffer = new byte[512];
            var ms = new MemoryStream();

            var readed = _client.Read(buffer, 0, buffer.Length);

            if (readed == 0)
            {
                Close();
                return null;
            }

            ms.Write(buffer, 0, readed);

            // Проверка и дочитывание недочитанного остатка сообщения клиента
            while (_client.IsConnected && !_client.IsMessageComplete)     // IsMessageComplete выдает исключение в отключенном состоянии
            {
                readed = _client.Read(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, readed);
            }

            if (!_client.IsConnected)
            {
                Close();
                return null;
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Отправка данных
        /// </summary>
        /// <param name="Data">Данные для отправки</param>
        public void Send(byte[] Data)
        {
            try
            {
                _writermutex.WaitOne();

                //aclient.WaitForPipeDrain();
                _aclient.Write(Data, 0, Data.Length);
            }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
            finally
            {
                _writermutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Закрытие канала
        /// </summary>
        public void Close()
        {
            _client.Dispose();

            Closed?.Invoke();
        }
        #endregion

        #region Частные методы
        /// <summary>
        /// Начинает прием команды сервера
        /// </summary>
        /// <param name="ar">Интерфейс асинхронного вызова</param>
        private void ReadIncoming(IAsyncResult ar)
        {
            var buffer = (byte[])ar.AsyncState;

            var ms = new MemoryStream();

            try
            {
                _writermutex.WaitOne();

                var readed = _client.EndRead(ar);

                if (readed == 0)
                {
                    Close();
                    return;
                }

                ms.Write(buffer, 0, readed);

                // Проверка и дочитывание недочитанного остатка сообщения клиента
                while (_client.IsConnected && !_client.IsMessageComplete)     // IsMessageComplete выдает исключение в отключенном состоянии
                {
                    readed = _client.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, readed);
                }

                if (!_client.IsConnected)
                {
                    Close();
                    return;
                }

                StartReceive();
            }
            finally
            {
                try
                {
                    _writermutex.ReleaseMutex();
                }
                catch (ApplicationException) { }
            }

            ms.Seek(0, SeekOrigin.Begin);

            Received?.Invoke(ms);
        }

        /// <summary>
        /// Стартует прием команд от сервера
        /// </summary>
        private void StartReceive()
        {
            var buffer = new byte[128];
            _client.BeginRead(buffer, 0, buffer.Length, ReadIncoming, buffer);
        }
        #endregion
    }
}
