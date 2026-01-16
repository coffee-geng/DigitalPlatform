using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace Coffee.DigitalPlatform.CommWPF
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public static BoolToVisibilityConverter Instance { get; } = new BoolToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool)value;
            if (parameter != null && parameter is string)
            {
                if ((parameter as string).Trim().ToLower() == "not")
                {
                    b = !b;
                }
            }
            switch (b)
            {
                case true:
                    return Visibility.Visible;
                case false:
                    return Visibility.Collapsed;
                default:
                    return Visibility.Hidden;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (Visibility)value;
            switch (v)
            {
                case Visibility.Collapsed:
                    return false;
                case Visibility.Hidden:
                    return false;
                case Visibility.Visible:
                    return true;
                default:
                    return false;
            }
        }

    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class NotBooleanConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new NotBooleanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }
    }
}
