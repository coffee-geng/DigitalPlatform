using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class DeviceComponentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return null;
            if (values[0] == DependencyProperty.UnsetValue || !(values[0] is string))
                return null;
            var componentTypeName = values[0];
            Canvas canvas = null;
            if (values.Length > 1)
            {
                if (values[1] != DependencyProperty.UnsetValue && values[1] is Canvas)
                {
                    canvas = (Canvas)values[1];
                }
            }

            var assembly = Assembly.Load("Coffee.DigitalPlatform.Components");
            Type t = assembly.GetType("Coffee.DigitalPlatform.Components." + componentTypeName)!;
            var obj = Activator.CreateInstance(t)!;
            if (new string[] { "WidthRuler", "HeightRuler" }.Contains(componentTypeName))
                return obj;

            // 组件生成
            var c = (ComponentBase)obj;

            Binding binding1 = new Binding();
            binding1.Path = new System.Windows.PropertyPath("."); //对组件对象进行布局的画布
            //在创建组件时，仅当外部画布对象作为绑定源数据传递时，才绑定组件ComponentBase的Canvas属性
            if (canvas != null)
            {
                binding1.Source = canvas;
                binding1.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }
            c.SetBinding(ComponentBase.CanvasProperty, binding1);

            Binding binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("IsSelected");
            c.SetBinding(ComponentBase.IsSelectedProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("Rotate");
            c.SetBinding(ComponentBase.RotateAngleProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("FlowDirection");
            c.SetBinding(ComponentBase.FlowDirectionProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("IsWarning");
            c.SetBinding(ComponentBase.IsWarningProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("WarningMessage.AlarmContent");
            c.SetBinding(ComponentBase.WarningMessageProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("IsMonitor");
            c.SetBinding(ComponentBase.IsMonitorProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("Variables");
            c.SetBinding(ComponentBase.VariableListProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ManualList");
            c.SetBinding(ComponentBase.ManualListProperty, binding);

            #region Command Binding
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("DeleteCommand");
            c.SetBinding(ComponentBase.DeleteCommandProperty, binding);

            binding = new Binding();
            //binding.Path = new System.Windows.PropertyPath(".");
            c.SetBinding(ComponentBase.DeleteParameterProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("DataContext.AlarmDetailCommand");
            binding.RelativeSource = new RelativeSource { AncestorType = typeof(Window) };
            c.SetBinding(ComponentBase.AlarmDetailCommandProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ManualControlCommand");
            c.SetBinding(ComponentBase.ManualControlCommandProperty, binding);
            #endregion

            return c;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
