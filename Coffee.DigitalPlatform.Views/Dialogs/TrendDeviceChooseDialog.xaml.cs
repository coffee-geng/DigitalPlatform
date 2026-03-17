using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Coffee.DigitalPlatform.Views
{
    /// <summary>
    /// TrendDeviceChooseDialog.xaml 的交互逻辑
    /// </summary>
    public partial class TrendDeviceChooseDialog : Window
    {
        public TrendDeviceChooseDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class TypeToNameConverter : IValueConverter
    {
        public static TypeToNameConverter Instance { get; } = new TypeToNameConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue || value is not Type)
                return string.Empty;
            return ((Type)value).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
