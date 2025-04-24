using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;

using PinkPact.Helpers;

using static PinkPact.Helpers.MathHelper;

namespace PinkPact.Converters
{
    public class PointConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object param, CultureInfo culture)
        {
            try
            {
                if (!((string)param)[0].Equals('f')) return new Point(System.Convert.ToDouble(values[0]),
                                                                      System.Convert.ToDouble(values[1]));

                string[] point = string.Format(((string)param).Substring(1), values).SplitSequence(",", new Dictionary<char, char>() { { '(', ')' } });
                return new Point(System.Convert.ToDouble(Compute(point[0])),
                                 System.Convert.ToDouble(Compute(point[1])));
            }
            catch
            {
                return new Point();
            }
            finally
            { }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object param, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
