using System.IO;

namespace fmslstrap.Variables.VarTypes
{
    public unsafe class FVar : Variable
    {
        private readonly float* _fptr;

        public FVar(ref uint Offset)
        {
            AlignOffset(ref Offset);

            SharedOffset = Offset;
            AssingValuePointer();
            _fptr = (float*)SharedPointer;
        }

        public override void PackValue(BinaryWriter writer)
        {
            writer.Write(*_fptr);
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
            var v = Reader.ReadSingle();

            if (SkipOnly)
                return;

            *_fptr = v;
        }

        public override uint SizeOf
        {
            get { return sizeof(float); }
        }
    }
}
