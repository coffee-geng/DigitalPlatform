using Coffee.DigitalPlatform.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Coffee.DigitalPlatform.Views
{
    /// <summary>
    /// AlarmPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmPage : UserControl
    {
        public AlarmPage()
        {
            InitializeComponent();
        }

        private void alarmDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }

    public class AlarmStateNameConverter : IValueConverter
    {
        public static AlarmStateNameConverter Instance { get; } = new AlarmStateNameConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            AlarmStatus status = AlarmStatus.Unknown;
            if (value != null && value != DependencyProperty.UnsetValue && value is AlarmStatus)
            {
                status = (AlarmStatus)value;                
            }
            string displayName = value.GetType().GetField(Enum.GetName(typeof(AlarmStatus), value))?.GetCustomAttribute<DisplayAttribute>()?.GetName();
            return displayName;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UserIdConverter : IMultiValueConverter
    {
        public static UserIdConverter Instance { get; } = new UserIdConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return string.Empty;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue || !(values[0] is string userId))
                return string.Empty;
            if (values[1] == null || values[1] == DependencyProperty.UnsetValue || !(values[1] is IEnumerable<User> userList))
                return string.Empty;
            var user = userList.FirstOrDefault(usr => usr.UserName == userId);
            return user != null ? user.RealName : userId;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
