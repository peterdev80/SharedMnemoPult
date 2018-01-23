using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using fmslapi;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Globalization;
using fmslapi.Channel;

namespace fmsproxy
{
    public class ModelProxy
    {
        #region Частные данные
        private UdpClient _udp;
        private IChannel _chan;
        private static readonly Regex tprxyregex = new Regex(@"(.*)\s*:\s*(\d*)(\s*,\s*(\d*))?(\s*,\s*(\S*))?");
        private Tuple<IPEndPoint, UdpClient, PacketReassembler>[] _sendlst;
        private bool _rawsend;
        #endregion

        #region Публичные события
        public event EventHandler OnDisconnected;
        public event EventHandler OnConnected;
        #endregion

        #region Публичные свойства
        public Label CountIndicator;
        public long ReceivedPackets;
        public long SendedPackets;
        public static readonly Dictionary<int, UdpClient> LocalPoints = new Dictionary<int, UdpClient>();

        public IManager Manager
        {
            set { _manager = value; }
        }

        public IConfigSection Config
        {
            set { _config = value; }
        }
        #endregion

        #region Наследуемые данные
        protected IConfigSection _config;
        protected IManager _manager;
        protected readonly ConcurrentQueue<Tuple<byte[], UdpClient, IPEndPoint>> OutputQueue = new ConcurrentQueue<Tuple<byte[], UdpClient, IPEndPoint>>();
        #endregion

        public virtual void Load()
        {
            var lport = _config.GetInt("port.incoming");
            _rawsend = _config.GetBool("raw.send");

            if (!LocalPoints.ContainsKey(lport))
                LocalPoints.Add(lport, new UdpClient(new IPEndPoint(IPAddress.Any, lport)));

            _udp = LocalPoints[lport];

            var tpt = _config.AsArray("send.to");
            if (tpt != null && tpt.Length > 0)
            {
                _sendlst = new Tuple<IPEndPoint, UdpClient, PacketReassembler>[tpt.Length];
                for (var i = 0; i < tpt.Length; i++)
                {
                    var m = tprxyregex.Match(tpt[i]);

                    var eip = m.Groups[1].Value.Trim();
                    var sp = m.Groups[2].Value;
                    var port = string.IsNullOrWhiteSpace(sp) ? 0 : int.Parse(sp, CultureInfo.InvariantCulture);
                    var spb = m.Groups[4].Value;
                    var portb = string.IsNullOrWhiteSpace(spb) ? 0 : int.Parse(spb, CultureInfo.InvariantCulture);
                    var rsm = m.Groups[6].Value;

                    PacketReassembler reassembler = null;
                    if (!string.IsNullOrWhiteSpace(rsm))
                    {
                        var rc = _config.GetPrefixed(x => string.Format("{0}.{1}", rsm, x));
                        reassembler = Activator.CreateInstance(Type.GetType(rc["type.name"])) as PacketReassembler;
                        reassembler.Config = rc;
                        reassembler.Proxy = this;
                        reassembler.Init();
                    }

                    UdpClient udp;
                    LocalPoints.TryGetValue(portb, out udp);
                    if (udp == null)
                        udp = _udp;

                    _sendlst[i] = new Tuple<IPEndPoint, UdpClient, PacketReassembler>(new IPEndPoint(IPAddress.Parse(eip), port), udp, reassembler);
                }
            }

            if (_config.GetBool("udp.startreceive"))
                StartReceive();

            var rawch = _config["raw.channel"];
            if (!string.IsNullOrWhiteSpace(rawch))
            {
                var dr = _config.GetBool("fms.writeonly") ? (DataReceived)null : ProcessIncomingUDP;
                _chan = _manager.JoinChannel(rawch, dr, ChanChanged);
            }
        }

        private void StartReceive()
        {
            var done = false;
            while (!done)
            {
                try
                {
                    _udp.BeginReceive(Received, null);
                    done = true;
                }
                catch (SocketException) { }
            }
        }

        #region Наследуемые методы
        protected virtual void ProcessIncomingUDP(ISenderChannel Sender, byte[] Data)
        {
            if (Data == null)
                return;

            if (_chan != null && _rawsend)
                _chan.SendMessage(Data);

            SendToProxies(Data);
        }

        protected void ProcessIncomingUDP(ISenderChannel Sender, ReceivedMessage Message)
        {
            ProcessIncomingUDP(Sender, Message.Data);
        }

        protected virtual void ProcessIncomingChannel(byte[] Data) { }
        protected virtual void IndividualVarProcess(UInt16 Index, IVariable Variable, object NewValue, ref bool UndoVar) { }
        #endregion

        private void Received(IAsyncResult ar)
        {
            var ipe = new IPEndPoint(IPAddress.Any, 0);

            byte[] binary = null;

            try
            {
                binary = _udp.EndReceive(ar, ref ipe);
                Interlocked.Increment(ref ReceivedPackets);
            }
            catch (SocketException) { }
            catch (IOException) { }
            catch (ObjectDisposedException) { return; }
            
            StartReceive();

            if (binary != null)
                ProcessIncomingUDP(null, binary);
        }

        protected void SendToProxies(byte[] Data)
        {
            if (_sendlst == null || Data == null)
                return;

            foreach (var ipt in _sendlst)
            {
                var rsm = ipt.Item3;

                Action<byte[]> addq = b => OutputQueue.Enqueue(new Tuple<byte[], UdpClient, IPEndPoint>(b, ipt.Item2, ipt.Item1));

                if (rsm != null)
                    rsm.ReassemblyPacket(Data, addq);
                else
                    addq(Data);

                SendQueue();
            }
        }

        protected void SendQueue()
        {
            while (OutputQueue.Count > 0)
            {
                try
                {
                    Tuple<byte[], UdpClient, IPEndPoint> s = null;
                    if (!OutputQueue.TryDequeue(out s))
                        return;

                    var b = s.Item1;
                    s.Item2.Send(b, b.Length, s.Item3);
                    Interlocked.Increment(ref SendedPackets);
                }
                catch (SocketException) { }
                catch (IOException) { }
                catch (ObjectDisposedException) { }
            }
        }

        protected void ChanChanged(ChannelStateChangedStates args)
        {
            switch (args)
            {
                case ChannelStateChangedStates.FirstConnect:
                case ChannelStateChangedStates.Connected:
                    if(OnConnected != null)
                        OnConnected(this, EventArgs.Empty);
                    break;

                case ChannelStateChangedStates.CantConnect:
                case ChannelStateChangedStates.Disconnected:
                    if(OnDisconnected != null)
                        OnDisconnected(this, EventArgs.Empty);
                    break;

                default:
                    break;
            }
        }

        public virtual void CloseProxy()
        {
            if (_chan != null)
                _chan.Leave();

            try
            {
                _udp.Close();
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            if (OnDisconnected != null)
                OnDisconnected(this, EventArgs.Empty);
        }
    }
}
