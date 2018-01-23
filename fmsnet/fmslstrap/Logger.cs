using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace fmslstrap
{
    public delegate void LogHandler(string Sender, String Message, DateTime LogTime);

    /// <summary>
    /// Внутренне представление записи журнала
    /// </summary>
    public class LogEntry
    {
        public DateTime LogTime;
        public string LogSender;
        public string LogString;
    }

    /// <summary>
    /// Общесистемный журнал
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Внутренний журнал
        /// </summary>
        private static readonly List<LogEntry> _log = new List<LogEntry>();

        private static event LogHandler OnLog;

        /// <summary>
        /// Добавляет запись в журнал
        /// </summary>
        /// <param name="Log">Текст записи журнала</param>
        public static void WriteLine(string Log)
        {
            WriteLine(null, Log);
        }

        /// <summary>
        /// Добавляет запись в журнал
        /// </summary>
        /// <param name="Sender">Отправитель записи журнала</param>
        /// <param name="Log">Текст записи журнала</param>
        /// <param name="Verbose">Запись только в режиме verbose</param>
        public static void WriteLine(string Sender, string Log, bool Verbose = false)
        {
            if (Verbose && !Config.Verbose)
                return;

            var now = DateTime.Now;

            if (string.IsNullOrWhiteSpace(Sender))
                Sender = "fmslstrap";
            
            lock (_log)
                _log.Add(new LogEntry { LogString = Log, LogSender = Sender, LogTime = now });

            Debug.WriteLine("{0} ({2}): {1}", now, Log, Sender);

            // Если есть подписчики -> вызываем в отдельном потоке
            // т.к. возможно они попытаются передать запись на сторону и, в случае неудачи,
            // поток придет сюда, что может привести к блокировке
            ThreadPool.QueueUserWorkItem(s => OnLog?.Invoke(Sender, Log, now));
        }

        public static IEnumerable<LogEntry> Subscribe(LogHandler OnLog)
        {
            Logger.OnLog += OnLog;

            lock (_log)
            {
                return _log.ToArray();
            }
        }

        public static void Unsubscribe(LogHandler OnLog)
        {
            Logger.OnLog -= OnLog;
        }
    }
}
