using System.ComponentModel;
using System.Windows;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Символьная переменная
    /// </summary>
    public class CharVariable : Variable
    {
        static CharVariable()
        {
            ValueProperty.OverrideMetadata(typeof(CharVariable), new FrameworkPropertyMetadata('#'));
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        [Browsable(false)]
        public new char Value
        {
            get => (char)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #region Неявные преобразования типов
        public static implicit operator char(CharVariable v)
        {
            return v.Value;
        }

        public static implicit operator string(CharVariable v)
        {
            return new string(v.Value, 1);
        }
        #endregion
    }
}
