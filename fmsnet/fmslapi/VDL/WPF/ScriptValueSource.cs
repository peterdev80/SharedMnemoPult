using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using fmslapi.Bindings;
using fmslapi.Bindings.WPF;

namespace fmslapi.VDL.WPF
{
    /// <summary>
    /// Привязка к скрипту VDL
    /// </summary>
    public class ScriptValueSource : DispatcherObject, IValueSource
    {
        #region Частные данные

        /// <summary>
        /// Имя скрипта
        /// </summary>
        private readonly string _scriptName;

        /// <summary>
        /// Явно заданные аргументы
        /// </summary>
        private readonly object[] _arguments;

        private readonly IServiceProvider _provider;

        /// <summary>
        /// Привязки к аргументам скрипта
        /// </summary>
        private IValueSource[] _parsources;

        /// <summary>
        /// Скрипт VDL
        /// </summary>
        private VDLScript _script;

        /// <summary>
        /// Текущие значения аргументов
        /// </summary>
        private object[] _parvals;
        
        /// <summary>
        /// Не вызывать подписчиков скрипта
        /// </summary>
        private bool _silent;

        private static readonly IValue _donothing = new DoNothing();

        #endregion

        #region Частные типы

        public class DoNothing : IValue
        {
            public object Value => null;
        }

        private sealed class valvalsource : IValueSource
        {
            private readonly IValue _val;

            public valvalsource(object Val)
            {
                _val = Val as IValue ?? new Value(Val);
            }

            public void Init(object AttachedTo, VariablesDataContext DataContext)
            {
            }

            public event SourceValueChanged ValueChanged;

            public void UpdateTarget()
            {
                OnValueChanged(_val);
            }

            public void UpdateSource(object NewValue)
            {
            }

            private void OnValueChanged(IValue Newvalue)
            {
                ValueChanged?.Invoke(Newvalue);
            }

            public IValue Value => _val;

            public Type ValueType => null;
        }

        private sealed class bindingsource : DependencyObject, IValueSource
        {
            private static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                "Value", typeof(object), typeof(bindingsource), new PropertyMetadata(default(object)));

            public IValue Value 
            {
                get => (IValue)GetValue(ValueProperty);
                // ReSharper disable once UnusedMember.Local
                set => SetValue(ValueProperty, value);
            }

            public Type ValueType => null;

            public bindingsource(BindingBase BSrc)
            {
                BindingOperations.SetBinding(this, ValueProperty, BSrc);
            }

            public void Init(object AttachedTo, VariablesDataContext DataContext)
            {
                var pd = DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(bindingsource));

                pd.AddValueChanged(this, doc);
            }

            private void doc(object Sender, EventArgs E)
            {
                UpdateTarget();
            }

            public event SourceValueChanged ValueChanged;
            public void UpdateTarget()
            {
                OnValueChanged(Value);
            }

            public void UpdateSource(object NewValue)
            {
                throw new NotImplementedException();
            }

            private void OnValueChanged(IValue Newvalue)
            {
                ValueChanged?.Invoke(Newvalue);
            }
        }

        private class ScriptValueSourceFactory : IValueSourceFactory
        {
            public IValueSource CreateValueSource(string Source)
            {
                if (!Source.StartsWith("%"))
                    return null;

                return new ScriptValueSource(Source.Remove(0, 1));
            }
        }

        #endregion

        #region Конструкторы

        public ScriptValueSource(string ScriptName, IServiceProvider Provider = null, object[] Arguments = null)
        {
            _scriptName = ScriptName;
            _arguments = Arguments;
            _provider = Provider as ServiceProvider;
        }

        #endregion

        #region Инициализация привязки

        public void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            _script = VDLRuntime.GetScript(_scriptName);

            if (_arguments != null && _arguments.Length != _script.ParamsCount)
                throw new InvalidOperationException("Несоответствие количества параметров скрипта");

            var prs = new List<IValueSource>();

            _parvals = new object[_script.ParamsCount];

            if (_arguments == null)
            {
                foreach (var par in _script.GetParamNames())
                {
                    var parsrc = VariableValueSource.Create(par);

                    string a1, a2;

                    _script.GetParamAdditional(par, out a1, out a2);

                    var dc = DataContext;
                    if (a1 != null)
                    {
                        dc = DataContext.Clone(AttachedTo as DependencyObject);
                        dc.VariablesChannelName = a1;
                    }

                    parsrc.Init(AttachedTo, dc);

                    prs.Add(parsrc);
                }
            }
            else
            {
                foreach (var ar in _arguments)
                {
                    IValueSource vs;
                    if (ar is IValueSource) vs = ar as IValueSource;
                    else if (ar is BindingBase) 
                        vs = new bindingsource(ar as BindingBase);
                    else vs = new valvalsource(ar);

                    vs.Init(AttachedTo, DataContext);

                    prs.Add(vs);
                }
            }

            var i = 0;
            foreach (var vs in prs)
            {
                var i1 = i++;
                vs.ValueChanged += NewValue =>
                                   {
                                       _parvals[i1] = NewValue?.Value;

                                       if (!_silent)
                                           UpdateTarget();
                                   };
            }

            _parsources = prs.ToArray();
        }

        #endregion

        public event SourceValueChanged ValueChanged;
        
        public void UpdateTarget()
        {
            var h = ValueChanged;
            if (h == null)
                return;

            var r = Value;

            if (!(r is DoNothing))
                h(r);
        }

        public IValue Value
        {
            get
            {
                if (_parsources == null)
                    return null;

                _silent = true;

                foreach (var s in _parsources)
                    s.UpdateTarget();

                _silent = false;

                if (_parvals.Any(x => x == null))
                    return _donothing;

                var r = _script.Execute(_parvals, null);

                if (_script.ReturnType == Types.DynamicResource)
                    r = (new DynamicResourceExtension(r).ProvideValue(_provider));

                return new Value(r);
            }
        }

        public Type ValueType => null;

        public void UpdateSource(object NewValue)
        {
            // Скрипты работают только в одну сторону
            throw new InvalidOperationException();
        }

        public static void RegisterFactory()
        {
            VariableValueSource.RegisterValueSourceFactory(new ScriptValueSourceFactory());
        }
    }
}
