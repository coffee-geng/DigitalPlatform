using Coffee.DigitalPlatform.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class UserTypeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return null;
            if (value is UserTypes)
            {
                return ((UserTypes)value).GetDisplayName();
            }
            else if (value is byte || value is short || value is int || value is long)
            {
                return ((UserTypes)value).GetDisplayName();
            }
            else if (value is string)
            {
                if (Enum.TryParse(typeof(UserTypes), value.ToString(), out object? result) && result is UserTypes)
                {
                    return ((UserTypes)result).GetDisplayName();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
