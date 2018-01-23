namespace fmslapi.Channel
{
    public class SyncChannel : BaseSyncChannel
    {
        private readonly IChannel _basechan;

        public SyncChannel(IChannel BaseChannel)
        {
            _basechan = BaseChannel;

            if (_basechan != null)
                _basechan.Received += Received;
        }

        public IChannel BaseChannel => _basechan;
    }
}
