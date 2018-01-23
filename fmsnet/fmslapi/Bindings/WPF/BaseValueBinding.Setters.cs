using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace fmslapi.Bindings.WPF
{
    public partial class BaseValueBinding
    {
        private class seconverters : IMultiValueConverter
        {
            private readonly BaseValueBinding _p;

            public seconverters(BaseValueBinding P)
            {
                _p = P;
            }

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var v = values[0];
                var vdc = values[1] as VariablesDataContext;
                var tgt = values[2];

                if (tgt != null && !_p._loaded)
                {
                    var fe = tgt as FrameworkElement;
                    if (fe != null)
                    {
                        if (fe.IsLoaded)
                            _p.OnTargetLoaded(fe, null);
                        else
                            fe.Loaded += _p.OnTargetLoaded;
                    }

                    var fce = tgt as FrameworkContentElement;
                    if (fce != null)
                    {
                        if (fce.IsLoaded)
                            _p.OnTargetLoaded(fce, null);
                        else
                            fce.Loaded += _p.OnTargetLoaded;
                    }

                    if (fe == null && fce == null)
                        throw new ArgumentException("Невозможно сконвертировать");

                    return Binding.DoNothing;
                }

                if (vdc == null || tgt == null)
                    return Binding.DoNothing;

                if (v == null)
                    return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

                return _p._customconverter == null
                    ? _p.InternalConvert(v, vdc.FormatString ?? "0", targetType)
                    : _p._customconverter.Convert(v, targetType, null, culture);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                if (_p._loaded)
                    _p._valuesource.UpdateSource(value);

                return new[] { Binding.DoNothing, Binding.DoNothing, Binding.DoNothing };
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private object ProvideValueForSetters(IServiceProvider SP)
        {
            _valuesource = new UpdateControl(GetSource());

            _valuesource.ValueChanged += nv => _bridge.Value = nv?.Value;

            var mb = ProvdeBindingForFrameworkElement();
            mb.Converter = new seconverters(this);

            return mb;
        }

        private IValueSource ProvideValueSourceForSetters(ServiceProvider SP)
        {
            _valuesource = new UpdateControl(GetSource());

            switch (SP.TargetObject)
            {
                case FrameworkElement fe:
                    if (fe.IsLoaded)
                        OnTargetLoaded(fe, null);
                    else
                        fe.Loaded += OnTargetLoaded;
                    break;

                case FrameworkContentElement fce:
                    if (fce.IsLoaded)
                        OnTargetLoaded(fce, null);
                    else
                        fce.Loaded += OnTargetLoaded;
                    break;
            }

            return _valuesource;
        }
    }
}
