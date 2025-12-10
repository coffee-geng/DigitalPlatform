using Coffee.DigitalPlatform.CommWPF;
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
        }

        private void ItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            ItemsControl control = sender as ItemsControl;
            if (control == null)
                return;
            Panel panel = ItemsControlExtensions.GetItemsPanel(control);
            ItemsControlExtensions.SetLayoutContainer(control, panel);
        }
    }
}
