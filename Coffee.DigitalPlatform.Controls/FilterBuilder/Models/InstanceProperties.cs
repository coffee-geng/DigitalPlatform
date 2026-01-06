using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class InstanceProperties : IPropertyCollection
    {
        public InstanceProperties(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var finalProperties = new Dictionary<string, IPropertyMetadata>();

            var regularProperties = new List<PropertyInfo>();
            regularProperties.AddRange(type.GetProperties().Where(m => m.CanRead && InstancePropertyHelper.IsSupportedType(m)));

            foreach (var property in regularProperties.Distinct())
            {
                finalProperties[property.Name] = new PropertyMetadata(type, property);
            }
            Properties = new List<IPropertyMetadata>(finalProperties.Values.OrderBy(m => m.Name));
        }

        public List<IPropertyMetadata> Properties { get; }

        public IPropertyMetadata? GetProperty(string propertyName)
        {
            return (from property in Properties
                    where string.Equals(property.Name, propertyName)
                    select property).FirstOrDefault();
        }
    }

    public static class InstancePropertyHelper
    {
        private static readonly HashSet<Type> SupportedTypes;

        private static readonly HashSet<Type> UnsupportedTypes;

        static InstancePropertyHelper()
        {
            UnsupportedTypes = new HashSet<Type>
        {
            typeof(bool?),
            typeof(TimeSpan?)
        };

            SupportedTypes = new HashSet<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(ushort),
            typeof(short),
            typeof(uint),
            typeof(int),
            typeof(ulong),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };
        }

        public static bool IsSupportedType(this IPropertyMetadata property)
        {
            ArgumentNullException.ThrowIfNull(property);

            return IsSupportedType(property.Type);
        }

        public static bool IsSupportedType(this PropertyInfo property)
        {
            ArgumentNullException.ThrowIfNull(property);

            return IsSupportedType(property.PropertyType);
        }

        public static bool IsSupportedType(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (UnsupportedTypes.Contains(type))
            {
                return false;
            }

            if (type.IsNullableType())
            {
                type = type.GetNonNullable();
            }

            return SupportedTypes.Contains(type) || type.IsEnum;
        }
    }
}
