using System;
using System.Text;
using System.Windows;
using System.IO.Pipes;
using System.Threading;
using System.IO;

namespace fmsman.Formats
{
    public partial class ModelConsoleLog
    {
        private NamedPipeClientStream _cl;

        public ModelConsoleLog()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;

            ThreadPool.QueueUserWorkItem(readlog);
        }

        private void newpipe()
        {
            _cl = new NamedPipeClientStream(".", "fmsmodelconsole", PipeDirection.In);

            while (true)
            {
                try
                {
                    Thread.Sleep(50);
                    _cl.Connect(0);
                    break;
                }
                catch (InvalidOperationException) { }
                catch (TimeoutException) { }
                catch (IOException) { }
            }

            Dispatcher.BeginInvoke(new Action(() => Visibility = Visibility.Visible));
        }

        private void readlog(object state)
        {
            newpipe();

            var sr = new StreamReader(_cl, Encoding.GetEncoding(866));

            while (true)
            {
                var l = sr.ReadLine();

                if (l == null)
                {
                    _cl.Dispose();
                    newpipe();

                    sr = new StreamReader(_cl, Encoding.GetEncoding(866));

                    l = "-----------------------------------------------------------------------";
                }

                Dispatcher.BeginInvoke(new Action(() => { tb.AppendText(l + "\n"); tb.ScrollToEnd(); }));
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
