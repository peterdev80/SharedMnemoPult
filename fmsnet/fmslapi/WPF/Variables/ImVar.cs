using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Мост между WPF и NativeVariable
    /// </summary>
    [DesignTimeVisible(false)]
    internal class ImVar : DependencyObject, IValueConverter
    {
        #region Частные данные
        private readonly IVariable _nv;
        private bool _avoidevt;
        private readonly Binding _b;

        // ReSharper disable once CollectionNeverQueried.Local
        private static readonly List<Variable> _cc = new List<Variable>();
        #endregion

        #region Свойства зависимостей
        public static readonly DependencyProperty NativeEpProperty = DependencyProperty.Register("NativeEp", typeof(object), typeof(ImVar));

        public object NativeEp
        {
            get => GetValue(NativeEpProperty);
            set => SetValue(NativeEpProperty, value);
        }
        #endregion

        public ImVar(IVariable NativeVariable)
        {
            _nv = NativeVariable;

            if (_nv == null)
                return;

            NativeEp = _nv.Value;

            _b = new Binding
            {
                Source = this,
                Path = new PropertyPath(NativeEpProperty),
                Mode = BindingMode.TwoWay,
                NotifyOnSourceUpdated = true,
                NotifyOnTargetUpdated = true,
                Converter = this
            };
        }

        public IVariable NativeVariable => _nv;

        public bool AvoidEvents
        {
            set => _avoidevt = value;
        }

        public void AttachWpfEp(Variable Target)
        {
            if (Target == null)
                return;

            _cc.Add(Target);

            Binding.AddSourceUpdatedHandler(Target, OnNativeEpUpdate);
            Binding.AddTargetUpdatedHandler(Target, OnWpfEpUpdate);

            BindingOperations.SetBinding(Target, Variable.ValueProperty, _b);
        }

        /// <summary>
        /// Передача значения переменной WPF -> Native
        /// </summary>
        private void OnNativeEpUpdate(object sender, DataTransferEventArgs e)
        {
            if (_nv == null)
                return;

            var dobj = sender as DependencyObject;
            if (dobj == null)
                return;

            var wv = e.TargetObject as Variable;

            Debug.Assert(wv != null, "wv != null");

            if (wv.CheckDups)
            {
                var v = _nv.Value;
                var nv = GetValue(NativeEpProperty);

                if (!v.Equals(nv))
                    _nv.Value = nv;
            }
            else
                _nv.Value = GetValue(NativeEpProperty);

            if (wv.AutoSend)
                wv.Manager.SendChanges(wv);

            if (wv.PersistentVariable)
                ThreadPool.QueueUserWorkItem(x => { Thread.Sleep(250); wv.SavePersistent(); });
        }

        /// <summary>
        /// Передача значения переменной Native -> WPF
        /// </summary>
        private void OnWpfEpUpdate(object sender, DataTransferEventArgs e)
        {
            var wv = e.TargetObject as Variable;
            Debug.Assert(wv != null, "wv != null");

            wv.IsChanged = true;

            var ae = _avoidevt;
            _avoidevt = false;

            if (ae)
                return;

            var ev = new VariableChangedEventArgs(Variable.VariableChangedEvent, wv);
            wv.RaiseEvent(ev);
        }

        #region Члены IValueConverter

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (_nv.VariableType)
            {
                case VariableType.Boolean: return Convert.ToBoolean(value);
                case VariableType.Int32: return Convert.ToInt32(value);
                case VariableType.Long: return Convert.ToInt64(value);
                case VariableType.Single: return Convert.ToSingle(value);
                case VariableType.Double: return Convert.ToDouble(value);
                case VariableType.Char: return Convert.ToChar(value);
                case VariableType.String: return Convert.ToString(value);
                case VariableType.WatchDog: return value is bool && (bool)value;
                case VariableType.ByteArray: return value as byte[];
                default: return Binding.DoNothing;
            }
        }

        #endregion
    }
}
