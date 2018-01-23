using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace fmslapi.Channel
{
    /// <summary>
    /// Поток передачи данных в fmsldr
    /// </summary>
    internal class ChannelSendStream : Stream
    {
        private readonly string _pipename;
        private readonly NamedPipeServerStream _srv;
        private readonly EventWaitHandle _writeenabled = new ManualResetEvent(false);

        public ChannelSendStream()
        {
            _pipename = Guid.NewGuid().ToString();
            _srv = new NamedPipeServerStream(_pipename, PipeDirection.Out, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 262144, 262144);

            _srv.BeginWaitForConnection(OnConnect, null);
        }

        public string PipeName => _pipename;

        private void OnConnect(IAsyncResult ar)
        {
            _srv.EndWaitForConnection(ar);

            _writeenabled.Set();
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;

        public override void Flush()
        {
            _writeenabled.WaitOne();

            _srv.WaitForPipeDrain();
        }

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _writeenabled.WaitOne();

            // Вывод данных в поток всегда заканчивается успешно, даже если канал был закрыт сервером.

            try
            {
                if (_srv.IsConnected)
                    _srv.Write(buffer, offset, count);
            }
            catch (IOException) { }
        }

        public override void Close()
        {
            base.Close();

            try
            {
                if (_srv.IsConnected)
                {
                    _srv.WaitForPipeDrain();
                    _srv.Close();
                }
            }
            catch (IOException) { }

            _writeenabled.Close();
        }
    }
}
