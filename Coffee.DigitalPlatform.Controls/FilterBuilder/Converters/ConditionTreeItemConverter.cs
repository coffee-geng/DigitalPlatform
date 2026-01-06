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
    public class ConditionTreeItemConverter : IValueConverter
    {
        public static ConditionTreeItemConverter Instance { get; } = new ConditionTreeItemConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is null)
            {
                return Visibility.Collapsed;
            }

            return (string)parameter switch
            {
                "Group" => (value is ConditionGroup) ? Visibility.Visible : Visibility.Collapsed,
                "Expression" => (value is PropertyExpression) ? Visibility.Visible : Visibility.Collapsed,
                _ => throw new NotSupportedException(string.Format(LanguageHelper.GetString("FilterBuilder_Exception_Message_ParameterIsNotSupported_Pattern"), parameter))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
