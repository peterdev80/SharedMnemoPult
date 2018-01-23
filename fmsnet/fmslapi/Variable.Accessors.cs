using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
// ReSharper disable ParameterHidesMember
// ReSharper disable InconsistentNaming
#pragma warning disable 169

namespace fmslapi
{
    internal partial class Variable
    {
        #region Value accessors
        /// <summary>
        /// Значение переменной
        /// </summary>
        public object Value
        {
            get
            {
                switch (_type)
                {
                    case VariableType.Boolean: return (this as IBoolVariable).Value;
                    case VariableType.Trigger: return (this as ITriggerVariable).Value;
                    case VariableType.Int32: return (this as IIntVariable).Value;
                    case VariableType.Long: return (this as ILongVariable).Value;
                    case VariableType.Single: return (this as IFloatVariable).Value;
                    case VariableType.Double: return (this as IDoubleVariable).Value;
                    case VariableType.Char: return (this as ICharVariable).Value;
                    case VariableType.String: return (this as IStringVariable).Value;
                    case VariableType.WatchDog: return (this as IWatchDogVariable).Value;
                    case VariableType.ByteArray: return (this as IByteArrayVariable).Value;
                    default: return null;
                }
            }
            set
            {
                if (_checkdups)
                    if ((this as IVariable).Value == value)
                        return;

                switch (_type)
                {
                    case VariableType.Boolean: (this as IBoolVariable).Value = Convert.ToBoolean(value); break;
                    case VariableType.Trigger: (this as ITriggerVariable).Value = Convert.ToBoolean(value); break;
                    case VariableType.Int32: (this as IIntVariable).Value = Convert.ToInt32(value); break;
                    case VariableType.Long: (this as ILongVariable).Value = (int)value; break;
                    case VariableType.Single: (this as IFloatVariable).Value = (float)value; break;
                    case VariableType.Double: (this as IDoubleVariable).Value = (double)value; break;
                    case VariableType.Char: (this as ICharVariable).Value = (char)value; break;
                    case VariableType.String: (this as IStringVariable).Value = (string)value; break;
                    case VariableType.KMD: (this as IKVariable).Set(); break;
                    case VariableType.WatchDog: if ((bool)value) (this as IWatchDogVariable).Reset(); break;
                    case VariableType.ByteArray: (this as IByteArrayVariable).Value = value as byte[]; break;
                }

                OnPropertyChanged(nameof(Value));
            }
        }

        /// <inheritdoc />
        public bool SetIfChanged(object Value)
        {
            lock (this)
            {
                var r = IsChanged;

                if (r)
                    this.Value = Value;

                return r;
            }
        }

        /// <inheritdoc />
        public bool SetIfNotChanged(object Value)
        {
            lock (this)
            {
                var r = IsChanged;

                if (!r)
                    this.Value = Value;

                return !r;
            }
        }

#pragma warning disable 0649
        /// <summary>
        /// Заголовок хранилища переменных ByteArrayVariable
        /// </summary>
        private struct AH
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

        /// <summary>
        /// Заголовок хранилища строковых переменных
        /// </summary>
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
#pragma warning restore 0649

