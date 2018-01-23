using System;
using System.IO;

namespace fmslstrap.Variables.VarTypes
{
    public unsafe class CVar : Variable
    {
        private readonly char* _cptr;

        public CVar(ref uint Offset)
        {
            AlignOffset(ref Offset);

            SharedOffset = Offset;
            AssingValuePointer();
            _cptr = (char*)SharedPointer;
        }

        public override void PackValue(BinaryWriter writer)
        {
            try
            {
                writer.Write(*_cptr);
            }
            catch (ArgumentException)
            {
                writer.Write(' ');
            }
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
            var v = Reader.ReadChar();

            if (SkipOnly)
                return;

            *_cptr = v;
        }

        public override uint SizeOf
        {
            get { return sizeof(char); }
        }
    }
}
