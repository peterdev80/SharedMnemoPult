using System;
using System.IO;

// ReSharper disable InconsistentNaming

namespace fmslstrap.Variables.VarTypes
{
    public unsafe class IVar : Variable
    {
        private readonly int* _iptr;

        public IVar(ref uint Offset)
        {
            AlignOffset(ref Offset);

            SharedOffset = Offset;
            AssingValuePointer();
            _iptr = (int*)SharedPointer;
        }

        public override void PackValue(BinaryWriter writer)
        {
            writer.Write(*_iptr);
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
            var v = Reader.ReadInt32();

            if (SkipOnly)
                return;

            *_iptr = v;
        }

        public override uint SizeOf
        {
            get { return sizeof(Int32); }
        }
    }
}
