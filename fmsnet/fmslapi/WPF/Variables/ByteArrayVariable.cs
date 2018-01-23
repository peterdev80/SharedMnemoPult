using System.Windows;
using System.ComponentModel;

namespace fmslapi.WPF.Variables
{    
    /// <summary>
    /// Байтовый массив
    /// </summary>
    public class ByteArrayVariable : Variable
    {
        static ByteArrayVariable()
        {
            ValueProperty.OverrideMetadata(typeof(ByteArrayVariable), new FrameworkPropertyMetadata(new byte[0]));
        }

        /// <summary>
        /// Значение переменной
        /// </summary>
        [Browsable(false)]
        public new byte[] Value
        {
            get => (byte[])GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
    }
}
