using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class RowNumberConverter : IValueConverter
    {
        public static RowNumberConverter Instance { get; } = new RowNumberConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), out int index))
            {
                return index + 1;
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
