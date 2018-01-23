using System;
using System.IO;

// ReSharper disable InconsistentNaming

namespace fmslstrap.Variables.VarTypes
{
    public unsafe class LVar : Variable
    {
        private readonly Int64* _lptr;

        public LVar(ref uint Offset)
        {
            AlignOffset(ref Offset);

            SharedOffset = Offset;
            AssingValuePointer();
            _lptr = (Int64*)SharedPointer;
        }

        public override void PackValue(BinaryWriter writer)
        {
            writer.Write(*_lptr);
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
            var v = Reader.ReadInt64();

            if (SkipOnly)
                return;

            *_lptr = v;
        }

        public override uint SizeOf
        {
            get { return sizeof(Int64); }
        }
    }
}
