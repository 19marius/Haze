using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Data;
using System;


namespace MessengerClient.Converters
{
    public class ConditionalConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object param, CultureInfo culture)
        {
            if (!(param as string)[0].Equals('f')) return null;

            string expression = string.Format(Regex.Replace(param as string, " +|^f", ""), values),
                   condition = Regex.Match(expression, @"(?<=[^><=!]+)([><]=?|!=|==)(?=[^><=!]+)").Value;

            var op0 = Regex.Match(expression, @"^[^><=!]+(?=[><]=?|!=|==)").Value;
            var op1 = Regex.Match(expression, @"(?<=[><]=?|!=|==)[^><=!]+(?=\?)").Value;
            var op2 = Regex.Match(expression, @"(?<=\?)[^><=!]+(?=\:)").Value;
            var op3 = Regex.Match(expression, @"(?<=\:)[^><=!]+$").Value;

            return condition.Contains("==") ? op0 == op1 ? op2 : op3 :
                   condition.Contains("!=") ? op0 != op1 ? op2 : op3 :
                   condition.Contains("<") ? ((IComparable)op0).CompareTo(op1) < 0 ? op2 : op3 :
                   condition.Contains("<=") ? ((IComparable)op0).CompareTo(op1) <= 0 ? op2 : op3 :
                   condition.Contains(">") ? ((IComparable)op0).CompareTo(op1) > 0 ? op2 : op3 :
                   condition.Contains(">=") ? ((IComparable)op0).CompareTo(op1) >= 0 ? op2 : op3 :
                   null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object param, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
