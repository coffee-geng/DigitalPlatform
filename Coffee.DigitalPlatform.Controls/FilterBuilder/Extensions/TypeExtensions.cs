using System;
using System.Linq;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{

    /// <summary>
    /// Type class extensions methods
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets non nullable type used to create nullable type.
        /// </summary>
        /// <param name="type">Nullable Type to retrieve non nullable parameter</param>
        /// <returns></returns>
        public static Type GetNonNullable(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var genericArguments = type.GetGenericArguments();
            return type.IsNullable() ? genericArguments.Single() : type;
        }

        /// <summary>
        /// Checks if type is instance of nullable struct
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns></returns>
        public static bool IsNullable(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsNullableType(this Type type)
        {
            if ((object)type == null)
            {
                return false;
            }

            if (!type.IsValueType)
            {
                return true;
            }

            if ((object)Nullable.GetUnderlyingType(type) != null)
            {
                return true;
            }

            return false;
        }

        public static bool IsBasicType(this Type type)
        {
            if ((object)type == null)
            {
                return false;
            }

            if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid))
            {
                return true;
            }

            if (type.IsNullableType())
            {
                Type underlyingType = Nullable.GetUnderlyingType(type);
                if ((object)underlyingType != null)
                {
                    return underlyingType.IsBasicType();
                }
            }

            return false;
        }
    }
}