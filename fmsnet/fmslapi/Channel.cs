using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace fmslapi
{
    public partial class Channel : IChannel
    {
        private readonly UdpClient _udp;
        private readonly IPEndPoint _target;

        private readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();

        public Channel(string LocalPortID, string RemoteEndpointID)
        {
            try
            {
                if (LocalPortID == null)
                    _udp = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                else if (_endpoints.TryGetValue(LocalPortID, out var ipe))
                    _udp = new UdpClient(ipe);
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
            }

            _endpoints.TryGetValue(RemoteEndpointID, out _target);

            if (_udp != null)
                StartReceive();
        }

        private void StartReceive()
        {
            while (true)
            {
                try
                {
                    _udp.BeginReceive(Received, null);
                    return;
                }
                catch (SocketException) { }
            }
        }

        private void Received(IAsyncResult res)
        {
            try
            {
                var ipe = new IPEndPoint(IPAddress.Any, 0);
                var binary = _udp.EndReceive(res, ref ipe);

                _queue.Enqueue(binary);
            }
            catch (SocketException) { }
            finally
            {
                StartReceive();
            }
        }

        public byte[] TryGetMessage() => _queue.TryDequeue(out var r) ? r : null;

        public void SendMessage(byte[] Data)
        {
            if (_target == null || Data == null || _udp == null)
                return;

            try
            {
                _udp.Send(Data, Data.Length, _target);
            }
            catch (SocketException)
            {
            }
        }

        public void SendMessage(IntPtr Data, int Length)
        {
            var bf = new byte[Length];
            Marshal.Copy(Data, bf, 0, Length);

            SendMessage(bf);
        }

        public void Leave()
        {
        }
    }
}
