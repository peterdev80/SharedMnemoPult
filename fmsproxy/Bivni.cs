using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using fmslapi.Channel;

namespace fmsproxy
{
    public class Bivni : ModelProxy
    {
        public static event RUOHandler OnRUO;
        public static event RUDHandler OnRUD;

        protected override void ProcessIncomingUDP(ISenderChannel Sender, byte[] Data)
        {
            var br = new BinaryReader(new MemoryStream(Data));

            var X = br.ReadInt32();
            var R = br.ReadInt32();
            var Y = br.ReadInt32();

            var brake = br.ReadInt32() != 0;
            var accel = br.ReadInt32() != 0;
            var down = br.ReadInt32() != 0;
            var up = br.ReadInt32() != 0;
            var left = br.ReadInt32() != 0;
            var right = br.ReadInt32() != 0;

            if (OnRUO != null)
                OnRUO(X, R, Y);

            if (OnRUD != null)
                OnRUD(brake, accel, down, up, left, right);

            base.ProcessIncomingUDP(null, Data);
        }
    }

    public delegate void RUOHandler(int X, int R, int Y);
    public delegate void RUDHandler(bool Brake, bool Accel, bool Down, bool Up, bool Left, bool Right);
}
