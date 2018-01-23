using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using fmslapi.VDL.WPF;

namespace fmslapi.Bindings.WPF
{
    /// <summary>
    /// Привязка к переменным fmsldr
    /// </summary>
    public class VariableValueSource : IValueSource
    {
        private class V : IValueMetadata
        {
            private readonly IVariable _v;

            public V(IVariable V)
            {
                _v = V;
            }

            public object Value => _v?.Value;

            public string[] MetadataNames { get; } = { "Type" };

            public object GetMetadata(string Name)
            {
                return Name != "Type" ? null : _v?.VariableType;
            }
        }

        #region Частные данные

        /// <summary>
        /// Имя переменной
        /// </summary>
        private string _variableName;

        /// <summary>
        /// Таблица для создания переменной
        /// </summary>
        private readonly string _managerKey;

        /// <summary>
        /// Переменная
        /// </summary>
        private IVariable _variable;

        private bool _ispersistent;
        
        private static readonly List<IValueSourceFactory> _factories = new List<IValueSourceFactory>();

        #endregion

        #region Конструкторы

        static VariableValueSource()
        {
            ScriptValueSource.RegisterFactory();
            ConfigValue.RegisterFactory();
        }

        private VariableValueSource(string VariableName, string ManagerKey = null)
        {
            _variableName = VariableName;
            _managerKey = ManagerKey;
        }

        public static IValueSource Create(string SrcName, string Key = null)
        {
            foreach (var f in _factories)
            {
                var vs = f.CreateValueSource(SrcName);

                if (vs != null)
                    return vs;
            }

            return new VariableValueSource(SrcName, Key);
        }

        #endregion

        public void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            var dc = DataContext;
            if (_managerKey != null)
            {
                dc = dc.Clone(AttachedTo as DependencyObject);
                dc.VariablesChannelName = _managerKey;
            }

            try
            {
                if (_variableName.EndsWith("^"))
                {
                    // Запрошена сохраняемая переменная
                    _variableName = _variableName.Substring(0, _variableName.Length - 1);

                    _ispersistent = true;
                }

                _variable = dc.VariablesChannel.GetVariable(_variableName);
                _variable.AutoSend = true;
                _variable.NeedLocalFeedback = true;

                _variable.VariableChanged += VariableOnVariableChanged;
            }
            catch (NullReferenceException) { }
        }

        private void VariableOnVariableChanged(IVariable Sender, bool IsInit)
        {
            OnValueChanged(new V(Sender));
        }

        public IValue Value => new V(_variable);

        public Type ValueType => null;

        public event SourceValueChanged ValueChanged;
        
        public void UpdateTarget()
        {
            if (_variable != null)
                OnValueChanged(new V(_variable));
        }

        public void UpdateSource(object NewValue)
        {
            if (_variable == null)
                return;

            var nv = NewValue;

            switch (_variable.VariableType)
            {
                case VariableType.Single:
                    if (nv is string)
                    {
                        float.TryParse(nv.ToString().Replace(",", "."), NumberStyles.Float,
                            CultureInfo.InvariantCulture, out var fv);

                        nv = fv;
                    }
                    else
                        nv = Convert.ToSingle(nv);
                    break;

                case VariableType.Double:
                    if (nv is string)
                    {
                        double.TryParse(nv.ToString().Replace(",", "."), NumberStyles.Float,
                            CultureInfo.InvariantCulture, out var dv);

                        nv = dv;
                    }
                    else
                        nv = Convert.ToDouble(nv);
                    break;

                case VariableType.Int32:
                    nv = Convert.ToInt32(nv);
                    break;

                case VariableType.Boolean:
                    if (!(nv is bool))
                    {
                        var bsv = nv.ToString().Trim().ToLowerInvariant();
                        nv = bsv == "1" || bsv == "on" || bsv == "true" || bsv == "yes";
                    }
                    break;
            }

            _variable.Value = nv;
            UpdateTarget();

            if (_ispersistent)
                _variable.SavePersistent();
        }

        protected virtual void OnValueChanged(IValue NewValue)
        {
            ValueChanged?.Invoke(NewValue);
        }

        public static void RegisterValueSourceFactory(IValueSourceFactory Factory)
        {
            _factories.Add(Factory);
        }
    }

    public interface IValueSourceFactory
    {
        IValueSource CreateValueSource(string Source);
    }
}
