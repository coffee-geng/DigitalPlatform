using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    [ValueConversion(typeof(object), typeof(DateTime?))]
    public class EnsureDateTimeValueConverter : IValueConverter
    {
        public static EnsureDateTimeValueConverter Instance { get;  } = new EnsureDateTimeValueConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime time)
            {
                return null;
            }

            return time;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(object), typeof(TimeSpan?))]
    public class EnsureTimeSpanValueConverter : IValueConverter
    {
        public static EnsureTimeSpanValueConverter Instance { get; } = new EnsureTimeSpanValueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TimeSpan time)
            {
                return null;
            }

            return time;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
