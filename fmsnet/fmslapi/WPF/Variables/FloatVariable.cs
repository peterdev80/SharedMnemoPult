using System;
using System.ComponentModel;
using System.Windows;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Числовая переменная
    /// </summary>
    public class FloatVariable : Variable
    {
        static FloatVariable()
        {
            ValueProperty.OverrideMetadata(typeof(FloatVariable), new FrameworkPropertyMetadata(Single.NaN));
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        [Browsable(false)]
        public new float Value
        {
            get => (float)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #region Неявные преобразования типа
        public static implicit operator int(FloatVariable v)
        {
            return (int)v.Value;
        }

        public static implicit operator float(FloatVariable v)
        {
            return v.Value;
        }

        public static implicit operator double(FloatVariable v)
        {
            return v.Value;
        }

        public static implicit operator decimal(FloatVariable v)
        {
            return (decimal)v.Value;
        }
        #endregion
    }
}
