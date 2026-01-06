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
            if (item == null)
            {
                return NewTemplate;
            }
            else if (item is Alarm)
            {
                return AlarmTemplate;
            }
            else
            {
                return null;
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
            Dictionary<Alarm, FilterSchemeEditInfo> alarmInfoDict = new Dictionary<Alarm, FilterSchemeEditInfo>();
            if (values[0] != DependencyProperty.UnsetValue && values[0] is Alarm)
            {
                alarm = (Alarm)values[0];
            }
            if (values[1] != DependencyProperty.UnsetValue && (values[1] is Dictionary<Alarm, FilterSchemeEditInfo>))
            {
                alarmInfoDict = values[1] as Dictionary<Alarm, FilterSchemeEditInfo>;
            }
            if (alarm == null)
            {
                if (values.Length < 3 || values[2] == DependencyProperty.UnsetValue || !(values[2] is FilterSchemeEditInfo defaultScheme))
                    return null;
                return new ConditionViewModel(defaultScheme);
            }
            if (alarmInfoDict.TryGetValue(alarm, out FilterSchemeEditInfo filterSchemeEditInfo) && filterSchemeEditInfo != null)
            {
                return new ConditionViewModel(filterSchemeEditInfo);
            }
            else
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
