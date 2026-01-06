using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class ValueControlTypeVisibilityConverter : IValueConverter
    {
        public static ValueControlTypeVisibilityConverter Instance { get; } = new ValueControlTypeVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ValueControlType controlType)
            {
                return Visibility.Collapsed;
            }

            if (parameter is not null && parameter.GetType().IsArray)
            {
                var parameterType = (ValueControlType[])parameter;
                return parameterType.Contains(controlType) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (parameter is ValueControlType valueControlType)
            {
                return controlType == valueControlType ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
