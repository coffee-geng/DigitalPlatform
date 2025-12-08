using Coffee.DigitalPlatform.DataAccess;
using Coffee.DigitalPlatform.IDataAccess;
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
    /// ConfigureComponentDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigureComponentDialog : Window
    {
        public ConfigureComponentDialog()
        {
            InitializeComponent();

            this.DataContext = new ViewModels.ConfigureComponentViewModel(new LocalDataAccess());
        }
    }

    public class AA : IValueConverter
    {
        public static AA Instance = new AA();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
