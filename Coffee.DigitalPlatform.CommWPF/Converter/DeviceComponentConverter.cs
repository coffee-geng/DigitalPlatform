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

namespace Coffee.DigitalPlatform.CommWPF
{
    public class DeviceComponentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == "HL")
            {
                return new Line
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = 2000,
                    Y2 = 0,
                    //Height = 0.5,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 3.0, 3.0 },
                    ClipToBounds = true,
                };
            }
            else if (value.ToString() == "VL")
            {
                return new Line
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = 0,
                    Y2 = 2000,
                    //Width = 0.5,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 3.0, 3.0 },
                    ClipToBounds = true,
                };
            }


            var assembly = Assembly.Load("Coffee.DigitalPlatform.Components");
            Type t = assembly.GetType("Coffee.DigitalPlatform.Components." + value.ToString())!;
            var obj = Activator.CreateInstance(t)!;
            if (new string[] { "WidthRule", "HeightRule" }.Contains(value.ToString()))
                return obj;


            // 组件生成
            var c = (ComponentBase)obj;

            Binding binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("DeleteCommand");
            c.SetBinding(ComponentBase.DeleteCommandProperty, binding);
            binding = new Binding();
            //binding.Path = new System.Windows.PropertyPath(".");
            c.SetBinding(ComponentBase.DeleteParameterProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("IsSelected");
            c.SetBinding(ComponentBase.IsSelectedProperty, binding);


            // 处理组件尺寸缩放命令逻辑绑定
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ResizeDownCommand");
            c.SetBinding(ComponentBase.ResizeDownCommandProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ResizeMoveCommand");
            c.SetBinding(ComponentBase.ResizeMoveCommandProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ResizeUpCommand");
            c.SetBinding(ComponentBase.ResizeUpCommandProperty, binding);


            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("Rotate");
            c.SetBinding(ComponentBase.RotateAngleProperty, binding);

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("FlowDirection");
            c.SetBinding(ComponentBase.FlowDirectionProperty, binding);


            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("IsWarning");// Model中的属性
            c.SetBinding(ComponentBase.IsWarningProperty, binding);// 组件中的依赖属性
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("WarningMessage.AlarmContent");// Model中的属性
            c.SetBinding(ComponentBase.WarningMessageProperty, binding);// 组件中的依赖属性

            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("IsMonitor");// Model中的属性
            c.SetBinding(ComponentBase.IsMonitorProperty, binding);// 组件中的依赖属性
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("VariableList");// Model中的属性
            c.SetBinding(ComponentBase.VarListProperty, binding);// 组件中的依赖属性
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ManualControlList");// Model中的属性
            c.SetBinding(ComponentBase.ControlListProperty, binding);// 组件中的依赖属性
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("ManualControlCommand");// Model中的属性
            c.SetBinding(ComponentBase.ManualControlCommandProperty, binding);// 组件中的依赖属性

            // xaml  C#
            // "{Binding RelativeSource={RelativeSource Type=Window},Path=DataContext.AlarmDetailCommand}"
            binding = new Binding();
            binding.Path = new System.Windows.PropertyPath("DataContext.AlarmDetailCommand");// Model中的属性
            binding.RelativeSource = new RelativeSource { AncestorType = typeof(Window) };
            c.SetBinding(ComponentBase.AlarmDetailCommandProperty, binding);// 组件中的依赖属性


            return c;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
