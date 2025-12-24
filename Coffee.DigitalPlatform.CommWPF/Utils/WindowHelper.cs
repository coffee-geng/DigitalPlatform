using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class WindowHelper
    {
        public static bool GetClose(DependencyObject obj)
        {
            return (bool)obj.GetValue(CloseProperty);
        }

        public static void SetClose(DependencyObject obj, bool value)
        {
            obj.SetValue(CloseProperty, value);
        }

        public static readonly DependencyProperty CloseProperty =
            DependencyProperty.RegisterAttached(
                "Close",
                typeof(bool),
                typeof(WindowHelper),
                new PropertyMetadata(false, new PropertyChangedCallback((d, e) =>
                {
                    if ((bool)e.NewValue)
                        (d as Window).Close();
                })));
    }
}
