using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class RankingValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return 0;
            var value = double.Parse(values[0].ToString());
            var total = double.Parse(values[1].ToString());
            var width = double.Parse(values[2].ToString());
            return value / total * width;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
