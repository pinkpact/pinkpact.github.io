using System.Globalization;
using System.Windows.Data;
using System;

namespace PinkPact.Converters
{
    public class StringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object param, CultureInfo culture)
        {
            return string.Format((param as string).Substring(1), values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object param, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
