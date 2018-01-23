using System;

namespace fmslapi
{
    public class WeakReference<T> : WeakReference where T : class 
    {
        public WeakReference(T target) : base(target)
        {
        }

        public new T Target
        {
            get => (T)base.Target;
            set => base.Target = value;
        }
    }
}
