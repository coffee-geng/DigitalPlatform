
using System.ComponentModel;
using System.Windows;

namespace Coffee.DigitalPlatform.CommWPF
{
    public static class DesignTimeHelper
    {
        public static bool IsInDesignMode
        {
            get
            {
                // 方法1：使用 DesignerProperties
                if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                    return true;

                // 方法2：检查 LicenseManager（备用方法）
                return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            }
        }
    }

}
