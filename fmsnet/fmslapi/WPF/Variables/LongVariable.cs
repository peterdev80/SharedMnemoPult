using System;
using System.Windows;
using System.ComponentModel;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Целочисленная переменная размером 64 бита
    /// </summary>
    public class LongVariable : Variable
    {
        static LongVariable()
        {
            ValueProperty.OverrideMetadata(typeof(LongVariable), new FrameworkPropertyMetadata((Int64)9999));
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        [Browsable(false)]
        public new Int64 Value
        {
            get => (Int64)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #region Неявные преобразования типа
        public static implicit operator int(LongVariable v)
        {
            return Convert.ToInt32(v.Value);
        }

        public static implicit operator float(LongVariable v)
        {
            return v.Value;
        }

        public static implicit operator double(LongVariable v)
        {
            return v.Value;
        }

        public static implicit operator decimal(LongVariable v)
        {
            return v.Value;
        }
        #endregion
    }
}
