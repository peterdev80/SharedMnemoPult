using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace fmslapi.Bindings.WPF
{
	public partial class BaseValueBinding
	{
	    private class feconverters : IMultiValueConverter
	    {
	        private readonly BaseValueBinding _p;

	        public feconverters(BaseValueBinding P)
	        {
	            _p = P;
	        }

	        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	        {
	            var v = values[0];
	            var tgt = values[2];

	            _p.EnsureDataContext(values[1] as VariablesDataContext);

	            var vdc = _p.DataContexet;

                if (vdc == null || tgt == null)
                    return Binding.DoNothing;

	            if (!_p._loaded)
                    return Binding.DoNothing;

                if (_p._customconverter != null)
                    return _p._customconverter.Convert(v, targetType, vdc, culture);

                if (v == null)
                    return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

	            return _p.InternalConvert(v, vdc.FormatString ?? "0", targetType);
	        }

	        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	        {
                if (_p._loaded)
                    _p._valuesource.UpdateSource(value);

                return new[] { Binding.DoNothing, Binding.DoNothing, Binding.DoNothing };
	        }
	    }

	    private object ProvideValueForFrameworkElements(IServiceProvider SP)
	    {
	        var mb = ProvideValueForFrameworkElements() as BindingBase;

	        Debug.Assert(mb != null, "mb != null");

	        return mb.ProvideValue(SP);
	    }

	    private object ProvideValueForFrameworkElements()
        {
            var to = _provider.TargetObject;

	        var unvc = Attribute.GetCustomAttribute(to.GetType(), typeof(UseNamedVariablesContextAttribute));

	        var hasn = unvc != null;

            _valuesource = new UpdateControl(GetSource());

	        _valuesource.ValueChanged += nv => _bridge.Value = nv?.Value;

            var mb = ProvdeBindingForFrameworkElement();
            mb.Converter = new feconverters(this);

            switch (to)
            {
                case FrameworkElement fe:
                    if (fe.IsLoaded || hasn)
                        OnTargetLoaded(fe, null);
                    else
                        fe.Loaded += OnTargetLoaded;
                    break;

                case FrameworkContentElement fce:
                    if (fce.IsLoaded || hasn)
                        OnTargetLoaded(fce, null);
                    else
                        fce.Loaded += OnTargetLoaded;
                    break;
            }

            return mb;
        }

	    private MultiBinding ProvdeBindingForFrameworkElement()
	    {
            var mb = new MultiBinding
            {
                Mode = _bindingMode
            };

            _bridge = new Bridge();

            mb.Bindings.Add(new Binding
            {
                Source = _bridge,
                Path = new PropertyPath(nameof(Bridge.Value)),
            });

            mb.Bindings.Add(new Binding
            {
                RelativeSource = new RelativeSource { Mode = RelativeSourceMode.Self },
                Path = new PropertyPath(VariablesDataContext.VariablesDataContextProperty)
            });

            mb.Bindings.Add(new Binding
            {
                RelativeSource = new RelativeSource { Mode = RelativeSourceMode.Self },
                Path = new PropertyPath(".")
            });

            return mb;
	    }
	}
}
