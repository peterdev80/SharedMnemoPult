using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace fmslapi.Bindings.WPF
{
    /// <summary>
    /// Базовый класс универсальной привязки XAML
    /// </summary>
    public abstract partial class BaseValueBinding : MarkupExtension
    {
#if DEBUG
        private class dbm : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
#endif

        #region Частные данные

        private Bridge _bridge;

        /// <summary>
        /// Источник данных
        /// </summary>
        private IValueSource _valuesource;

        /// <summary>
        /// Направление привязки
        /// </summary>
        private BindingMode _bindingMode = BindingMode.Default;

        /// <summary>
        /// Привязка загружена и инициализирована
        /// </summary>
        private bool _loaded;

        protected IValueConverter _customconverter;

        protected ServiceProvider _provider;

        public VariablesDataContext DataContexet { get; private set; }
        #endregion

        #region Публичные свойства

        /// <summary>
        /// Направление привязки
        /// </summary>
        public BindingMode BindingMode
        {
            get => _bindingMode;
            set => _bindingMode = value;
        }

        #endregion
        
        public abstract IValueSource GetSource();

        public override object ProvideValue(IServiceProvider sp)
        {
            _provider = new ServiceProvider(sp);

            var to = _provider.TargetObject;

#if DEBUG
            var tp = _provider.TargetProperty as DependencyProperty;

            try
            {
                if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
                    throw new OperationCanceledException();
            }
            catch (Exception)
            {
                switch (to)
                {
                    case TriggerBase _: 
                    case Condition _:
                    case SetterBase _:
                        return new Binding { Converter = new dbm() };
                }

                if (tp == null)
                    return null;

                var pt = tp.PropertyType;

                if (pt.IsAssignableFrom(typeof(Binding)))
                    return new Binding();

                return pt.IsValueType ? Activator.CreateInstance(pt) : null;
            }
#endif
            
            if (to != null && to.GetType().Name.Contains("SharedDp"))
                return this;

            switch (to)
            {
                case null:
                case ResourceDictionary _:
                    return GetSource();

                case SetterBase _:
                case TriggerBase _:
                case Condition _:
                    return ProvideValueForSetters(sp);

                case IAcceptValueSource cvs when cvs.CheckAcceptValueSource(_provider.TargetProperty):
                    return ProvideValueSourceForSetters(_provider);

                case FrameworkElement _:
                case FrameworkContentElement _:
                    return ProvideValueForFrameworkElements(sp);

                case DependencyObject _:
                    return ProvideValueForAloneDependencyObjects(sp);
            }

            throw new ArgumentException("Невозможно создать биндинг");
        }

        protected object ProvideValue(DependencyObject Target, DependencyProperty TargetProperty)
        {
            _provider = new ServiceProvider(null) { TargetObject = Target };

            if (TargetProperty != null)
                _provider.TargetProperty = TargetProperty;

            if (Target is FrameworkElement || Target is FrameworkContentElement)
                return ProvideValueForFrameworkElements();

            // ReSharper disable once IsExpressionAlwaysTrue
            if (Target is DependencyObject)
                return ProvideValueForAloneDependencyObjects(_provider);

            throw new ArgumentException("Невозможно создать биндинг");
        }

        private void OnTargetLoaded(object Sender, RoutedEventArgs Args)
        {
            if (_loaded)
                return;

            switch (Sender)
            {
                case FrameworkElement fe:
                    fe.Loaded -= OnTargetLoaded;
                    break;

                case FrameworkContentElement fce:
                    fce.Loaded -= OnTargetLoaded;
                    break;
            }

            var tgt = Sender as DependencyObject;

            var vdc = VariablesDataContext.GetVariablesDataContext(tgt);

            DataContexet = vdc;

            if (_bridge != null)
                _valuesource.ValueChanged += nv => _bridge.Value = nv?.Value;
            
            _loaded = true;

            _valuesource.Init(tgt, vdc);
            _valuesource.UpdateTarget();
        }

        private object InternalConvert(object val, string fs, Type targetType)
        {
            if (targetType == typeof(string))
            {
                if ((val as double?) > 1e36)
                    return "";

                if ((val as float?) > 1e36)
                    return "";

                return string.Format($"{{0:{fs}}}", val);
            }

            if (targetType.IsInstanceOfType(val))
                return val;

            if (targetType == typeof(double))
                return Convert.ToDouble(val);

            if (targetType == typeof(float))
                return Convert.ToSingle(val);

            if (targetType.IsEnum)
                return Enum.Parse(targetType, val.ToString(), true);

            if (targetType != typeof(string) && val is string)
            {
                var s = val.ToString();

                var attr = targetType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
                foreach (TypeConverterAttribute at in attr)
                {
                    var type = Type.GetType(at.ConverterTypeName);

                    if (type == null)
                        continue;

                    var cvt = Activator.CreateInstance(type) as TypeConverter;
                    
                    switch (cvt) 
                    {
                        case null:
                            continue;
                        case ImageSourceConverter _:
                            return new BitmapImage(new Uri(s, UriKind.Relative));
                    }

                    return cvt.ConvertFromString(s);
                }
            }

            if (targetType == typeof(bool) && val is string sv)
            {
                switch (sv.ToLowerInvariant())
                {
                    case "true":
                        return true;
                    case "false":
                        return false;
                }
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return Convert.ToInt32(val) != 0;

            throw new ArgumentException("Невозможно сконвертировать");
        }

        public void EnsureDataContext(VariablesDataContext DC)
        {
            if (DC != null)
                DataContexet = DC;
        }
    }
}
