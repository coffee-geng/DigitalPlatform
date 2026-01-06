using Coffee.DigitalPlatform.Controls.Properties;
using System.Globalization;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class TextResourceConverter : IValueConverter
    {
        public static TextResourceConverter Instance { get; } = new TextResourceConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = (string)value;
            return FilterBuilderResource.ResourceManager.GetString(key);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
