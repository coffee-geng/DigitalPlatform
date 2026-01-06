using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public static class PropertyHelper
    {
        //
        // 摘要:
        //     Determines whether the specified property is a public property on the specified
        //     object.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   ignoreCase:
        //     if set to true, ignore case when searching for the property name.
        //
        // 返回结果:
        //     true if the property is a public property on the specified object; otherwise,
        //     false.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static bool IsPublicProperty(object obj, string property, bool ignoreCase = false)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            Argument.IsNotNullOrWhitespace("property", property);
            PropertyInfo propertyInfo = GetPropertyInfo(obj, property, ignoreCase);
            if ((object)propertyInfo == null)
            {
                return false;
            }

            return propertyInfo.GetGetMethod()?.IsPublic ?? false;
        }

        //
        // 摘要:
        //     Determines whether the specified property is available on the object.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   ignoreCase:
        //     if set to true, ignore case when searching for the property name.
        //
        // 返回结果:
        //     true if the property exists on the object type; otherwise, false.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static bool IsPropertyAvailable(object obj, string property, bool ignoreCase = false)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            Argument.IsNotNullOrWhitespace("property", property);
            return (object)GetPropertyInfo(obj, property, ignoreCase) != null;
        }

        //
        // 摘要:
        //     Tries to get the property value. If it fails, not exceptions will be thrown but
        //     the value is set to a default value and the method will return false.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   value:
        //     The value as output parameter.
        //
        // 返回结果:
        //     true if the method succeeds; otherwise false.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static bool TryGetPropertyValue(object obj, string property, out object value)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            Argument.IsNotNullOrWhitespace("property", property);
            return TryGetPropertyValue<object>(obj, property, out value);
        }

        //
        // 摘要:
        //     Tries to get the property value. If it fails, not exceptions will be thrown but
        //     the value is set to a default value and the method will return false.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   ignoreCase:
        //     if set to true, ignore case when searching for the property name.
        //
        //   value:
        //     The value as output parameter.
        //
        // 返回结果:
        //     true if the method succeeds; otherwise false.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static bool TryGetPropertyValue(object obj, string property, bool ignoreCase, out object value)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            Argument.IsNotNullOrWhitespace("property", property);
            return TryGetPropertyValue<object>(obj, property, ignoreCase, out value);
        }

        //
        // 摘要:
        //     Tries to get the property value. If it fails, not exceptions will be thrown but
        //     the value is set to a default value and the method will return false.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   value:
        //     The value as output parameter.
        //
        // 类型参数:
        //   TValue:
        //     The type of the value.
        //
        // 返回结果:
        //     true if the method succeeds; otherwise false.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static bool TryGetPropertyValue<TValue>(object obj, string property, out TValue value)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            Argument.IsNotNullOrWhitespace("property", property);
            return TryGetPropertyValue(obj, property, ignoreCase: false, out value);
        }

        //
        // 摘要:
        //     Tries to get the property value. If it fails, not exceptions will be thrown but
        //     the value is set to a default value and the method will return false.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   ignoreCase:
        //     if set to true, ignore case when searching for the property name.
        //
        //   value:
        //     The value as output parameter.
        //
        // 类型参数:
        //   TValue:
        //     The type of the value.
        //
        // 返回结果:
        //     true if the method succeeds; otherwise false.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static bool TryGetPropertyValue<TValue>(object obj, string property, bool ignoreCase, out TValue value)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            Argument.IsNotNullOrWhitespace("property", property);
            return TryGetPropertyValue(obj, property, ignoreCase, throwOnException: false, out value);
        }

        //
        // 摘要:
        //     Gets the property value of a specific object.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   ignoreCase:
        //     if set to true, ignore case when searching for the property name.
        //
        // 返回结果:
        //     The property value or null if no property can be found.
        //
        // 异常:
        //   T:Catel.Reflection.PropertyNotFoundException:
        //     The obj is not found or not publicly available.
        //
        //   T:Catel.Reflection.CannotGetPropertyValueException:
        //     The property value cannot be read.
        //
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static object GetPropertyValue(object obj, string property, bool ignoreCase = false)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            return GetPropertyValue<object>(obj, property, ignoreCase);
        }

        //
        // 摘要:
        //     Gets the property value of a specific object.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   ignoreCase:
        //     if set to true, ignore case when searching for the property name.
        //
        // 类型参数:
        //   TValue:
        //     The type of the value.
        //
        // 返回结果:
        //     The property value or null if no property can be found.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        //
        //   T:Catel.Reflection.PropertyNotFoundException:
        //     The obj is not found or not publicly available.
        //
        //   T:Catel.Reflection.CannotGetPropertyValueException:
        //     The property value cannot be read.
        public static TValue GetPropertyValue<TValue>(object obj, string property, bool ignoreCase = false)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            TryGetPropertyValue<TValue>(obj, property, ignoreCase, throwOnException: true, out var value);
            return value;
        }

        private static bool TryGetPropertyValue<TValue>(object obj, string property, bool ignoreCase, bool throwOnException, out TValue value)
        {
            string property2 = property;
            ArgumentNullException.ThrowIfNull(obj, "obj");
            Argument.IsNotNullOrWhitespace("property", property2);
            value = default;
            PropertyInfo propertyInfo = GetPropertyInfo(obj, property2, ignoreCase);
            if ((object)propertyInfo == null)
            {
                if (throwOnException)
                {
                    throw new Exception(string.Format("Property '{0}' is not found on the object '{1}', probably the wrong field is being mapped", property2, obj.GetType().Name));
                }

                return false;
            }

            if (!propertyInfo.CanRead)
            {
                if (throwOnException)
                {
                    throw new Exception(string.Format("Cannot read property {0}.'{1}'", obj.GetType().Name, property2));
                }

                return false;
            }

            try
            {
                value = (TValue)propertyInfo.GetValue(obj, null);
                return true;
            }
            catch (MethodAccessException)
            {
                if (throwOnException)
                {
                    throw new Exception(string.Format("Cannot read property {0}.'{1}'", obj.GetType().Name, property2));
                }

                return false;
            }
        }

        //
        // 摘要:
        //     Tries to set the property value. If it fails, no exceptions will be thrown, but
        //     false will be returned.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   value:
        //     The value.
        //
        //   ignoreCase:
        //     if set to true, ignore case when searching for the property name.
        //
        // 返回结果:
        //     true if the method succeeds; otherwise false.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static bool TrySetPropertyValue(object obj, string property, object? value, bool ignoreCase = false)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            return TrySetPropertyValue(obj, property, value, ignoreCase, throwOnError: false);
        }

        //
        // 摘要:
        //     Sets the property value of a specific object.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   value:
        //     The value.
        //
        //   ignoreCase:
        //     if set to true, ignore case when searching for the property name.
        //
        // 异常:
        //   T:Catel.Reflection.PropertyNotFoundException:
        //     The obj is not found or not publicly available.
        //
        //   T:Catel.Reflection.CannotSetPropertyValueException:
        //     The the property value cannot be written.
        //
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static void SetPropertyValue(object obj, string property, object? value, bool ignoreCase = false)
        {
            ArgumentNullException.ThrowIfNull(obj, "obj");
            TrySetPropertyValue(obj, property, value, ignoreCase, throwOnError: true);
        }

        private static bool TrySetPropertyValue(object obj, string property, object? value, bool ignoreCase, bool throwOnError)
        {
            string property2 = property;
            ArgumentNullException.ThrowIfNull(obj, "obj");
            Argument.IsNotNullOrWhitespace("property", property2);
            PropertyInfo propertyInfo = GetPropertyInfo(obj, property2, ignoreCase);
            if ((object)propertyInfo == null)
            {
                if (throwOnError)
                {
                    throw new Exception(string.Format("Property '{0}' is not found on the object '{1}', probably the wrong field is being mapped", property2, obj.GetType().Name));
                }

                return false;
            }

            if (!propertyInfo.CanWrite)
            {
                if (throwOnError)
                {
                    throw new Exception(string.Format("Cannot write property {0}.'{1}'", obj.GetType().Name, property2));
                }

                return false;
            }

            MethodInfo setMethod = propertyInfo.GetSetMethod(nonPublic: true);
            if ((object)setMethod == null)
            {
                if (throwOnError)
                {
                    throw new Exception(string.Format("Cannot write property {0}.'{1}', SetMethod is null", obj.GetType().Name, property2));
                }

                return false;
            }

            setMethod.Invoke(obj, new object[1] { value });
            return true;
        }

        //
        // 摘要:
        //     Gets hidden property value.
        //
        // 参数:
        //   obj:
        //     The obj.
        //
        //   property:
        //     The property.
        //
        //   baseType:
        //     The base Type.
        //
        // 类型参数:
        //   TValue:
        //     The type of the T value.
        //
        // 返回结果:
        //     ``0.
        //
        // 异常:
        //   T:Catel.Reflection.PropertyNotFoundException:
        //
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentNullException:
        //     The obj is null.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        //
        //   T:System.ArgumentException:
        //     The property is null or whitespace.
        public static TValue GetHiddenPropertyValue<TValue>(object obj, string property, Type baseType)
        {
            string property2 = property;
            ArgumentNullException.ThrowIfNull(obj, "obj");
            Argument.IsNotNullOrWhitespace("property", property2);
            Argument.IsOfType("obj", obj, baseType);
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo propertyEx = baseType.GetProperty(property2, bindingFlags);
            if ((object)propertyEx == null)
            {
                throw new Exception(string.Format("Hidden property '{0}' is not found on the base type '{1}'", property2, baseType.GetType().Name));
            }

            return (TValue)propertyEx.GetValue(obj, bindingFlags, null, Array.Empty<object>(), CultureInfo.InvariantCulture);
        }

        //
        // 摘要:
        //     Gets the property info from the cache.
        //
        // 参数:
        //   obj:
        //     The object.
        //
        //   property:
        //     The property.
        //
        //   ignoreCase:
        //     if set to true, ignore case.
        //
        // 返回结果:
        //     PropertyInfo.
        public static PropertyInfo? GetPropertyInfo(object obj, string property, bool ignoreCase = false)
        {
            object obj2 = obj;
            string property2 = property;
            ArgumentNullException.ThrowIfNull(obj2, "obj");
            ArgumentNullException.ThrowIfNull(property2, "property");
            Type type = obj2.GetType();
            StringComparison comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!ignoreCase)
            {
                return type.GetProperty(property2);
            }

            PropertyInfo[] propertiesEx = type.GetProperties();
            PropertyInfo[] array = propertiesEx;
            foreach (PropertyInfo propertyInfo in array)
            {
                if (string.Equals(propertyInfo.Name, property2, comparisonType))
                {
                    return propertyInfo;
                }
            }

            return null;
        }

        //
        // 摘要:
        //     Gets the name of the property based on the expression.
        //
        // 参数:
        //   propertyExpression:
        //     The property expression.
        //
        //   allowNested:
        //     If set to true, nested properties are allowed.
        //
        // 返回结果:
        //     The string representing the property name.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The propertyExpression is null.
        //
        //   T:System.NotSupportedException:
        //     The specified expression is not a member access expression.
        public static string GetPropertyName(Expression propertyExpression, bool allowNested = false)
        {
            ArgumentNullException.ThrowIfNull(propertyExpression, "propertyExpression");
            return GetPropertyName(propertyExpression, allowNested, nested: false);
        }

        //
        // 摘要:
        //     Gets the name of the property based on the expression.
        //
        // 参数:
        //   propertyExpression:
        //     The property expression.
        //
        //   allowNested:
        //     If set to true, nested properties are allowed.
        //
        // 类型参数:
        //   TValue:
        //     The type of the value.
        //
        // 返回结果:
        //     The string representing the property name.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The propertyExpression is null.
        //
        //   T:System.NotSupportedException:
        //     The specified expression is not a member access expression.
        public static string GetPropertyName<TValue>(Expression<Func<TValue>> propertyExpression, bool allowNested = false)
        {
            ArgumentNullException.ThrowIfNull(propertyExpression, "propertyExpression");
            Expression body = propertyExpression.Body;
            return GetPropertyName(body, allowNested);
        }

        //
        // 摘要:
        //     Gets the name of the property based on the expression.
        //
        // 参数:
        //   propertyExpression:
        //     The property expression.
        //
        //   allowNested:
        //     If set to true, nested properties are allowed.
        //
        // 类型参数:
        //   TModel:
        //     The type of the model.
        //
        //   TValue:
        //     The type of the value.
        //
        // 返回结果:
        //     The string representing the property name.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The propertyExpression is null.
        //
        //   T:System.NotSupportedException:
        //     The specified expression is not a member access expression.
        public static string GetPropertyName<TModel, TValue>(Expression<Func<TModel, TValue>> propertyExpression, bool allowNested = false)
        {
            ArgumentNullException.ThrowIfNull(propertyExpression, "propertyExpression");
            Expression body = propertyExpression.Body;
            return GetPropertyName(body, allowNested);
        }

        //
        // 摘要:
        //     Gets the name of the property based on the expression.
        //
        // 参数:
        //   propertyExpression:
        //     The property expression.
        //
        //   allowNested:
        //     If set to true, nested properties are allowed.
        //
        //   nested:
        //     If set to true, this is a nested call.
        //
        // 返回结果:
        //     The string representing the property name or System.String.Empty if no property
        //     can be found.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The propertyExpression is null.
        //
        //   T:System.NotSupportedException:
        //     The specified expression is not a member access expression.
        private static string GetPropertyName(Expression propertyExpression, bool allowNested = false, bool nested = false)
        {
            Expression propertyExpression2 = propertyExpression;
            ArgumentNullException.ThrowIfNull(propertyExpression2, "propertyExpression");

            MemberExpression memberExpression = !(propertyExpression2 is UnaryExpression unaryExpression) ? propertyExpression2 as MemberExpression : unaryExpression.Operand as MemberExpression;
            if (memberExpression == null)
            {
                if (nested)
                {
                    return string.Empty;
                }

                throw new NotSupportedException(string.Format("The expression is not a member access expression", Array.Empty<object>()));
            }

            if (!(memberExpression.Member is PropertyInfo propertyInfo))
            {
                if (nested)
                {
                    return string.Empty;
                }

                throw new NotSupportedException(string.Format("The expression is not a member access expression", Array.Empty<object>()));
            }

            if (allowNested && memberExpression.Expression != null && memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                string propertyName = GetPropertyName(memberExpression.Expression, allowNested: true, nested: true);
                return propertyName + (!string.IsNullOrEmpty(propertyName) ? "." : string.Empty) + propertyInfo.Name;
            }

            return propertyInfo.Name;
        }
    }
}
