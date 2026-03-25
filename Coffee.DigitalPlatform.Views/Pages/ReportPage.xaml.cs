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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Coffee.DigitalPlatform.Views
{
    /// <summary>
    /// ReportPage.xaml 的交互逻辑
    /// </summary>
    public partial class ReportPage : UserControl
    {
        public ReportPage()
        {
            InitializeComponent();

            this.Unloaded += ReportPage_Unloaded;
        }

        private void ReportPage_Unloaded(object sender, RoutedEventArgs e)
        {
            //因为页面切换的时候，是重新创建一个新的页面，也就是说页面中的DataGrid控件也会重新创建。
            //这个时候，如果卸载页面的DataGrid控件默认仍然是和ViewModel绑定的。
            //这个页面切换方式用的是多个页面实例绑定同一个共享的ViewModel，即使页面卸载不再使用，仍然是绑定状态。
            //这个时候，在OnNavigateTo方法中添加动态生成的Columns，会同时应用于所有与其绑定的DataGrid，也就会产生同一个列添加到了多个DataGrid的错误
            //所以在页面卸载的时候必须解除绑定
            BindingOperations.ClearAllBindings(this);
        }
    }
}
