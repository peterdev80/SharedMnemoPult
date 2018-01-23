using System;
using System.Collections.Generic;
using fmslstrap.Channel;
using System.IO;
using System.Threading;

namespace fmslstrap.Administrator
{
    internal class AdmChannel
    {
        public delegate void Received(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender);

        private readonly ChanConfig _adm;
        private readonly Dictionary<char, Received> _cmds = new Dictionary<char, Received>();

        public event Action<AdmChannel> OnClose;

        public AdmChannel(ChanConfig AdmChannel)
        {
            _adm = AdmChannel;
            _adm.OnDataReceived += OnReceived;
            _adm.OnClose += CloseChan;
        }

        public void RegisterAdmCommand(char Cmd, Received OnReceived)
        {
            lock (_cmds)
            {
                Received rcv;

                _cmds.TryGetValue(Cmd, out rcv);
                _cmds[Cmd] = (Received)Delegate.Combine(rcv, OnReceived);
            }
        }

        private void CloseChan(ChanConfig Sender)
        {
            OnClose?.Invoke(this);
        }

        private void OnReceived(ChanConfig Sender, DataPacket Packet)
        {
            var msg = Packet.Data;

            if (msg.Length == 0)
                return;

            var cmd = (char)msg[0];
            Logger.WriteLine(string.Format("AdmChannel: Принята команда {0}", cmd));

            Received rcvd;
            lock (_cmds)
            {
                if (!_cmds.TryGetValue(cmd, out rcvd))
                    if (!_cmds.TryGetValue('*', out rcvd))
                        return;
            }

            ThreadPool.QueueUserWorkItem(x =>
                {
                    foreach (var v in rcvd.GetInvocationList())
                    {
                        var mms = new MemoryStream(msg) { Position = 1 };

                        v.DynamicInvoke(mms, new BinaryReader(mms), msg, Sender);
                    }
                });
        }

        public void SendMessage(byte[] Data)
        {
            _adm.SendMessage(Data);
        }

        public void SendMessage(byte[] Data, string ToHost)
        {
            _adm.SendMessage(Data, ToHost);
        }
    }
}
