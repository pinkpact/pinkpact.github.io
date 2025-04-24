using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;
using System.Windows;
using System;

using PinkPact.Helpers;

using static PinkPact.Helpers.MathHelper;


namespace PinkPact.Converters
{
    public class PathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object param, CultureInfo culture)
        {
            if (!(param as string)[0].Equals('f')) return null;

            //Compute the source string, keep command symbols
            return Geometry.Parse(string.Join(" ", string.Format((param as string).Substring(1), values).SplitSequence(" ", new Dictionary<char, char>() { { '(', ')' }, { '[', ']' } }).Select(x => Regex.IsMatch(x, @"^[a-zA-Z]$") ? x : string.Join(",", x.Split(',').Select(x0 => Compute(x0).ToString())))));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object param, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
