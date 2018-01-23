using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace fmslstrap.Variables.VarTypes
{
    /// <summary>
    /// Сторожевая переменная
    /// </summary>
    public unsafe class WVar : BVar
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct VS
        {
            [FieldOffset(0)]
            public UInt16 TickCounter;

            [FieldOffset(2)]
            public bool Locked;
        }

        private static readonly List<WVar> _wdogs = new List<WVar>();
        private static Timer _wdt;

        private readonly VS* _v;
        private readonly UInt16 _size;
        private bool _pv;
        private readonly int _reductionsize;
        private readonly int _delaysize;
        private int _reductioncnt;
        private bool _needsend;

        public WVar(ref uint Offset, UInt16 Size, UInt16 Reduction, UInt16 Delay)
            : base(ref Offset)
        {
            //TickCounter = (UInt16*)SharedPointer;
            _v = (VS*)SharedPointer;

            _size = Size;
            _reductionsize = Reduction;
            _delaysize = Delay;

            _v->TickCounter = 0;
            _v->Locked = false;
            _pv = false;

            _wdogs.Add(this);
        }

        public override void PackValue(BinaryWriter writer)
        {
            var v = _v->TickCounter;
            if (v <= _size)
                v = 0;

            writer.Write(v);
            writer.Write(_v->Locked);
        }

        public override void ParseDelta(BinaryReader Reader, bool SkipOnly)
        {
            // Факт приема переменной из сети сбрасывает сторожевой таймер
            var v = Reader.ReadUInt16();
            var l = Reader.ReadBoolean();

            if (SkipOnly)
                return;

            Reset(v);
            _v->Locked = l;
        }

        /// <summary>
        /// Размер тела переменной в сетевом пакете
        /// </summary>
        public override uint SizeOf
        {
            get { return (uint)sizeof(VS); }
        }

        public bool Reset()
        {
            Interlocked.Increment(ref _reductioncnt);

            if (_delaysize > 0)
                if (_v->TickCounter > _delaysize)
                {
                    _needsend = true;
                    return false;
                }

            if (_reductioncnt < _reductionsize)
                return false;

            _reductioncnt = 0;

            Reset(0);

            return true;
        }

        public void TotalReset()
        {
            _v->TickCounter = 0;
            _pv = false;
            _vartable.RaiseOnVarChanged(new[] { this }, false);
        }

        public void Reset(UInt16 TickCounterValue)
        {
            var ov = _v->TickCounter;

            if (ov < _size)
                _v->TickCounter = _size;

            if (TickCounterValue > _size)
                _v->TickCounter = TickCounterValue;

            if (ov == 0 || !_pv)
            {
                _pv = true;
                _vartable.RaiseOnVarChanged(new[] { this }, false);
            }
        }

        public static void TickDogs(object state)
        {
            lock (_wdogs)
            {
                foreach (var wd in _wdogs)
                {
                    var v = wd._v;
                    var vt = wd._vartable;

                    if (wd._needsend && v->TickCounter > 0 && v->TickCounter < wd._delaysize && !v->Locked)
                    {
                        vt.RaiseOnVarChanged(new[] { wd }, false);
                        wd._needsend = false;
                    }

                    if (v->TickCounter > 0 && !v->Locked)
                        if (--(v->TickCounter) == 0)
                        {
                            wd._pv = false;
                            vt.RaiseOnVarChanged(new[] { wd }, false);
                        }
                }
            }
        }

        public static void StartTimer()
        {
            Debug.Assert(_wdogs != null, "_wdogs != null");

            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var wd in _wdogs)
            {
                wd._v->TickCounter = 0;
                //wd._vartable.SendChanges(new[] { wd }, true);     // Зачем отслыать? Это же инициализация, ёпт!!!
            }

            _wdt = new Timer(TickDogs, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        }

        public static void StopTimer()
        {
            _wdt.Dispose();
            _wdt = null;
        }
    }
}
