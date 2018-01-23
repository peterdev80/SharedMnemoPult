using System.ComponentModel;
using System.Windows;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Строковая переменная
    /// </summary>
    public class StringVariable : Variable
    {
        static StringVariable()
        {
            ValueProperty.OverrideMetadata(typeof(StringVariable), new FrameworkPropertyMetadata("<!НЕВЕРНАЯ ПЕРЕМЕННАЯ!>"));
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        [Browsable(false)]
        public new string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
    }
}
