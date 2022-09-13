using System.Collections.Generic;
using System.Linq.Extensions;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;

namespace HazeClient.Converters
{
    public class ThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object param, CultureInfo culture)
        {
            try
            {
                if (values.Length == 1 && !((string)param)[0].Equals('f')) return new Thickness(System.Convert.ToDouble(values[0]));

                if (!((string)param)[0].Equals('f')) return new Thickness(System.Convert.ToDouble(values[0]),
                                                                         System.Convert.ToDouble(values[1]),
                                                                         System.Convert.ToDouble(values[2]),
                                                                         System.Convert.ToDouble(values[3]));

                string[] margin = string.Format(((string)param).Substring(1), values).SplitSequence(",", new Dictionary<char, char>() { { '(', ')' } });
                return new Thickness(System.Convert.ToDouble(Helpers.MathHelper.Compute(margin[0])),
                                     System.Convert.ToDouble(Helpers.MathHelper.Compute(margin[1])),
                                     System.Convert.ToDouble(Helpers.MathHelper.Compute(margin[2])),
                                     System.Convert.ToDouble(Helpers.MathHelper.Compute(margin[3])));
            }
            catch
            {
                return new Thickness();
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