        private unsafe string UnsafeExtractStringVariableValue()
        {
            var ptr = (SH*)_sharedpointer;

            var bs = ptr->Size * sizeof(char);

            if (bs == 0)
                return "";

            var b = new byte[bs];
            Marshal.Copy(new IntPtr(ptr + 1), b, 0, bs);

            return Encoding.Unicode.GetString(b);
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        unsafe string IStringVariable.Value
        {
            get
            {
                Debug.Assert(_lock != null, "_lock != null");
                Debug.Assert(VariableType == VariableType.String); 
                
                try
                {
                    _lock.EnterReadLock();

                    return UnsafeExtractStringVariableValue();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                Debug.Assert(VariableType == VariableType.String);
                Debug.Assert(_lock != null, "_lock != null");

                try
                {
                    _lock.EnterWriteLock();

                    if (_checkdups)
                        if (string.Equals(UnsafeExtractStringVariableValue(), value))
                            return;

                    var ptr = (SH*)_sharedpointer;

                    var v = value;
                    var ml = ptr->MaxSize;
                    if (v.Length > ml)
                        v = v.Substring(0, ml);

                    ptr->Size = (UInt16)v.Length;

                    var l = Encoding.Unicode.GetBytes(v);

                    Marshal.Copy(l, 0, new IntPtr(ptr + 1), l.Length);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
                _channel.AddChangedVariable(this);

                if (_autosend)
                    (_channel as IVariablesChannel)?.SendChanges(this);
            }
        }

        unsafe void IStringVariable.SetFromAnsi(void* Val)
        {
            Debug.Assert(VariableType == VariableType.String);

            ((IStringVariable)this).Value = Val == null ? "" : Marshal.PtrToStringAnsi(new IntPtr(Val));
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        int IIntVariable.Value
        {
            get
            {
                unsafe
                {
                    Debug.Assert(VariableType == VariableType.Int32);

                    return *((int*)_sharedpointer);
                }
            }
            set
            {
                unsafe
                {
                    Debug.Assert(VariableType == VariableType.Int32);

                    if (_checkdups)
                        if (*((int*)_sharedpointer) == value)
                            return;

                    *((int*)_sharedpointer) = value;
                }

                _channel.AddChangedVariable(this);

                if (_autosend)
                    (_channel as IVariablesChannel)?.SendChanges(this);
            }
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        unsafe Int64 ILongVariable.Value
        {
            get
            {
                Debug.Assert(VariableType == VariableType.Long);
                return *((Int64*)_sharedpointer);
            }
            set
            {
                Debug.Assert(VariableType == VariableType.Long);

                if (_checkdups)
                    if (*((Int64*)_sharedpointer) == value)
                        return;

                *((Int64*)_sharedpointer) = value;

                _channel.AddChangedVariable(this);

                if (_autosend)
                    (_channel as IVariablesChannel)?.SendChanges(this);
            }
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        void IKVariable.Set()
        {
            Debug.Assert(VariableType == VariableType.KMD);

            _channel.AddChangedVariable(this);

            if (_autosend)
                (_channel as IVariablesChannel)?.SendChanges(this);
        }

        /// <summary>
        /// Команда активирована
        /// </summary>
        /// <remarks>
        /// Чтение обнуляет флаг активации
        /// </remarks>
        bool IKVariable.IsFired => IsChanged;

        /// <summary>
        /// Значение переменной
        /// </summary>
        unsafe char ICharVariable.Value
        {
            get
            {
                Debug.Assert(VariableType == VariableType.Char);
                
                return *((char*)_sharedpointer);
            }
            set
            {
                Debug.Assert(VariableType == VariableType.Char);

                if (_checkdups)
                    if (*((char*)_sharedpointer) == value)
                        return;

                *((char*)_sharedpointer) = value;

                _channel.AddChangedVariable(this);

                if (_autosend)
                    (_channel as IVariablesChannel)?.SendChanges(this);
            }
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        unsafe bool IBoolVariable.Value
        {
            get
            {
                Debug.Assert(VariableType == VariableType.Boolean || VariableType == VariableType.WatchDog);

                return *((byte*)_sharedpointer) != 0;
            }
            set
            {
                Debug.Assert(VariableType == VariableType.Boolean);
                
                if (_checkdups)
                    if ((*((byte*)_sharedpointer) != 0) == value)
                        return;

                *((byte*)_sharedpointer) = value ? (byte)1 : (byte)0;

                _channel.AddChangedVariable(this);

                if (_autosend)
                    (_channel as IVariablesChannel)?.SendChanges(this);
            }
        }

        /// <summary>
        /// Инвертирует значение логической переменной
        /// </summary>
        unsafe void IBoolVariable.Toggle()
        {
            Debug.Assert(VariableType == VariableType.Boolean);

            *((byte*)_sharedpointer) = *((byte*)_sharedpointer) == 0 ? (byte)1 : (byte)0;

            _channel.AddChangedVariable(this);

            if (_autosend)
                (_channel as IVariablesChannel)?.SendChanges(this);
        }

        /// <inheritdoc />
        unsafe bool ITriggerVariable.Value
        {
            get
            {
                Debug.Assert(VariableType == VariableType.Boolean || VariableType == VariableType.WatchDog);

                return *((byte*)_sharedpointer) != 0;
            }

            set
            {
                Debug.Assert(VariableType == VariableType.Boolean);

                var bptr = (byte*)_sharedpointer;

                if (*bptr == 1)         // Значение можно только установить, но не сбросить
                    return;

                if (_checkdups)
                    if ((*bptr != 0) == value)
                        return;

                *bptr = value ? (byte)1 : (byte)0;

                _channel.AddChangedVariable(this);

                if (_autosend)
                    (_channel as IVariablesChannel)?.SendChanges(this);
            }
        }

        /// <inheritdoc />
        public unsafe void ResetTrigger()
        {
            Debug.Assert(VariableType == VariableType.Boolean);

            var bptr = (byte*)_sharedpointer;

            if (_checkdups)
                if (*bptr == 0)
                    return;

            *bptr = 0;

            _channel.AddChangedVariable(this);

            if (_autosend)
                (_channel as IVariablesChannel)?.SendChanges(this);
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        unsafe float IFloatVariable.Value
        {
            get
            {
                Debug.Assert(VariableType == VariableType.Single);

                return *((float*)_sharedpointer);
            }
            set
            {
                Debug.Assert(VariableType == VariableType.Single);

                if (_checkdups)
                {
                    if (float.IsNaN(*((float*)_sharedpointer)) && float.IsNaN(value))
                        return;

                    if (Math.Abs(*((float*)_sharedpointer) - value) < Threshold)
                        return;
                }

                *((float*)_sharedpointer) = value;

                _channel.AddChangedVariable(this);

                if (_autosend)
                    (_channel as IVariablesChannel)?.SendChanges(this);
            }
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        unsafe double IDoubleVariable.Value
        {
            get
            {
                Debug.Assert(VariableType == VariableType.Double);

                return *((double*)_sharedpointer);
            }
            set
            {
                Debug.Assert(VariableType == VariableType.Double);

                if (_checkdups)
                {
                    if (double.IsNaN(*((double*)_sharedpointer)) && double.IsNaN(value))
                        return;

                    if (Math.Abs(*((double*)_sharedpointer) - value) < Threshold)
                        return;
                }

                *((double*)_sharedpointer) = value;

                _channel.AddChangedVariable(this);

                if (_autosend)
                    (_channel as IVariablesChannel)?.SendChanges(this);
            }
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        unsafe byte[] IByteArrayVariable.Value
        {
            get
            {
                Debug.Assert(VariableType == VariableType.ByteArray);
                Debug.Assert(_lock != null, "_lock != null");

                try
                {
                    _lock.EnterReadLock();

                    var ah = (AH*)_sharedpointer;

                    var l = ah->Length;
                    var b = new byte[l];
                    _accessor.ReadArray(_sharedoffset + sizeof(AH), b, 0, l);
                    return b;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                Debug.Assert(VariableType == VariableType.ByteArray);
                Debug.Assert(_lock != null, "_lock != null");

                var ah = (AH*)_sharedpointer;

                var l = value.Length;

                try
                {
                    _lock.EnterWriteLock();

                    if (_checkdups)
                    {
                        fixed (byte* pt = &value[0])
                        {
                            var pt1 = pt;
                            var pt2 = (byte*)_sharedpointer + sizeof(AH);
                            if (ah->Length == (UInt16)l)
                            {
                                var c = false;
                                for (var i = 0; i < l; i++)
                                {
                                    if (*(pt1++) == *(pt2++))
                                        continue;

                                    c = true;
                                    break;
                                }

                                if (!c)
                                    return;
                            }
                        }
                    }

                    ah->Length = (UInt16)l;
                    _accessor.WriteArray(_sharedoffset + sizeof(AH), value, 0, l);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                _channel.AddChangedVariable(this);

                if (_autosend)
                    (_channel as IVariablesChannel)?.SendChanges(this);
            }
        }

        unsafe bool IWatchDogVariable.Value
        {
            get
            {
                Debug.Assert(VariableType == VariableType.WatchDog);
                return *((UInt16*)_sharedpointer) != 0;
            }
        }

        void IWatchDogVariable.Reset()
        {
            Debug.Assert(VariableType == VariableType.WatchDog);

            _channel.AddChangedVariable(this);

            if (_autosend)
                (_channel as IVariablesChannel)?.SendChanges(this);
        }

        unsafe void IWatchDogVariable.Reset(UInt16 Value)
        {
            Debug.Assert(VariableType == VariableType.WatchDog);

            *((UInt16*)_sharedpointer) = Value;
            _channel.AddChangedVariable(this);

            if (_autosend)
                (_channel as IVariablesChannel)?.SendChanges(this);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct WV
        {
            [FieldOffset(0)] 
            public readonly UInt16 TickCounter;

            [FieldOffset(2)]
            public bool Locked;
        }

        unsafe bool IWatchDogVariable.Locked
        {
            get
            {
                var p = (WV*)_sharedpointer;
                return p->Locked;
            }

            set
            {
                var p = (WV*)_sharedpointer;
                p->Locked = value;
            }
        }
        #endregion
    }
}
