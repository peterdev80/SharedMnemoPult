using System;
using System.IO;

namespace fmslstrap.Tasks
{
    public static class LogConsoleWriter
    {
        private static bool _subscribed;
        private static StreamWriter _sw;

        public static void Start()
        {
            if (_subscribed)
                return;

            _subscribed = true;

            _sw = new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding)
                            {
                                AutoFlush = true
                            };


            var ll = Logger.Subscribe(OnLog);

            foreach (var l in ll)
                OnLog(l.LogSender, l.LogString, l.LogTime);

        }

        private static void OnLog(string Sender, string Message, DateTime Logtime)
        {
            // ReSharper disable once InterpolatedStringExpressionIsNotIFormattable
            var z = $"{Logtime:dd.MM.yyyy hh:mm:ss}: ({Sender:-16}) {Message}";
            _sw.WriteLine(z);
        }
    }
}
