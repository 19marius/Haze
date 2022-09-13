using System.Globalization;
using HazeClient.Helpers;
using System.Windows.Data;
using System.Windows;
using System.Linq;
using System.Data;
using System;

namespace HazeClient.Converters
{
    public class ArithmeticConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object param, CultureInfo culture)
        {
            string op = param as string;

            //Check if any value not set
            if (values.Contains(DependencyProperty.UnsetValue)) return 0;

            //Unformatted
            if (!op[0].Equals('f'))
            {
                //Get the operator
                int opIndex = 0;

                //Aggregate the operations
                return values.Skip(1).Select(x => System.Convert.ToDouble(x)).Aggregate(System.Convert.ToDouble(values[0]), (total, next) =>
                {
                    //If there are no operators left, go through them again
                    switch (op[(opIndex++ > op.Length - 1 ? opIndex = 0 : opIndex - 1)])
                    {
                        //Signed cases

                        case '+':
                            return total + next;

                        case '-':
                            return total - next;

                        case '*':
                            return total * next;

                        case '/':
                            return total / next;

                        case '%':
                            return total % next;
                    }

                    return 0;
                });
            }

            //Formatted
            return MathHelper.Compute(string.Format(op.Substring(1), values));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object param, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
