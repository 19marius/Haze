using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq.Extensions;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;
using System;


namespace MessengerClient.Converters
{
    public class PathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object param, CultureInfo culture)
        {
            if (!(param as string)[0].Equals('f')) return null;

            //Compute the source string, keep command symbols
            return Geometry.Parse(string.Join(" ", string.Format((param as string).Substring(1), values).SplitSequence(" ", new Dictionary<char, char>() { { '(', ')' }, { '[', ']' } }).Select(x => Regex.IsMatch(x, @"^[a-zA-Z]$") ? x : Helpers.MathHelper.Compute(x).ToString())));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object param, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
