using System;
using System.Text;
using System.IO.Pipes;
using System.IO;

namespace fmsldr
{
    internal class Control
    {
        public static bool CheckParams(string[] Params)
        {
            var client = new NamedPipeClientStream(".", "fmschanpipe", PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);

            var attempts = 4;

            while (attempts-- > 0)
            {
                try
                {
                    client.Connect(25);

                    client.ReadMode = PipeTransmissionMode.Message;
                    var ms = new MemoryStream();
                    var wrt = new BinaryWriter(ms, Encoding.UTF8);

                    wrt.Write('Y');

                    wrt.Write((UInt16)Params.Length);
                    foreach (var p in Params)
                        wrt.Write(p);

                    var buf = ms.ToArray();
                    client.Write(buf, 0, buf.Length);

                    client.Dispose();

                    return true;
                }
                catch (TimeoutException) { }
            }

            return false;
        }
    }
}
