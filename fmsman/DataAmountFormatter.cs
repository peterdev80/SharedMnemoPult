using System;
using System.Windows.Data;
using System.Globalization;

namespace fmsman
{
    /// <summary>
    /// Осуществляет форматирование размера данных в формат КБ, МБ, ...
    /// </summary>
    public class DataAmountFormatter : IValueConverter
    {
        private const long KB = 1024;
        private const long MB = 1024 * KB;
        private const long GB = 1024 * MB;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            var amount = (long)value;

            if (amount > GB)
                return $"{amount / (double)GB:F3} ГБ"; 
            
            if (amount > MB)
                return $"{amount / (double)MB:F3} МБ";

            if (amount > KB)
                return $"{amount / (double)KB:F2} КБ";

            return $"{amount} Б";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
