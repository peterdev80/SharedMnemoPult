namespace fmslstrap.Channel
{
    public class ChannelChangeEventArgs
    {
        public ChannelChangeType ChangeType;
        public string NewHost;
    }

    public enum ChannelChangeType
    {
        AddHost
    }
}
