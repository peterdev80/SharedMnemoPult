using System;

namespace fmslapi
{
    public interface IChannel
    {
        byte[] TryGetMessage();
        
        void SendMessage(byte[] Data);

        void SendMessage(IntPtr Data, int Length);

        void Leave();
    }
}