using System;
using System.IO;
#pragma warning disable 649

namespace fmslstrap.Variables.VarTypes
{
    /// <summary>
    /// Переменная хранящая массив байт
    /// </summary>
    public class AVar : Variable
    {
        private struct SH
        {
            /// <summary>
            /// Счетчик межпроцессной блокировки
            /// </summary>
            public UInt32 LockCounter;

            /// <summary>
            /// Размер массива
            /// </summary>
            public UInt16 Length;
        }

        public unsafe AVar(ref uint Offset, int Size)
        {
            AlignOffset(ref Offset);

            SharedOffset = Offset;
            this.Size = Size;
            AssingValuePointer();

            var ptr = (SH*)SharedPointer;

            _lock = new CrossProcessReaderWriterLock(&(ptr->LockCounter));
        }

        public override void PackValue(BinaryWriter writer)
        {
            var b = Value;
            writer.Write((UInt16)b.Length);
            writer.Write(b);
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
            var sz = Reader.ReadUInt16();
            var v = Reader.ReadBytes(sz);

            if (SkipOnly)
                return;

            Value = v;
        }

        public readonly int Size;

        public override unsafe uint SizeOf
        {
            get { return (uint)(sizeof(SH) + (uint)Size); }
        }

        public override unsafe uint ActualSizeOf
        {
            get 
            {
                try
                {
                    _lock.EnterReadLock();

                    var ptr = (SH*)SharedPointer;

                    return ptr->Length;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        private unsafe byte[] Value
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();

                    var ptr = (SH*)SharedPointer;

                    var l = ptr->Length;
                    var b = new byte[l];
                    VariablesTable.SharedAccessor.ReadArray((int)SharedOffset + sizeof(SH), b, 0, l);
                    return b;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _lock.EnterWriteLock();

                    var ptr = (SH*)SharedPointer;

                    var l = value.Length;

                    ptr->Length = (UInt16)l;

                    VariablesTable.SharedAccessor.WriteArray((int)SharedOffset + sizeof(SH), value, 0, l);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
    }
}
