using System;
using System.IO;
using System.ComponentModel;
using System.IO.Pipes;

namespace fmslapi.Channel
{
    /// <summary>
    /// Поток чтения
    /// </summary>
    public class ChannelDataStream : Stream, INotifyPropertyChanged
    {
        private double _progress;
        private NamedPipeClientStream _ps;
        private int _expsize;
        private bool _active;
        private long _pos;
        private double _ppr;

        public ChannelDataStream(string PipeName, int ExpectedSize)
        {
            _ps = new NamedPipeClientStream(".", PipeName, PipeDirection.In);
            _ps.Connect();

            _expsize = ExpectedSize;
            _active = false;
            _pos = 0;
            _ppr = -1;
            _active = true;

            ProgressAccuracy = 0.001;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override long Length => _expsize;

        public override long Position
        {
            get => _pos;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_active)
                return 0;

            var readed = 0;

            // Если данных об ожидаемой длине потока нет - не формируем отчеты о прогрессе
            if (_expsize <= 0)
            {
                readed = _ps.Read(buffer, offset, count);

                if (readed == 0)
                {
                    // Входной поток закончился
                    _active = false;
                    _ps.Close();
                    _ps = null;
                }

                return readed;
            }

            var toread = buffer.Length - offset;
            if (toread > count)
                toread = count;

            var step = _expsize / 1000;             // Количество принятых байт между отчетами о прогрессе
            
            if (step < 128)
                step = 128;

            if (step > count)
                step = count;

            while (toread > 0)
            {
                if (step > toread)
                    step = toread;

                var r = _ps.Read(buffer, offset, step);

                readed += r;
                toread -= r;
                _pos += r;
                offset += r;

                Progress = _pos / (double)_expsize;

                if (r != 0)
                    continue;

                // Поток неожиданно закончился
                _active = false;
                _ps.Close();
                _ps = null;
                return readed;
            }

            if (_pos == _expsize)
            {
                _active = false;
                _ps.Close();
                _ps = null;

                Progress = 1;
            }

            return readed;
        }

        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) {  throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        public override void Flush() { } 

        public double Progress
        {
            get => _progress;

            private set
            {
                _progress = value;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                var nu = Math.Abs(_ppr - _progress) > ProgressAccuracy || value == 1D;

                if (!nu) 
                    return;
                
                _ppr = _progress;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
            }
        }

        public double ProgressAccuracy;

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    _ps?.Close();

                    _ps = null;
                }
            }

            base.Dispose(disposing);
        }

        public void SetExpectedSize(int ExpectedSize)
        {
            _expsize = ExpectedSize;
        }
    }
}
