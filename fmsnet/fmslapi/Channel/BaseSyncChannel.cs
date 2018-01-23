using System.Collections.Generic;
using System.Threading;

namespace fmslapi.Channel
{
    public class BaseSyncChannel
    {
        private struct hold
        {
            public ISenderChannel Sender;
            public ReceivedMessage Message;
        }

        private readonly ReaderWriterLockSlim _l = new ReaderWriterLockSlim();
        private readonly Queue<hold> _q = new Queue<hold>();

        public bool HasMessages
        {
            get
            {
                try
                {
                    _l.EnterReadLock();

                    return _q.Count > 0;
                }
                finally
                {
                    _l.ExitReadLock();
                }
            }
        }

        public DataReceived OnReceived => Received;

        public ReceivedMessage TryGetMessage()
        {
            // ReSharper disable once UnusedVariable
            TryGetMessage(out var s, out var m);

            return m;
        }

        public bool TryGetMessage(out ISenderChannel Sender, out ReceivedMessage Msg)
        {
            Sender = null;
            Msg = null;

            try
            {
                _l.EnterWriteLock();

                if (_q.Count == 0)
                    return false;

                var h = _q.Dequeue();

                Sender = h.Sender;
                Msg = h.Message;

                return true;
            }
            finally
            {
                _l.ExitWriteLock();
            }
        }

        protected void Received(ISenderChannel Sender, ReceivedMessage Msg)
        {
            try
            {
                _l.EnterWriteLock();

                _q.Enqueue(new hold { Message = Msg, Sender = Sender });
            }
            finally
            {
                _l.ExitWriteLock();
            }
        }
    }
}
