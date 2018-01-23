using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace fmslapi.Bindings.WPF
{
    /// <summary>
    /// Контроль обновления цели привязки по таймеру
    /// </summary>
    public class UpdateControl : IValueSource
    {
        #region Частные данные

        private static long _instcnt;

        private readonly long _instanceid = _instcnt++;
        private readonly IValueSource _source;

        private IValue _val;

        private static DispatcherTimer _timer;

        private static readonly Dictionary<long, WeakReference<UpdateControl>> _dirtylst = new Dictionary<long, WeakReference<UpdateControl>>();

        #endregion

        #region События

        /// <summary>
        /// Событие изменения значения источника привязки
        /// </summary>
        public event SourceValueChanged ValueChanged;

        #endregion

        static UpdateControl()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += OnTick;
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Start();
        }

        public UpdateControl(IValueSource Source)
        {
            _source = Source;
        }

        /// <inheritdoc />
        public void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            if (_source == null)
                return;

            if (DataContext == null)
                return;

            _source.ValueChanged += nv =>
                                    {
                                        _val = nv;

                                        lock (_dirtylst)
                                            _dirtylst[_instanceid] = new WeakReference<UpdateControl>(this);
                                    };

            _source.Init(AttachedTo, DataContext);
        }

        private static void OnTick(object Sender, EventArgs E)
        {
            UpdateControl[] lst;

            lock (_dirtylst)
            {
                lst = _dirtylst.Values.Select(x => x.Target).Where(x => x != null).ToArray();
                _dirtylst.Clear();
            }

            foreach (var uc in lst)
                uc.UpdateTarget();
        }

        public void UpdateTarget()
        {
            ValueChanged?.Invoke(Value);
        }

        /// <inheritdoc />
        public IValue Value => _val ?? _source.Value;

        /// <inheritdoc />
        public Type ValueType => _source?.ValueType;

        /// <inheritdoc />
        public void UpdateSource(object NewValue)
        {
            _source.UpdateSource(NewValue);
        }
    }
}
