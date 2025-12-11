using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class SumConverter : IMultiValueConverter
    {
        public static SumConverter Instance { get; } = new SumConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return 0;
            double sum = 0;
            foreach (var value in values)
            {
                if (value == null)
                    continue;
                if (double.TryParse(value.ToString(), out double d))
                {
                    sum += d;
                }
            }
            if (targetType == typeof(byte))
                return (byte)sum;
            else if (targetType == typeof(short))
                return (short)sum;
            else if (targetType == typeof(int))
                return (int)sum;
            else if (targetType == typeof(long))
                return (long)sum;
            else if (targetType == typeof(float))
                return (float)sum;
            else if (targetType == typeof(double))
                return sum;
            else if (targetType == typeof(string))
                return sum.ToString();
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
