using System;
using System.Text;
using System.IO;
using System.Threading;

namespace fmslstrap.Pipe
{
    /// <summary>
    /// Разбор и выполнение команд командной строки
    /// </summary>
    internal class CmdLine
    {
        public static void Execute(Stream Stream)
        {
            var rdr = new BinaryReader(Stream, Encoding.UTF8);

            var cnt = rdr.ReadUInt16();

            var pars = new string[cnt];

            for (var i = 0; i < cnt; i++)
                pars[i] = rdr.ReadString();

            ThreadPool.QueueUserWorkItem(x => Execute(pars));
        }

        public static void Execute(string[] Params)
        {
            // ReSharper disable once InconsistentNaming
            Action<MemoryStream, BinaryWriter> FillData = null;

            var pl = Params.Length;
            string p1 = null;
            if (pl >= 2)
                p1 = Params[1].ToLower();

            if (pl >= 3 && p1 == "start")
            {
                var start = Params[2].ToLower();
                FillData = (s, w) =>
                {
                    w.Write((byte)'D');
                    w.Write(start);
                };
            }

            if (pl >= 3 && p1 == "lstart")
            {
                var lstart = Params[2].ToLower();
                FillData = (s, w) =>
                {
                    w.Write((byte)'F');
                    w.Write(lstart);
                };
            }

            if (pl >= 4 && p1 == "rstart")
            {
                var rhost = Params[2].ToLower();
                var rstart = Params[3].ToLower();
                FillData = (s, w) =>
                {
                    w.Write((byte)'G');
                    w.Write(rhost);
                    w.Write(rstart);
                };
            }

            if (pl >= 3 && p1 == "stop")
            {
                var stop = Params[2].ToLower();
                FillData = (s, w) =>
                {
                    w.Write((byte)'E');
                    w.Write(stop);
                };
            }

            if (pl >= 2 && p1 == "killall")
                FillData = (s, w) => w.Write((byte)'Y');

            if (pl >= 2 && p1 == "shutdown")
                FillData = (s, w) => w.Write((byte)'Z');

            if (FillData == null)
                return;

            var ms = new MemoryStream();
            var wrt = new BinaryWriter(ms);

            FillData(ms, wrt);

            AdmLocChannel.Send(ms.ToArray());
        }
    }
}
