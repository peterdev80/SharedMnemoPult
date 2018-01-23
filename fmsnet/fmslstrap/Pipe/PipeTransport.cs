using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace fmslstrap.Pipe
{
    /// <summary>
    /// Транспорт обмена данными с клиентами на базе именованных каналов
    /// </summary>
    internal class PipeTransport : IClientTransport
    {
        #region Частные данные
        /// <summary>
        /// Разрешено подключение клиентов
        /// </summary>
        private static bool _acceptconnections = true;

        /// <summary>
        /// Событие приема данных от клиента
        /// </summary>
        public event Received Received;

        /// <summary>
        /// Событие отключения клиента от транспорта
        /// </summary>
        public event Action Closed;
        
        /// <summary>
        /// Обновление статистики
        /// </summary>
        public event UpdateStatistics UpdateStats;

        /// <summary>
        /// Асинхронный именованный канал связи с клиентом
        /// </summary>
        private readonly NamedPipeServerStream _wserver;

        /// <summary>
        /// Cинхронный именованный канал связи с клиентом
        /// </summary>
        private NamedPipeServerStream _server;

        /// <summary>
        /// Объект синхронизации системного канала
        /// </summary>
        private readonly Mutex _chanmutex = new Mutex();

        /// <summary>
        /// Триггер выдачи пакетов из очереди клиентам
        /// </summary>
        private readonly AutoResetEvent _dsa = new AutoResetEvent(false);

        /// <summary>
        /// Дескриптор метода триггера выдачи пакетов из очереди
        /// </summary>
        private RegisteredWaitHandle _dsahandle;

        /// <summary>
        /// Поток отправки данных клиенту занят
        /// </summary>
        private bool _stbusy;

        /// <summary>
        /// Очередь принятых извне пакетов
        /// </summary>
        private readonly Queue<byte[]> _dq = new Queue<byte[]>();

        /// <summary>
        /// Список активных каналов
        /// </summary>
        private static readonly List<PipeTransport> _activetransports = new List<PipeTransport>();
        #endregion

        #region Публичные методы
        public static void Init()
        {
            // Одновременно сущетсвует 4 ожидающих подключения экземпляра
            // для снижения риска возникновения гонки при массовом подключении на старте
            for (var i = 0; i < 4; i++)
                New();
        }

        private static void New()
        {
            lock (_activetransports)
            {
                if (!_acceptconnections)
                    return;

                _activetransports.Add(new PipeTransport());
            }
        }

        public static void ShutdownAllPipes()
        {
            IEnumerable<PipeTransport> pfc;
            
            lock (_activetransports)
            {
                _acceptconnections = false;

                pfc = _activetransports.ToArray();
            }

            foreach (var t in pfc)
                t.Close();

        }
        #endregion

        #region Конструкторы
        private PipeTransport()
        {
            while (true)
            {
                try
                {
                    _wserver = new NamedPipeServerStream("fmschanpipe", PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 262144, 262144);
                    _wserver.BeginWaitForConnection(StartReceive, null);
                    return;
                }
                catch (IOException) { }
                catch (InvalidOperationException) { }

                try
                {
                    _wserver.Dispose();
                }
                catch (SystemException) { }
            }
        }
        #endregion

        #region Подключение клиента
        /// <summary>
        /// Ожидание подключения
        /// </summary>
        private void StartReceive(IAsyncResult ar)
        {
            try
            {
                _wserver.EndWaitForConnection(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            lock (_activetransports)
            {
                if (!_acceptconnections)
                {
                    _activetransports.Remove(this);

                    return;
                }
            }

            // Создаем новый экземпляр, который будет ожидать подключения следующего клиента
            New();

            // Т.к. wserver, созданный в конструкторе, асинхронный 
            // а для записи в канал гораздо лучше синхронный
            // создаем новый синхронный сервер на его основе
            _server = new NamedPipeServerStream(PipeDirection.InOut, false, true, _wserver.SafePipeHandle);

            _dsahandle = ThreadPool.RegisterWaitForSingleObject(_dsa, SendQueuedData, null, TimeSpan.FromMilliseconds(250), false);

            PipeManager.New(this);

            StartReceive(new byte[256]);
        }

        /// <summary>
        /// Старт приема данных от клиента подключения
        /// </summary>
        private void StartReceive(byte[] buffer)
        {
            try
            {
                _wserver.BeginRead(buffer, 0, buffer.Length, ReadIncoming, buffer);
            }
            catch (ObjectDisposedException) { }
        }
        #endregion

        #region Прием данных от клиента
        /// <summary>
        /// Начало чтения пакета данных от клиента
        /// </summary>
        private void ReadIncoming(IAsyncResult ar)
        {
            var rsm = false;
            var buffer = (byte[])ar.AsyncState;

            try
            {
                _chanmutex.WaitOne();
                rsm = ProceedIncoming(ar);
            }
            catch (AbandonedMutexException) { }
            finally
            {
                _chanmutex.ReleaseMutex();
            }

            if (rsm)
                StartReceive(buffer);
        }

        /// <summary>
        /// Обработка пакета данных клиента
        /// </summary>
        /// <returns>
        /// Состояние мьютекса
        /// </returns>
        private bool ProceedIncoming(IAsyncResult ar)
        {
            var buffer = (byte[])ar.AsyncState;

            var msgs = new MemoryStream();
            var readed = _wserver.EndRead(ar);

            // Если вернули 0 - значит клиент отвалился
            if (readed == 0)
            {
                // И нужно прибраться
                Close();
                return false;
            }

            msgs.Write(buffer, 0, readed);

            // Проверка и дочитывание недочитанного остатка сообщения клиента
            while (_wserver.IsConnected && !_wserver.IsMessageComplete)     // IsMessageComplete выдает исключение в отключенном состоянии
            {
                readed = _wserver.Read(buffer, 0, buffer.Length);
                msgs.Write(buffer, 0, readed);
            }

            if (!_wserver.IsConnected)
                return false;

            msgs.Seek(0, SeekOrigin.Begin);

            UpdateStats?.Invoke((uint)msgs.Length, 0);

            Received?.Invoke(msgs, Send);

            return true;
        }
        #endregion

        #region Отправка данных клиенту
        private void SendQueuedData(object state, bool timedout)
        {
            lock (this)
            {
                if (_stbusy)
                    return;

                _stbusy = true;
            }

            try
            {
                while (true)
                {
                    byte[] d;
                    lock (_dq)
                    {
                        if (_dq.Count == 0)
                            return;

                        d = _dq.Dequeue();
                    }

                    try
                    {
                        _chanmutex.WaitOne();

                        var l = d.Length;

                        _server.Write(d, 0, l);

                        UpdateStats?.Invoke(0, (uint)l);
                    }
                    catch (AbandonedMutexException) { }
                    catch (ObjectDisposedException) { }
                    catch (IOException) { }
                    finally
                    {
                        _chanmutex.ReleaseMutex();
                    }
                }
            }
            finally
            {
                _stbusy = false;
            }
        }

        public void Send(byte[] Data)
        {
            lock (_dq)
            {
                var c = _dq.Count;

                // Если накопилось более 2500 непринятых сообщений
                // самые старые удаляем
                if (c > 2500)
                    _dq.Dequeue();

                _dq.Enqueue(Data);

                // Если накопилось больше 300 сообщений - где-то имеются проблемы со скоростью приемки и разбором 
                // Перестаем дергать триггер отправки после каждого принятого сообщения
                if (c < 300)
                    _dsa.Set();
                else
                    Trace.WriteLine($"Велико количество сообщений в очереди на отправку: {c}");
            }
        }
        #endregion

        #region Закрытие канала
        public void Close()
        {
            _dsahandle?.Unregister(null);

            try
            {
                if (_wserver.IsConnected)
                    _wserver.Disconnect();

                _wserver.Close();
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }

            lock (_activetransports)
                _activetransports.Remove(this);

            Closed?.Invoke();
        }
        #endregion
    }
}
