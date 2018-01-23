using System.ComponentModel;
using System.Windows;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Логическая переменная
    /// </summary>
    public class BooleanVariable : Variable
    {
        static BooleanVariable()
        {
            ValueProperty.OverrideMetadata(typeof(BooleanVariable), new FrameworkPropertyMetadata(false));
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        [Browsable(false)]
        public new virtual bool Value
        {
            get => (bool)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Инвертирует значение логической переменной
        /// </summary>
        public void Toggle()
        {
            Value = !Value;
        }

        #region Неявные преобразования типов
        public static implicit operator bool(BooleanVariable v)
        {
            return v.Value;
        }

        public static implicit operator string(BooleanVariable v)
        {
            return v.Value ? "True" : "False";
        }

        public static implicit operator char(BooleanVariable v)
        {
            return v.Value ? 'T' : 'F';
        }
        #endregion
    }
}
