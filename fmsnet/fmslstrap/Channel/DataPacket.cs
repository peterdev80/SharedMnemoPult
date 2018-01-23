using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Diagnostics;

namespace fmslstrap.Channel
{
    internal class DataPacket
    {
        private class StreamData
        {
            public Stream Stream;
            public Action<long> UpdateStats;
        }

        public string Sender = Config.WorkstationName;
        public byte[] Data;
        public int Size;
        public UInt32 OrderID;
        public UInt32 SenderID;
        public IPEndPoint ReceivedFrom;

        private StreamData _src;
        private List<StreamData> _dsts = new List<StreamData>();

        private bool _localready, _remoteready;

        private event Action OnComplete;

        public DataPacket(ulong InstanceID)
        {
            CreatorInstanceID = InstanceID;
        }

        public DataPacket()
        {
        }

        public ulong CreatorInstanceID { get; }

        public bool IsStreamPacket => _src != null;

        public int Length => IsStreamPacket ? -1 :  Data.Length;

        public void SetSourceStream(Stream Source, Action<long> UpdateStats)
        {
            _src = new StreamData { Stream = Source, UpdateStats = UpdateStats };
        }

        public void SetSourceUpdateStats(Action<long> UpdateStats)
        {
            Debug.Assert(_src != null, "_src != null");

            _src.UpdateStats = UpdateStats;
        }

        public void AddTargetStream(Stream Target, Action<long> UpdateStats)
        {
            _dsts.Add(new StreamData { Stream = Target, UpdateStats = UpdateStats });
        }

        private void TransferStream()
        {
            var buf = new byte[16384];

            while (true)
            {
                int readed;

                // Чтение порции данных из входного потока
                // При возникновении ошибки передача завершается
                try
                {
                    readed = _src.Stream.Read(buf, 0, buf.Length);
                    if (readed == 0)
                        break;
                }
                catch (IOException)
                {
                    break;
                }

                _src.UpdateStats?.Invoke(readed);

                var hasfailedstream = false;

                // Передача прочитанных данных во все целевые потоки
                // При ошибке передачи - целевой поток, выбросивший ошибку, исключается из списка
                foreach (var t in _dsts)
                {
                    try
                    {
                        if (t.Stream == null)
                            continue;

                        var ps = t.Stream as NamedPipeServerStream;

                        if (ps != null && !ps.IsConnected)
                            ps.WaitForConnection();

                        t.Stream.Write(buf, 0, readed);

                        t.UpdateStats?.Invoke(readed);
                    }
                    catch (SystemException)
                    {
                        hasfailedstream = true;

                        try
                        {
                            t.Stream?.Close();
                        }
                        catch (SystemException) { }

                        t.Stream = null;
                    }
                }

                if (hasfailedstream)
                    _dsts = _dsts.Where(s => s.Stream != null).ToList();
            }

            // Закрытие каналов
            try
            {
                _src.Stream.Close();
            }
            catch (SystemException) { }

            foreach (var t in _dsts)
            {
                try
                {
                    t.Stream?.Close();
                }
                catch (SystemException) { }
            }
        }

        public void RemoteReady(Action Complete)
        {
            lock (this)
            {
                _remoteready = true;

                OnComplete += Complete;
            }

            CheckReady();
        }

        public void LocalReady(Action Complete)
        {
            lock (this)
            {
                _localready = true;

                OnComplete += Complete;
            }
            
            CheckReady();
        }

        private void CheckReady()
        {
            lock (this)
            {
                if (!_localready || !_remoteready)
                    return;
            }

            TransferStream();

            OnComplete?.Invoke();
        }

        public void ConvertToStream()
        {
            _src = new StreamData { Stream = new MemoryStream(Data), UpdateStats = null };
        }
    }
}
