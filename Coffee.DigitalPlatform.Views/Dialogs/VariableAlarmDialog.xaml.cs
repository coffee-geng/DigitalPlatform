using Coffee.DigitalPlatform.Controls.FilterBuilder;
using Coffee.DigitalPlatform.Models;
using Coffee.DigitalPlatform.ViewModels;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.Views
{
    /// <summary>
    /// VariableAlarmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class VariableAlarmDialog : Window
    {
        public VariableAlarmDialog()
        {
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class VariableAlarmTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AlarmTemplate { get; set; }

        public DataTemplate NewTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null || !(item is Alarm alarmInfo))
            {
                return null;
            }
            if (alarmInfo.IsFirstEditing)
            {
                return NewTemplate;
            }
            else
            {
                return AlarmTemplate;
            }
        }
    }

    public class AlarmConditionViewModelConverter : IMultiValueConverter
    {
        public static AlarmConditionViewModelConverter Instance { get; } = new AlarmConditionViewModelConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            Alarm alarm = null;
            if (values[0] != DependencyProperty.UnsetValue && values[0] is Alarm)
            {
                alarm = (Alarm)values[0];
            }
            ConditionView conditionView = null;
            if (values[1] != DependencyProperty.UnsetValue && (values[1] is ConditionView))
            {
                conditionView = (ConditionView)values[1];
            }
            if (conditionView != null)
            {
                //ConditionView定义在Alarm列表控件的ItemTemplate中，会继承这个控件Item的DataContext：即Alarm对象
                //而ConditionView的ViewModel是ConditionViewModel，这是通过读取Alarm对象的ConditionTemplate转换成对应的ViewModel的
                if (conditionView.DataContext != null && conditionView.DataContext is ConditionViewModel)
                {
                    return conditionView.DataContext;
                }
                else
                {
                    return new ConditionViewModel(alarm.ConditionTemplate);
                }
            }
            else
            {
                return new ConditionViewModel(alarm.ConditionTemplate);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FilterSchemeToStringConverter : IMultiValueConverter
    {
        public static FilterSchemeToStringConverter Instance { get; } = new FilterSchemeToStringConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            if (values[0] == DependencyProperty.UnsetValue || !(values[0] is FilterScheme))
            {
                return null;
            }
            return (values[0] as FilterScheme).ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CurrentFilterSchemeChangedEventArgsConverter : IValueConverter
    {
        public static CurrentFilterSchemeChangedEventArgsConverter Instance { get; } = new CurrentFilterSchemeChangedEventArgsConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RoutedPropertyChangedEventArgs<FilterScheme> changedArgs)
            {
                return new ReceiveFilterSchemeArgs
                {
                    FilterScheme = changedArgs.NewValue as FilterScheme,
                    Receiver = parameter as IReceiveFilterScheme
                };
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
