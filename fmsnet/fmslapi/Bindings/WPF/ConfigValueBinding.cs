using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace fmslapi.Bindings.WPF
{
    /// <summary>
    /// Источник данных из конфигурационного файла
    /// </summary>
    public class ConfigValueBindingSource : IValueSource
    {
        private readonly string _key;
        private IConfigSection _cs;

        public ConfigValueBindingSource(string Key)
        {
            _key = Key;
        }

        public void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            _cs = DataContext.Manager.DefaultSection;

            var dio = AttachedTo as DispatcherObject;

            if (dio != null)
                DataContext.Manager.OnConfigReload += () =>
                    dio.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _cs = DataContext.Manager.DefaultSection;
                        UpdateTarget();
                    }));
        }

        public event SourceValueChanged ValueChanged;

        public void UpdateTarget()
        {
            var v = new Value(_cs[_key]);

            OnValueChanged(v);
        }

        public void UpdateSource(object NewValue)
        {
            throw new InvalidOperationException("Конфигурация работает в одну сторону");
        }

        protected virtual void OnValueChanged(IValue Newvalue)
        {
            ValueChanged?.Invoke(Newvalue);
        }

        public IValue Value => new Value(_cs?[_key]);

        public Type ValueType => typeof(string);
    }

    public class ConfigValue : BaseValueBinding
    {
        private class ConfigValueSourceFactory : IValueSourceFactory
        {
            public IValueSource CreateValueSource(string Source)
            {
                if (!Source.StartsWith("#"))
                    return null;

                return new ConfigValueBindingSource(Source.Remove(0, 1));
            }
        }

        private class coconv : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (targetType == typeof(Visibility) && value == null)
                    return Visibility.Collapsed;

                var vs = value?.ToString().Trim().ToLower();

                if (targetType == typeof(Visibility))
                    return vs == "1" || vs == "on" || vs == "true" || vs == "yes"
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                if (targetType == typeof(double))
                    return System.Convert.ToDouble(value);

                if (targetType == typeof(float))
                    return System.Convert.ToSingle(value);

                if (targetType == typeof(int))
                    return System.Convert.ToInt32(value);

                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new InvalidOperationException("Конфигурация работает в одну сторону");
            }
        }

        private readonly string _key;
        private static readonly IValueConverter _coconv = new coconv();

        public ConfigValue(string Key)
        {
            _key = Key;
            _customconverter = _coconv;
        }

        public override IValueSource GetSource()
        {
            return new ConfigValueBindingSource(_key);
        }

        public static void RegisterFactory()
        {
            VariableValueSource.RegisterValueSourceFactory(new ConfigValueSourceFactory());
        }
    }
}
