using System.Net;
using System.IO;

namespace fmslstrap.CommandSocket.PeerCommands
{
    /// <summary>
    /// Запрос канальной конфигурации хоста 
    /// </summary>
    public class PeerMembershipReqCmd : BaseCommand
    {
        public override void Invoke(BinaryReader Reader, IPEndPoint EndPoint, out string LogLine)
        {
            LogLine = null;

            var domain = Reader.ReadString();

            if (domain != Config.DomainName)
                return;

            LogLine = string.Format(@"->PEERMEMBERSHIPREQ<- Domain: {0}", domain);

            CommandSocket.SendCommand(PeerHostConfCmd.GetCommand(), EndPoint);
        }

        public static byte[] GetCommand()
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write((byte)'B');
            wr.Write(Config.DomainName);

            return ms.ToArray();
        }
    }
}
