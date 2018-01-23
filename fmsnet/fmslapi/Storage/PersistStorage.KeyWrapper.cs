using System.Linq;

// ReSharper disable InconsistentNaming

namespace fmslapi.Storage
{
    internal partial class PersistStorage
    {
        private class kw
        {
            private readonly byte[] _v;

            public kw(byte[] Value)
            {
                _v = Value;
            }

            public override int GetHashCode()
            {
                return _v.Sum(x => (int)x);
            }

            public override bool Equals(object obj)
            {
                var o = obj as kw;
                if (o == null)
                    return false;

                return _v.SequenceEqual(o._v);
            }

            public bool IsEmpty
            {
                get
                {
                    return _v.All(x => x == 0);
                }
            }
        }
    }
}
