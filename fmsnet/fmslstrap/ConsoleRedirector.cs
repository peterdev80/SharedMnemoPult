using System;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using fmslstrap.Channel;
// ReSharper disable InconsistentNaming

namespace fmslstrap
{
    internal class ConsoleRedirector
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int SetStdHandle(int device, IntPtr handle);

        const int STD_OUTPUT_HANDLE = -11;
        const int STD_ERROR_HANDLE = -12;

        private static NamedPipeServerStream _con;
        // ReSharper disable once RedundantDefaultMemberInitializer
        private static uint _oid = 0;

        // ReSharper disable once RedundantNameQualifier
        private static Channel.ChanConfig _conchan;

        public static void Start()
        {
            _con = new NamedPipeServerStream("fmsconsole_stdout", PipeDirection.InOut, -1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough, 512, 512);

            ThreadPool.QueueUserWorkItem(x => ConnectOut());

            _con.WaitForConnection();

            SetStdHandle(STD_ERROR_HANDLE, _con.SafePipeHandle.DangerousGetHandle());
            SetStdHandle(STD_OUTPUT_HANDLE, _con.SafePipeHandle.DangerousGetHandle());
        }

        public static void SetConsoleChannel(ChanConfig ConsoleChannel)
        {
            _conchan = ConsoleChannel;
        }
       
        private static void ConnectOut()
        {
            Thread.Sleep(200);

            var c = new NamedPipeClientStream(".", "fmsconsole_stdout", PipeDirection.InOut, PipeOptions.WriteThrough);

            c.Connect(1000);

            var buf = new byte[512];

            while (true)
            {
                var readed = c.Read(buf, 0, buf.Length);
                if (readed == 0)
                    break;

                _conchan?.SendMessage(buf.Take(readed).ToArray(), OrderID: _oid++);
            }
        }
    }
}
