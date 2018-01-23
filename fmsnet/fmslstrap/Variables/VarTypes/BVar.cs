using System.IO;

namespace fmslstrap.Variables.VarTypes
{
    public unsafe class BVar : Variable
    {
        private readonly bool* _bptr;

        public BVar(ref uint Offset)
        {
            SharedOffset = Offset;
            AssingValuePointer();
            _bptr = (bool*)SharedPointer;
        }

        public override void PackValue(BinaryWriter writer)
        {
            writer.Write(*_bptr);
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
            var v = Reader.ReadBoolean();

            if (SkipOnly)
                return;

            *_bptr = v;
        }

        public override uint SizeOf => sizeof(bool);
    }
}
