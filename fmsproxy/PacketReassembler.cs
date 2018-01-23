using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fmslapi;

namespace fmsproxy
{
    public abstract class PacketReassembler 
    {
        protected ModelProxy _proxy;
        protected IConfigSection _config;

        public ModelProxy Proxy
        {
            set { _proxy = value; }
        }

        public IConfigSection Config
        {
            set { _config = value; }
        }

        public abstract void ReassemblyPacket(byte[] Data, Action<byte[]> Enqueue);
        public abstract void Init();
    }
}
