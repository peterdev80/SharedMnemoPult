using System.IO;

namespace fmslstrap.Variables.VarTypes
{
    public unsafe class DVar : Variable
    {
        private readonly double *_dptr;

        public DVar(ref uint Offset)
        {
            AlignOffset(ref Offset);

            SharedOffset = Offset;
            AssingValuePointer();
            _dptr = (double*)SharedPointer;
        }

        public override void PackValue(BinaryWriter writer)
        {
            writer.Write(*_dptr);
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
            var v = Reader.ReadDouble();

            if (SkipOnly)
                return;

            *_dptr = v;
        }

        public override uint SizeOf
        {
            get { return sizeof(double); }
        }
    }
}
