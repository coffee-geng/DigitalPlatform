using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.ViewModels;
using System;
using System.Collections.Generic;
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
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            if (new LoginWindow().ShowDialog() != true)
            {
                Application.Current.Shutdown();
                return;
            }

            InitializeComponent();

            ActionManager.Register<object>("ShowConfigureComponentDialog", new Func<object, bool>(showConfigureComponentDialog));
        }

        private bool showConfigureComponentDialog(object dataContext)
        {
            return showDialog(new ConfigureComponentDialog()
            {
                Owner = this,
                DataContext = dataContext
            });
        }

        private bool showDialog(Window dialog)
        {
            bool result = dialog.ShowDialog() == true;
            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
