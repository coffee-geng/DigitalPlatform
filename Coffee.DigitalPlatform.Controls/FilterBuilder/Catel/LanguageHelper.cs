using Coffee.DigitalPlatform.Controls.Properties;
using System.Globalization;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class LanguageHelper
    {
        public static string GetString(string resource)
        {
            if (resource == null)
                return string.Empty;
            return FilterBuilderResource.ResourceManager.GetString(resource, CultureInfo.CurrentCulture);
        }
    }
}
