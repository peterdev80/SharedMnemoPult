using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using fmslstrap.Channel;
using System.IO;
using System.Threading;

namespace fmslstrap.CommandSocket.PeerCommands
{
    /// <summary>
    /// Канальная конфигурация хоста
    /// </summary>
    public class PeerHostConfCmd : BaseCommand
    {
        private static readonly Dictionary<UInt32, Int32> _sequences = new Dictionary<UInt32, Int32>();
        private static int _sequence = 1;
        private static readonly IEqualityComparer<EndPointEntry> _epc = new epc();

        public override void Invoke(BinaryReader Reader, IPEndPoint EndPoint, out string LogLine)
        {
            LogLine = null;

            var domain = Reader.ReadString();
            var hst = Reader.ReadString();

            var cc = Reader.ReadUInt16();
            var cl = new List<ReceivedEndPoint>();

            for (var i = 0; i < cc; i++)
            {
                var c = Reader.ReadString();
                var ipe = new IPEndPoint(EndPoint.Address, Reader.ReadUInt16());
                var dst = Reader.ReadBoolean();

                cl.Add(new ReceivedEndPoint { Channel = c, EndPoint = ipe, DontSendTo = dst });
            }

            var sequence = Reader.ReadInt32();
            var senderhash = Reader.ReadUInt32();

            if (domain != Config.DomainName)
                return;                                 // Посылки в другой домен отбрасываются

            if (hst == Config.WorkstationName)
                return;                                 // Принятый свой же пакет отбрасывается

            lock (_sequences)
            {
                int ls;

                if (_sequences.TryGetValue(senderhash, out ls))
                    if (sequence <= ls)
                        return;

                _sequences[senderhash] = sequence;
                
                EndPointsList.UpdateHostEndpoints(hst, cl.ToArray(), true);
            }

            LogLine = string.Format(@"->PEERHOSTCONF<- Domain: {0}; Host: {1}; Channels: {2}; Seq: {3}; SenderHash: {4:X8};",
                domain, hst, string.Join("", cl.Select(x => string.Format("({0}:{1}{2})", x.Channel, x.EndPoint, x.DontSendTo ? "*" : ""))), sequence, senderhash);

            ChanConfig.SendDelayedDatagrams();
        }

        public static byte[] GetCommand()
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write((byte)'C');
            wr.Write(Config.DomainName);
            wr.Write(Config.WorkstationName);

            var ca = EndPointsList.GetByHost(Config.WorkstationName).Distinct(_epc).ToArray();
            wr.Write((UInt16)ca.Length);

            foreach (var c in ca)
            {
                wr.Write(c.Channel);
                wr.Write((UInt16)c.EndPoint.Port);
                wr.Write(c.DontSendTo);
            }

            wr.Write(Interlocked.Increment(ref _sequence));

            wr.Write(CommandSocket.MySenderHash);

            return ms.ToArray();
        }

        // ReSharper disable once InconsistentNaming
        private class epc : IEqualityComparer<EndPointEntry>
        {
            public bool Equals(EndPointEntry x, EndPointEntry y)
            {
                return x.Channel.Equals(y.Channel);
            }

            public int GetHashCode(EndPointEntry obj)
            {
                return obj.Channel.GetHashCode();
            }
        }
    }
}
