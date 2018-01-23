using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace fmslapi.Bindings.WPF
{
    public partial class BaseValueBinding
    {
        private class ado2 : IValueConverter
        {
            private readonly BaseValueBinding _p;

            public ado2(BaseValueBinding P)
            {
                _p = P;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                _p._valuesource.UpdateSource(value);

                return Binding.DoNothing;
            }
        }

        private object ProvideValueForAloneDependencyObjects(IServiceProvider SP)
        {
            _valuesource = GetSource();

            _bridge = new Bridge();

            var to = _provider.TargetObject as DependencyObject;

            switch (_provider.RootObject)
            {
                case FrameworkElement fro:
                    if (fro.IsLoaded)
                        FroOnLoaded(fro, null);
                    else
                        fro.Loaded += FroOnLoaded;

                    _valuesource.UpdateTarget();

                    return new Binding
                           {
                               Source = _bridge,
                               Path = new PropertyPath(nameof(Bridge.Value)),
                               Converter = new ado2(this),
                               Mode = _bindingMode
                           }.ProvideValue(SP);

                case ResourceDictionary resdict:
                    _valuesource.ValueChanged += nv => _bridge.Value = nv?.Value;
                    _valuesource.Init(_provider.TargetObject, resdict[typeof(VariablesDataContext)] as VariablesDataContext);
                    _valuesource.UpdateTarget();

                    return new Binding
                           {
                               Source = _bridge,
                               Path = new PropertyPath(nameof(Bridge.Value)),
                               Converter = new ado2(this)
                           }.ProvideValue(SP);
            }

            if (to == null || Attribute.GetCustomAttribute(to.GetType(), typeof(UseNamedVariablesContextAttribute)) == null)
                throw new ArgumentException("Невозможно сконвертировать");

            _valuesource.ValueChanged += nv => _bridge.Value = nv?.Value;
            _valuesource.Init(to, to.GetVariablesDataContext());
            _valuesource.UpdateTarget();

            var b = new Binding
                    {
                        Source = _bridge,
                        Path = new PropertyPath(nameof(Bridge.Value)),
                        Converter = new ado2(this)
                    };

            return b.ProvideValue(null);
        }

        private void FroOnLoaded(object Sender, RoutedEventArgs Args)
        {
            var fe = Sender as FrameworkElement;

            if (fe != null)
                fe.Loaded -= FroOnLoaded;

            _valuesource.ValueChanged += nv => _bridge.Value = nv?.Value;
            _valuesource.Init(fe, fe.GetVariablesDataContext());
        }
    }
}
