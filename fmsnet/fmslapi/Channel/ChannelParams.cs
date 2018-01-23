using System;

namespace fmslapi.Channel
{
    internal struct ChannelParams
    {
        public Manager Manager;
        public string Channel;
        public ChannelStateChanged ChannelStateChanged;
        public ChannelMode ChannelMode;
        public string EndPoint;
        public string VarMap;
        public VariablesChanged VariablesChanged;
        public Guid ComponentID;
        public Guid InstanceID;
    }
}
