using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
#pragma warning disable 649

namespace fmslstrap.Variables.VarTypes
{
    public unsafe class SVar : Variable
    {
        private struct SH
        {
            /// <summary>
            /// Счетчик межпроцессной блокировки
            /// </summary>
            public UInt32 LockCounter;

            /// <summary>
            /// Максимальный размер строки в символах Unicode
            /// </summary>
            public UInt16 MaxSize;

            /// <summary>
            /// Текущий размер строки в символах Unicode
            /// </summary>
            public UInt16 Size;
        }

        private readonly SH* _ptr;

        public SVar(ref uint Offset, int Size)
        {
            AlignOffset(ref Offset);

            SharedOffset = Offset;
            AssingValuePointer();

            _ptr = (SH*)SharedPointer;
            _ptr->MaxSize = (UInt16)Size;

            _lock = new CrossProcessReaderWriterLock(&(_ptr->LockCounter));
        }

        public override void PackValue(BinaryWriter writer)
        {
            var bs = Encoding.UTF8.GetBytes(Value);
            var sz = (UInt16)bs.Length;

            writer.Write(sz);
            writer.Write(bs);
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
            var sz = Reader.ReadUInt16();
            var v = Reader.ReadBytes(sz);

            if (SkipOnly)
                return;

            Value = Encoding.UTF8.GetString(v);
        }

        public override UInt32 SizeOf
        {
            get 
            {
                try
                {
                    _lock.EnterReadLock();

                    return (UInt32)_ptr->MaxSize * sizeof(char) + (UInt32)sizeof(SH);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public override uint ActualSizeOf
        {
            get 
            {
                try
                {
                    _lock.EnterReadLock();

                    return (UInt32)Encoding.UTF8.GetByteCount((char*)(_ptr + 1), _ptr->Size) + sizeof(UInt16);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        private string Value
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();

                    var bs = _ptr->Size * sizeof(char);

                    if (bs == 0)
                        return "";

                    var b = new byte[bs];
                    Marshal.Copy(new IntPtr(_ptr + 1), b, 0, bs);

                    return Encoding.Unicode.GetString(b);
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

                    var ml = _ptr->MaxSize;
                    if (value.Length > ml)
                        value = value.Substring(0, ml);

                    _ptr->Size = (UInt16)value.Length;

                    var l = Encoding.Unicode.GetBytes(value);

                    Marshal.Copy(l, 0, new IntPtr(_ptr + 1), l.Length);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
    }
}
