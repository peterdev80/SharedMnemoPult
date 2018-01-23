using System;

namespace fmsman
{
    public static class GlobalWatcher
    {
        public static Action<uint> Change;

        public static void Raise(uint VarIndex)
        {
            Change?.Invoke(VarIndex);
        }
    }
}
