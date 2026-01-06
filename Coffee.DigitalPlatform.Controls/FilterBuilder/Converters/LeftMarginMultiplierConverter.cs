using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{ 
    public class LeftMarginMultiplierConverter : IValueConverter
    {
        private static Thickness DefaultThickness = new(0);

        public double Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is not TreeViewItem item
            ? DefaultThickness
            : new Thickness(Length * item.GetDepth(), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
