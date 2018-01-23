using System;
using System.ComponentModel;
using System.Windows;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Числовая переменная двойной точности
    /// </summary>
    public class DoubleVariable : Variable
    {
        static DoubleVariable()
        {
            ValueProperty.OverrideMetadata(typeof(DoubleVariable), new FrameworkPropertyMetadata(Double.NaN));
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        [Browsable(false)]
        public new double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #region Неявные преобразования типа
        public static implicit operator int(DoubleVariable v)
        {
            return (int)v.Value;
        }

        public static implicit operator float(DoubleVariable v)
        {
            return (float)v.Value;
        }

        public static implicit operator double(DoubleVariable v)
        {
            return v.Value;
        }

        public static implicit operator decimal(DoubleVariable v)
        {
            return (decimal)v.Value;
        }
        #endregion
    }
}
