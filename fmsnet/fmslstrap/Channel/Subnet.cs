using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace fmslstrap.Channel
{
    public class Subnet
    {
        private readonly IPAddress _addr;
        private readonly IPAddress _mask;

        public IPAddress Address => _addr;
        public IPAddress Mask => _mask;

        public Subnet(string Net)
        {
            var ips = Net.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var s = ips[0];
            for (var i = s.Count(x => x == '.'); i < 3; i++)
                s += ".0";

            _addr = IPAddress.Parse(s);

            if (ips.Length == 1)
                _mask = IPAddress.Parse("255.255.255.255");
            else
            {
                var mp = int.Parse(ips[1]);
                var mb = _addr.GetAddressBytes();

                var bp = mp / 8;
                for (var i = 0; i < bp; i++)
                    mb[i] = 0xff;
                for (var i = bp; i < mb.Length; i++)
                    mb[i] = 0;

                var bpo = 8 - mp % 8;

                if (bpo < 8)
                    mb[bp] = (byte)(0xff - ((1U << bpo) - 1));

                _mask = new IPAddress(mb);
            }
        }

        public bool IsAddressInSubnet(IPAddress Addr)
        {
            if (_addr.AddressFamily != Addr.AddressFamily)
                return false;

            if (_addr.AddressFamily != AddressFamily.InterNetwork)
                return false;

            var am = _addr.GetAddressBytes();
            var mb = _mask.GetAddressBytes();
            var cb = Addr.GetAddressBytes();

            am = am.Zip(mb, (x, y) => (byte)(x & y)).ToArray();
            cb = cb.Zip(mb, (x, y) => (byte)(x & y)).ToArray();

            return !am.Where((t, i) => t != cb[i]).Any();
        }

        public int ByteLength => _addr.GetAddressBytes().Length;

        public byte[] GetAddressBytes()
        {
            return _addr.GetAddressBytes();
        }

        public byte[] GetMaskBytes()
        {
            return _mask.GetAddressBytes();
        }
    }
}
