using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;
using fmslapi.Annotations;
using fmslapi.UpdateTriggers;
using ch = fmslapi.Channel;

namespace fmslapi
{
    /// <summary>
    /// Базовый класс всех переменных
    /// </summary>
#if DEBUG
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
#endif
    internal partial class Variable : IStringVariable, IIntVariable, ILongVariable, ITriggerVariable,
                                      IKVariable, ICharVariable, IBoolVariable, 
                                      IFloatVariable, IDoubleVariable, IByteArrayVariable,
                                      IWatchDogVariable, INotifyPropertyChanged
    {
        #region Частные данные
        private readonly IVariablesChannelSupport _channel;
        private readonly string _name;
        private int _vindex;
        private VariableType _type = VariableType.Unknown;

        private static readonly Dictionary<string, Tuple<MemoryMappedFile, MemoryMappedViewAccessor>> Shmems = new Dictionary<string, Tuple<MemoryMappedFile, MemoryMappedViewAccessor>>();
        private static readonly ReaderWriterLockSlim Shlock = new ReaderWriterLockSlim();
        private MemoryMappedViewAccessor _accessor;
        private int _sharedoffset;
        private bool _autosend;
        private bool _checkdups;
        private unsafe void* _sharedpointer;
        private bool _ischanged;
        private ManualResetEvent _chevt;
        private bool _needlocalfeedback;

        private TriggerBase _updtrigger;

        private CrossProcessReaderWriterLock _lock;
        #endregion

        #region Конструкторы

        internal Variable(IVariablesChannelSupport Channel, string Name)
        {
            _name = Name;
            _channel = Channel;
            AssignToSharedMemory("fakememory", 0);
            Channel.RegisterVariable(this);
        }

        #endregion

        #region Публичные свойства
        /// <summary>
        /// Сразу присылать событие об изменении переменной обратно отправителю
        /// </summary>
        public bool NeedLocalFeedback
        {
            get => _needlocalfeedback;
            set => _needlocalfeedback = value;
        }

        /// <summary>
        /// Автоматическая отправка изменений в канал при изменении этой переменной
        /// </summary>
        public bool AutoSend
        {
            get => _autosend;
            set => _autosend = value;
        }

        /// <summary>
        /// Проверять и игнорировать при отправке одно и то же значение
        /// </summary>
        public bool CheckDups
        {
            get => _checkdups;
            set => _checkdups = value;
        }

        /// <summary>
        /// Индекс переменной в глобальной карте переменных
        /// </summary>
        public int Index
        {
            get => _vindex;
            set
            {
                if (_vindex != value && _vindex != 0)
                    throw new InvalidOperationException("Попытка повторной установки индекса переменной");
                
                _vindex = value;
            }
        }

        /// <summary>
        /// Имя переменной
        /// </summary>
        public string VariableName => _name;

        /// <summary>
        /// Тип переменной
        /// </summary>
        public VariableType VariableType
        {
            get => _type;
            internal set
            {
                if (_type != VariableType.Unknown && _type != value)
                    throw new InvalidOperationException(@"Попытка изменения типа переменной");

                _type = value;
            }
        }

        public float Threshold;
        #endregion

        #region Общие события
        public event VariableChanged VariableChanged;

        public void RaiseVariableChanged(bool IsInit)
        {
            _channel.RaiseDelegate(VariableChanged, this, IsInit);

            OnPropertyChanged("Value");
        }
        #endregion

        #region Методы ожидания
        /// <summary>
        /// Блокирует текущий поток до получения уведомления об изменении переменной
        /// </summary>
        /// <returns>true</returns>
        public bool WaitOne()
        {
            return WaitOne(-1);
        }

        /// <summary>
        /// Блокирует текущий поток до получения уведомления об изменении переменной
        /// </summary>
        /// <param name="MillisecondsTimeout">Время ожидания в миллисекундах</param>
        /// <returns>true в случае изменения переменной</returns>
        public bool WaitOne(int MillisecondsTimeout)
        {
            lock (this)
            {
                if (_ischanged)
                {
                    _ischanged = false;
                    return true;
                }

                if (_chevt == null)
                    _chevt = new ManualResetEvent(false);
            }

            return _chevt.WaitOne(MillisecondsTimeout);
        }

        /// <summary>
        /// Блокирует текущий поток до получения уведомления об изменении переменной
        /// </summary>
        /// <param name="Timeout">Время ожидания</param>
        /// <returns>true в случае изменения переменной</returns>
        public bool WaitOne(TimeSpan Timeout)
        {
            return WaitOne(Timeout.Milliseconds);
        }

        internal void Set()
        {
            lock (this)
            {
                _ischanged = true;
                _chevt?.Set();
            }
        }

        public void Reset()
        {
            lock (this)
            {
                _ischanged = false;
                _chevt?.Reset();
            }
        }

        /// <summary>
        /// Сохраняет значение переменной в постоянном хранилище
        /// </summary>
        public void SavePersistent()
        {
            _channel.SavePersistentVariable(this);
        }
        #endregion

        public bool IsChanged
        {
            get
            {
                try
                {
                    _chevt?.Reset();

                    lock (this)
                        return _ischanged;
                }
                finally
                {
                    _ischanged = false;
                }
            }
        }

        internal Variable CheckValid()
        {
            return _vindex == -1 ? null : this;
        }

        internal void AssignToSharedMemory(string shmemname, int SharedOffset)
        {
            try
            {
                Shlock.EnterUpgradeableReadLock();

                if (!Shmems.TryGetValue(shmemname, out var shm))
                {
                    var sh = shmemname == "fakememory" ? MemoryMappedFile.CreateOrOpen(shmemname, 128) : MemoryMappedFile.OpenExisting(shmemname);
                    var sha = sh.CreateViewAccessor();

                    try
                    {
                        Shlock.EnterWriteLock();
                        shm = Tuple.Create(sh, sha);
                        Shmems.Add(shmemname, shm);
                    }
                    finally
                    {
                        Shlock.ExitWriteLock();
                    }
                }

                _accessor = shm.Item2;
                _sharedoffset = SharedOffset;
                unsafe
                {
                    byte* origin = null;
                    shm.Item2.SafeMemoryMappedViewHandle.AcquirePointer(ref origin);

                    origin += _sharedoffset;

                    if (VariableType == VariableType.ByteArray || VariableType == VariableType.String)
                        _lock = new CrossProcessReaderWriterLock(origin);

                    _sharedpointer = origin;
                }
            }
            finally
            {
                Shlock.ExitUpgradeableReadLock();
            }
        }

        #region Триггеры обновления
        /// <summary>
        /// Триггер обновления значения переменной
        /// </summary>
        public TriggerBase UpdateTrigger 
        {
            get
            {
                lock (this)
                {
                    return _updtrigger;
                }
            }

            set
            {
                lock (this)
                {
                    if (ReferenceEquals(_updtrigger, value))
                        return;

                    _updtrigger?.RemoveVariable(this);

                    _updtrigger = value;

                    _updtrigger?.AddVariable(this);
                }
            }
        }
        #endregion

        #region Визуализация отладки
#if DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{_name} = {((IVariable)this).Value} ({_type})";
#endif
        #endregion

        internal ch.Channel Channel => _channel as ch.Channel;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string PropertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
    }
}
