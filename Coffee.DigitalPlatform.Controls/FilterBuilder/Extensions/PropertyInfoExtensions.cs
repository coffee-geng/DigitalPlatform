using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    internal static class PropertyInfoExtensions
    {
        public static string? GetDisplayName(this PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);

            if (propertyInfo.TryGetAttribute<DisplayNameAttribute>(out var catelDispNameAttr))
            {
                return catelDispNameAttr.DisplayName;
            }

            return propertyInfo.TryGetAttribute<DisplayAttribute>(out var dispAttr)
                ? dispAttr.GetName()
                : null;
        }

        /// <summary>
        /// Tries to the get attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type.</typeparam>
        /// <param name="memberInfo">The member Info.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>
        /// <c>true</c> if the attribute is retrieved successfully; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="memberInfo"/> is <c>null</c>.</exception>
        public static bool TryGetAttribute<TAttribute>(this MemberInfo memberInfo, [NotNullWhen(true)] out TAttribute? attribute)
            where TAttribute : Attribute
        {
            ArgumentNullException.ThrowIfNull(memberInfo);

            var result = TryGetAttribute(memberInfo, typeof(TAttribute), out var tempAttribute);

            attribute = tempAttribute as TAttribute;
            return result;
        }

        /// <summary>
        /// Tries to the get attribute.
        /// </summary>
        /// <param name="memberInfo">The member Info.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>
        ///   <c>true</c> if the attribute is retrieved successfully; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="memberInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="attributeType"/> is <c>null</c>.</exception>
        public static bool TryGetAttribute(this MemberInfo memberInfo, Type attributeType, [NotNullWhen(true)] out Attribute? attribute)
        {
            ArgumentNullException.ThrowIfNull(memberInfo);
            ArgumentNullException.ThrowIfNull(attributeType);

            attribute = null;
            var attributes = memberInfo.GetCustomAttributes(attributeType, false) as Attribute[];

            if ((attributes is not null) && (attributes.Length > 0))
            {
                attribute = attributes[0];
                return true;
            }

            return false;
        }
    }
}
