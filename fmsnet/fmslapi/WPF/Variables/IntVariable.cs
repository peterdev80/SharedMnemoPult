using System.ComponentModel;
using System.Windows;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Целочисленная переменная
    /// </summary>
    public class IntVariable : Variable
    {
        static IntVariable()
        {
            ValueProperty.OverrideMetadata(typeof(IntVariable), new FrameworkPropertyMetadata(9999));
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        [Browsable(false)]
        public new int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #region Неявные преобразования типа
        public static implicit operator int(IntVariable v)
        {
            return v.Value;
        }

        public static implicit operator float(IntVariable v)
        {
            return v.Value;
        }

        public static implicit operator double(IntVariable v)
        {
            return v.Value;
        }

        public static implicit operator decimal(IntVariable v)
        {
            return v.Value;
        }
        #endregion
    }
}
