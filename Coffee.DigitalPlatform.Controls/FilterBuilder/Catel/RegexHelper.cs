using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public static class RegexHelper
    {
        public static bool IsValid(string pattern)
        {
            ArgumentNullException.ThrowIfNull(pattern);

            try
            {
                new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1))
                    .IsMatch(string.Empty);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
