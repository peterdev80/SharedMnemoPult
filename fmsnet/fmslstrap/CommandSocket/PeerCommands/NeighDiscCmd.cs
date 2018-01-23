using System.Linq;
using System.Net;
using fmslstrap.Channel;
using System.IO;
using System.Threading;

namespace fmslstrap.CommandSocket.PeerCommands
{
    /// <summary>
    /// Поиск соседей в домене
    /// </summary>
    public class NeighDiscCmd : BaseCommand
    {
        private static Timer _polltimer;
        private static int _pollcnt = 3;

        public override void Invoke(BinaryReader Reader, IPEndPoint EndPoint, out string LogLine)
        {
            LogLine = null;

            var domain = Reader.ReadString();
            var name = Reader.ReadString();
            var known = Reader.ReadString();
            var sknown = known.Split(',');

            if (domain != Config.DomainName)            // Пакеты из другого домена отбрасываем
                return;

            if (name == Config.WorkstationName)         // Свои пакеты отбрасываем
                return; 

            LogLine = string.Format(@"->PEERNEIGHBOURDISCOVERY<- Domain: {0}; Name: {1}; I know this guys: {2};", domain, name, known);

            var heknowme = sknown.Any(kn => kn == Config.WorkstationName);
            
            if (!heknowme)
                CommandSocket.SendCommand(PeerHostConfCmd.GetCommand(), EndPoint);
        }

        public static byte[] GetCommand()
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write((byte)'E');
            wr.Write(Config.DomainName);
            wr.Write(Config.WorkstationName);
            wr.Write(string.Join(",", EndPointsList.GetHosts()));

            return ms.ToArray();
        }

        public static void InitNeighbourDiscovery()
        {
            _polltimer = new Timer(PeerSendPoll, null, 100, 500);
        }

        private static void PeerSendPoll(object State)
        {
            if (_pollcnt-- == 0)
            {
                _polltimer.Change(Timeout.Infinite, Timeout.Infinite);
                _polltimer.Dispose();
                _polltimer = null;
            }

            // ReSharper disable once ArrangeStaticMemberQualifier
            CommandSocket.SendCommand(NeighDiscCmd.GetCommand());
        }
    }
}
