using Coffee.DigitalPlatform.Controls.FilterBuilder;
using Coffee.DigitalPlatform.Models;
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
    /// ControlInfoByTriggerDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ControlInfoByTriggerDialog : Window
    {
        public ControlInfoByTriggerDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class ControlInfoByTriggerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ControlInfoTemplate { get; set; }

        public DataTemplate NewTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null || !(item is ControlInfoByTrigger controlInfo))
            {
                return null;
            }
            if (controlInfo.IsFirstEditing)
            {
                return NewTemplate;
            }
            else
            {
                return ControlInfoTemplate;
            }
        }
    }

    public class LinkageConditionViewModelConverter : IMultiValueConverter
    {
        public static LinkageConditionViewModelConverter Instance { get; } = new LinkageConditionViewModelConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            ControlInfoByTrigger controlInfo = null;
            if (values[0] != DependencyProperty.UnsetValue && values[0] is ControlInfoByTrigger)
            {
                controlInfo = (ControlInfoByTrigger)values[0];
            }
            ConditionView conditionView = null;
            if (values[1] != DependencyProperty.UnsetValue && (values[1] is ConditionView))
            {
                conditionView = (ConditionView)values[1];
            }
            if (conditionView != null)
            {
                //ConditionView定义在联动控制列表控件的ItemTemplate中，会继承这个控件Item的DataContext：即联控对象
                //而ConditionView的ViewModel是ConditionViewModel，这是通过读取联控对象的ConditionTemplate转换成对应的ViewModel的
                if (conditionView.DataContext != null && conditionView.DataContext is ConditionViewModel)
                {
                    return conditionView.DataContext;
                }
                else
                {
                    return new ConditionViewModel(controlInfo.ConditionTemplate);
                }
            }
            else
            {
                return new ConditionViewModel(controlInfo.ConditionTemplate);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DeviceNumToNameConverter : IMultiValueConverter
    {
        public static DeviceNumToNameConverter Instance { get; } = new DeviceNumToNameConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                return null;
            if (!(values[0] is string deviceNum) || string.IsNullOrWhiteSpace(deviceNum))
                return null;
            if (!(values[1] is IEnumerable<Device> deviceInfos))
                return null;
            var device = deviceInfos.FirstOrDefault(d => d.DeviceNum == deviceNum);
            return device?.Name;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
