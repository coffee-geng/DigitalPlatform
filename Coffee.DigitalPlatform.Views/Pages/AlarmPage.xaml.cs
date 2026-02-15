using Coffee.DigitalPlatform.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
}
