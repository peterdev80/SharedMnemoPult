using System.IO;

namespace fmslstrap.Variables.VarTypes
{
    public class KVar : Variable
    {
        public KVar(ref uint Offset)
        {
            AlignOffset(ref Offset);

            SharedOffset = Offset;
            AssingValuePointer();
        }

        public override void PackValue(BinaryWriter writer)
        {
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
        }

        public override uint SizeOf
        {
            get { return 0; }
        }
    }
}
