using System.Collections.Generic;

namespace fmslapi.Channel
{
    public class SyncMultiChannel : BaseSyncChannel
    {
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<IChannel> _chans = new List<IChannel>();

        public void AddSourceChannel(IChannel Channel)
        {
            lock (this)
            {
                _chans.Add(Channel);
            }

            Channel.Received += Received;
        }
    }
}
