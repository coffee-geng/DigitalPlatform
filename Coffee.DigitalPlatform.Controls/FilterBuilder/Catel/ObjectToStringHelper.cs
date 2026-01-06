using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public static class ObjectToStringHelper
    {
        //
        // 摘要:
        //     Gets or sets the default culture to use for parsing.
        //
        // 值:
        //     The default culture.
        public static CultureInfo? DefaultCulture { get; set; }

        //
        // 摘要:
        //     Initializes static members of the Catel.StringToObjectHelper class.
        static ObjectToStringHelper()
        {
            DefaultCulture = CultureInfo.InvariantCulture;
        }

        //
        // 摘要:
        //     Returns a System.String that represents the instance.
        //
        //     If the instance is null, this method will return "null". This method is great
        //     when the value of a property must be logged.
        //
        // 参数:
        //   instance:
        //     The instance, can be null.
        //
        // 返回结果:
        //     A System.String that represents the instance.
        public static string ToString(object? instance)
        {
            return ToString(instance, DefaultCulture);
        }

        //
        // 摘要:
        //     Returns a System.String that represents the instance.
        //
        //     If the instance is null, this method will return "null". This method is great
        //     when the value of a property must be logged.
        //
        // 参数:
        //   instance:
        //     The instance, can be null.
        //
        //   cultureInfo:
        //     The culture information.
        //
        // 返回结果:
        //     A System.String that represents the instance.
        public static string ToString(object? instance, CultureInfo? cultureInfo)
        {
            if (instance == null)
            {
                return "null";
            }

            if (instance == DBNull.Value)
            {
                return "dbnull";
            }

            Type type = instance.GetType();
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return ((DateTime)instance).ToString(cultureInfo);
            }

            MethodInfo methodEx = type.GetMethod("ToString", TypeArray.From<IFormatProvider>());
            if ((object)methodEx != null)
            {
                string text = (string)methodEx.Invoke(instance, new object[1] { cultureInfo });
                if (text != null)
                {
                    return text;
                }
            }

            return instance.ToString() ?? "null";
        }

        //
        // 摘要:
        //     Returns a System.String that represents the type name of the instance.
        //
        //     If the instance is null, this method will return "null". This method is great
        //     when the value of a property must be logged.
        //
        // 参数:
        //   instance:
        //     The instance.
        //
        // 返回结果:
        //     A System.String that represents the type of the instance.
        public static string ToTypeString(object? instance)
        {
            if (instance == null)
            {
                return "null";
            }

            if (instance is Type type)
            {
                return type.Name;
            }

            return instance.GetType().Name;
        }

        //
        // 摘要:
        //     Returns a System.String that represents the full type name of the instance.
        //
        //     If the instance is null, this method will return "null". This method is great
        //     when the value of a property must be logged.
        //
        // 参数:
        //   instance:
        //     The instance.
        //
        // 返回结果:
        //     A System.String that represents the type of the instance.
        public static string ToFullTypeString(object? instance)
        {
            if (instance == null)
            {
                return "null";
            }

            Type type = instance as Type;
            if ((object)type == null)
            {
                type = instance.GetType();
            }

            return type.FullName;
        }
    }
}
