using System;
using System.Net;
using fmslstrap.Administrator;
using System.IO;

namespace fmslstrap.CommandSocket.PeerCommands
{
    /// <summary>
    /// Поиск в сети загрузчика
    /// </summary>
    public class BootstrapDiscoveryCmd : BaseCommand
    {
        public override void Invoke(BinaryReader Reader, IPEndPoint EndPoint, out string LogLine)
        {
            var domain = Reader.ReadString();

            LogLine = string.Format(@"->BOOTSTRAPDISCOVERY<- Domain: {0}", domain);

            if (domain != Config.DomainName)
                return;

            if (!BootstrapDeploy.HasData)
                return;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write((UInt16)BootstrapDeploy.LocalPort);

            CommandSocket.SendCommand(ms.ToArray(), EndPoint);
        }
    }
}
