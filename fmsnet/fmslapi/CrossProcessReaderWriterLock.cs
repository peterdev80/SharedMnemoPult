using System;
using System.Threading;

namespace fmslapi
{
    /// <summary>
    /// Межпроцессная потоковая блокировка
    /// </summary>
    internal unsafe class CrossProcessReaderWriterLock
    {
        #region Частные данные

        /// <summary>
        /// Указатель на счетчик блокировки
        /// </summary>
        private readonly int* _counter;

        /// <summary>
        /// Установлена блокировка на запись
        /// </summary>
        private bool _inwritelock;

        #endregion

        #region Конструкторы

        public CrossProcessReaderWriterLock(void* Counter)
        {
            _counter = (int*)Counter;
        }

        #endregion

        /// <summary>
        /// Вход в блокировку на запись
        /// </summary>
        public void EnterWriteLock()
        {
            // ReSharper disable once EmptyEmbeddedStatement
            while (Interlocked.CompareExchange(ref *_counter, -1, 0) != 0) ;

            _inwritelock = true;
        }

        /// <summary>
        /// Выход из блокировки на запись
        /// </summary>
        public void ExitWriteLock()
        {
            if (!_inwritelock && Interlocked.Add(ref *_counter, 0) == -1)
                throw new InvalidOperationException("Попытка снять чужую блокировку для записи");

            _inwritelock = false;

            var v = Interlocked.CompareExchange(ref *_counter, 0, -1);

            if (v != -1)
                throw new InvalidOperationException("Блокировка не была заблокирована для записи");
        }

        /// <summary>
        /// Вход в блокировку на чтение
        /// </summary>
        public void EnterReadLock()
        {
            int v;
            do
            {
                v = Interlocked.Add(ref *_counter, 0);
            } while (v < 0 || v != Interlocked.CompareExchange(ref *_counter, v + 1, v));
        }

        /// <summary>
        /// Выход из блокировки на чтение
        /// </summary>
        public void ExitReadLock()
        {
            var l = Interlocked.Add(ref *_counter, 0);

            if (l == 0)
                throw new InvalidOperationException("Попытка снять чужую блокировку для чтения");

            if (l == -1)
                throw new InvalidOperationException("Попытка снять блокировку для записи как блокировку для чтения");

            Interlocked.Decrement(ref *_counter);
        }
    }
}
