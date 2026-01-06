using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    //
    // 摘要:
    //     Argument validator class to help validating arguments that are passed into a
    //     method.
    //
    //     This class automatically adds thrown exceptions to the log file.
    public static class Argument
    {
        //
        // 摘要:
        //     The parameter info.
        private class ParameterInfo<T>
        {
            //
            // 摘要:
            //     Gets the value.
            public T Value { get; private set; }

            //
            // 摘要:
            //     Gets the name.
            public string Name { get; private set; }

            //
            // 摘要:
            //     Initializes a new instance of the Catel.Argument.ParameterInfo`1 class.
            //
            // 参数:
            //   name:
            //     The parameter name.
            //
            //   value:
            //     The value.
            public ParameterInfo(string name, T value)
            {
                Name = name;
                Value = value;
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //
        //   T:System.ArgumentNullException:
        //     If paramValue is null.
        public static void IsNotNull(string paramName, object paramValue)
        {
            if (paramValue == null)
            {
                string text = "Argument '" + ObjectToStringHelper.ToString(paramName) + "' cannot be null";
                throw new ArgumentNullException(paramName, text);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //
        //   T:System.ArgumentNullException:
        //     If paramValue is null.
        public static void IsNotNull<T>(string paramName, T paramValue)
        {
            if (paramValue == null)
            {
                string text = "Argument '" + ObjectToStringHelper.ToString(paramName) + "' cannot be null";
                throw new ArgumentNullException(paramName, text);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null or empty.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        // 异常:
        //   T:System.ArgumentException:
        //
        //   T:System.ArgumentException:
        //     If paramValue is null or empty.
        public static void IsNotNullOrEmpty(string paramName, string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
            {
                string text = "Argument '" + ObjectToStringHelper.ToString(paramName) + "' cannot be null or empty";
                throw new ArgumentException(text, paramName);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not empty.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If paramValue is null or empty.
        public static void IsNotEmpty(string paramName, Guid paramValue)
        {
            if (paramValue == Guid.Empty)
            {
                string text = "Argument '" + ObjectToStringHelper.ToString(paramName) + "' cannot be Guid.Empty";
                throw new ArgumentException(text, paramName);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null or empty.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        // 异常:
        //   T:System.ArgumentException:
        //
        //   T:System.ArgumentException:
        //     If paramValue is null or empty.
        public static void IsNotNullOrEmpty(string paramName, Guid? paramValue)
        {
            if (!paramValue.HasValue || paramValue.Value == Guid.Empty)
            {
                string text = "Argument '" + ObjectToStringHelper.ToString(paramName) + "' cannot be null or Guid.Empty";
                throw new ArgumentException(text, paramName);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null or a whitespace.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        // 异常:
        //   T:System.ArgumentException:
        //
        //   T:System.ArgumentException:
        //     If paramValue is null or a whitespace.
        public static void IsNotNullOrWhitespace(string paramName, string paramValue)
        {
            if (string.IsNullOrWhiteSpace(paramValue))
            {
                string text = "Argument '" + ObjectToStringHelper.ToString(paramName) + "' cannot be null or whitespace";
                throw new ArgumentException(text, paramName);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null or an empty array (.Length
        //     == 0).
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        // 异常:
        //   T:System.ArgumentException:
        //
        //   T:System.ArgumentException:
        //     If paramValue is null or an empty array.
        public static void IsNotNullOrEmptyArray(string paramName, Array paramValue)
        {
            if (paramValue == null || paramValue.Length == 0)
            {
                string text = "Argument '" + ObjectToStringHelper.ToString(paramName) + "' cannot be null or an empty array";
                throw new ArgumentException(text, paramName);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not out of range.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        //   minimumValue:
        //     The minimum value.
        //
        //   maximumValue:
        //     The maximum value.
        //
        //   validation:
        //     The validation function to call for validation.
        //
        // 类型参数:
        //   T:
        //
        // 异常:
        //   T:System.ArgumentOutOfRangeException:
        //
        //   T:System.ArgumentOutOfRangeException:
        //     If paramValue is out of range.
        //
        //   T:System.ArgumentNullException:
        //     The validation is null.
        public static void IsNotOutOfRange<T>(string paramName, T paramValue, T minimumValue, T maximumValue, Func<T, T, T, bool> validation)
        {
            if (!validation(paramValue, minimumValue, maximumValue))
            {
                string text = $"Argument '{ObjectToStringHelper.ToString(paramName)}' should be between {minimumValue} and {maximumValue}";
                throw new ArgumentOutOfRangeException(paramName, text);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not out of range.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        //   minimumValue:
        //     The minimum value.
        //
        //   maximumValue:
        //     The maximum value.
        //
        // 类型参数:
        //   T:
        //     Type of the argument.
        //
        // 异常:
        //   T:System.ArgumentOutOfRangeException:
        //     If paramValue is out of range.
        public static void IsNotOutOfRange<T>(string paramName, T paramValue, T minimumValue, T maximumValue) where T : IComparable
        {
            IsNotOutOfRange(paramName, paramValue, minimumValue, maximumValue, (T innerParamValue, T innerMinimumValue, T innerMaximumValue) => ((IComparable<T>)(object)innerParamValue).CompareTo(innerMinimumValue) >= 0 && ((IComparable<T>)(object)innerParamValue).CompareTo(innerMaximumValue) <= 0);
        }

        //
        // 摘要:
        //     Determines whether the specified argument has a minimum value.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        //   minimumValue:
        //     The minimum value.
        //
        //   validation:
        //     The validation function to call for validation.
        //
        // 类型参数:
        //   T:
        //     Type of the argument.
        //
        // 异常:
        //   T:System.ArgumentOutOfRangeException:
        //
        //   T:System.ArgumentOutOfRangeException:
        //     If paramValue is out of range.
        //
        //   T:System.ArgumentNullException:
        //     The validation is null.
        public static void IsMinimal<T>(string paramName, T paramValue, T minimumValue, Func<T, T, bool> validation)
        {
            if (!validation(paramValue, minimumValue))
            {
                string text = $"Argument '{ObjectToStringHelper.ToString(paramName)}' should be minimal {minimumValue}";
                throw new ArgumentOutOfRangeException(paramName, text);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument has a minimum value.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        //   minimumValue:
        //     The minimum value.
        //
        // 类型参数:
        //   T:
        //     Type of the argument.
        //
        // 异常:
        //   T:System.ArgumentOutOfRangeException:
        //     If paramValue is out of range.
        public static void IsMinimal<T>(string paramName, T paramValue, T minimumValue) where T : IComparable
        {
            IsMinimal(paramName, paramValue, minimumValue, (T innerParamValue, T innerMinimumValue) => ((IComparable<T>)(object)innerParamValue).CompareTo(innerMinimumValue) >= 0);
        }

        //
        // 摘要:
        //     Determines whether the specified argument has a maximum value.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        //   maximumValue:
        //     The maximum value.
        //
        //   validation:
        //     The validation function to call for validation.
        //
        // 类型参数:
        //   T:
        //     Type of the argument.
        //
        // 异常:
        //   T:System.ArgumentOutOfRangeException:
        //
        //   T:System.ArgumentOutOfRangeException:
        //     If paramValue is out of range.
        //
        //   T:System.ArgumentNullException:
        //     The validation is null.
        public static void IsMaximum<T>(string paramName, T paramValue, T maximumValue, Func<T, T, bool> validation)
        {
            if (!validation(paramValue, maximumValue))
            {
                string text = $"Argument '{ObjectToStringHelper.ToString(paramName)}' should be at maximum {maximumValue}";
                throw new ArgumentOutOfRangeException(paramName, text);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument has a maximum value.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     Value of the parameter.
        //
        //   maximumValue:
        //     The maximum value.
        //
        // 类型参数:
        //   T:
        //     Type of the argument.
        //
        // 异常:
        //   T:System.ArgumentOutOfRangeException:
        //     If paramValue is out of range.
        public static void IsMaximum<T>(string paramName, T paramValue, T maximumValue) where T : IComparable
        {
            IsMaximum(paramName, paramValue, maximumValue, (T innerParamValue, T innerMaximumValue) => ((IComparable<T>)(object)innerParamValue).CompareTo(innerMaximumValue) <= 0);
        }

        //
        // 摘要:
        //     Checks whether the specified type inherits from the baseType.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   type:
        //     The type.
        //
        //   baseType:
        //     The base type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //
        //   T:System.ArgumentException:
        //     The paramName is null.
        //
        //   T:System.ArgumentNullException:
        //     The type is null.
        //
        //   T:System.ArgumentNullException:
        //     The baseType is null.
        public static void InheritsFrom(string paramName, Type type, Type baseType)
        {
            ArgumentNullException.ThrowIfNull(type, "type");
            ArgumentNullException.ThrowIfNull(baseType, "baseType");
            Type baseTypeEx = type.BaseType;
            do
            {
                if (baseTypeEx == baseType)
                {
                    return;
                }

                if (baseTypeEx == typeof(object))
                {
                    break;
                }

                baseTypeEx = type.BaseType;
            }
            while ((object)baseTypeEx != null);
            string text = $"Type '{type.Name}' should have type '{baseType.Name}' as base class, but does not";
            throw new ArgumentException(text, paramName);
        }

        //
        // 摘要:
        //     Checks whether the specified instance inherits from the baseType.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   instance:
        //     The instance.
        //
        //   baseType:
        //     The base type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     The paramName is null.
        //
        //   T:System.ArgumentNullException:
        //     The instance is null.
        public static void InheritsFrom(string paramName, object instance, Type baseType)
        {
            ArgumentNullException.ThrowIfNull(instance, "instance");
            InheritsFrom(paramName, instance.GetType(), baseType);
        }

        //
        // 摘要:
        //     Checks whether the specified instance inherits from the specified TBase.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   instance:
        //     The instance.
        //
        // 类型参数:
        //   TBase:
        //     The base type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     The paramName is null.
        //
        //   T:System.ArgumentNullException:
        //     The instance is null.
        public static void InheritsFrom<TBase>(string paramName, object instance) where TBase : class
        {
            ArgumentNullException.ThrowIfNull(instance, "instance");
            Type typeFromHandle = typeof(TBase);
            InheritsFrom(paramName, instance, typeFromHandle);
        }

        //
        // 摘要:
        //     Checks whether the specified instance implements the specified interfaceType.
        //
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   instance:
        //     The instance to check.
        //
        //   interfaceType:
        //     The type of the interface to check for.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The instance is null.
        //
        //   T:System.ArgumentException:
        //     The instance does not implement the interfaceType.
        public static void ImplementsInterface(string paramName, object instance, Type interfaceType)
        {
            ArgumentNullException.ThrowIfNull(instance, "instance");
            ImplementsInterface(paramName, instance.GetType(), interfaceType);
        }

        //
        // 摘要:
        //     Checks whether the specified instance implements the specified TInterface.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   instance:
        //     The instance to check.
        //
        // 类型参数:
        //   TInterface:
        //     The type of the T interface.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     The paramName is null.
        //
        //   T:System.ArgumentNullException:
        //     The instance is null.
        public static void ImplementsInterface<TInterface>(string paramName, object instance) where TInterface : class
        {
            ArgumentNullException.ThrowIfNull(instance, "instance");
            Type typeFromHandle = typeof(TInterface);
            if (instance is Type type)
            {
                ImplementsInterface(paramName, type, typeFromHandle);
            }
            else
            {
                ImplementsInterface(paramName, instance, typeFromHandle);
            }
        }

        //
        // 摘要:
        //     Checks whether the specified type implements the specified interfaceType.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   type:
        //     The type to check.
        //
        //   interfaceType:
        //     The type of the interface to check for.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     type
        //
        //   T:System.ArgumentException:
        //     The type does not implement the interfaceType.
        //
        //   T:System.ArgumentNullException:
        //     The type is null.
        //
        //   T:System.ArgumentNullException:
        //     The interfaceType is null.
        public static void ImplementsInterface(string paramName, Type type, Type interfaceType)
        {
            Type interfaceType2 = interfaceType;
            ArgumentNullException.ThrowIfNull(type, "type");
            ArgumentNullException.ThrowIfNull(interfaceType2, "interfaceType");
            if (type.GetInterfaces().Any((Type iType) => iType == interfaceType2))
            {
                return;
            }

            string text = $"Type '{type.Name}' should implement interface '{interfaceType2.Name}', but does not";
            throw new ArgumentException(text, paramName);
        }

        //
        // 摘要:
        //     Checks whether the specified instance implements at least one of the specified
        //     interfaceTypes.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   instance:
        //     The instance to check.
        //
        //   interfaceTypes:
        //     The types of the interfaces to check for.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The instance is null.
        //
        //   T:System.ArgumentException:
        //     The interfaceTypes is null or an empty array.
        //
        //   T:System.ArgumentException:
        //     The instance does not implement at least one of the interfaceTypes.
        public static void ImplementsOneOfTheInterfaces(string paramName, object instance, Type[] interfaceTypes)
        {
            ArgumentNullException.ThrowIfNull(instance, "instance");
            ImplementsOneOfTheInterfaces(paramName, instance.GetType(), interfaceTypes);
        }

        //
        // 摘要:
        //     Checks whether the specified type implements at least one of the the specified
        //     interfaceTypes.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   type:
        //     The type to check.
        //
        //   interfaceTypes:
        //     The types of the interfaces to check for.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     type
        //
        //   T:System.ArgumentException:
        //     The interfaceTypes is null or an empty array.
        //
        //   T:System.ArgumentException:
        //     The type does not implement the interfaceTypes.
        //
        //   T:System.ArgumentNullException:
        //     The type is null.
        public static void ImplementsOneOfTheInterfaces(string paramName, Type type, Type[] interfaceTypes)
        {
            ArgumentNullException.ThrowIfNull(type, "type");
            IsNotNullOrEmptyArray("interfaceTypes", interfaceTypes);
            foreach (Type interfaceType in interfaceTypes)
            {
                if (type.GetInterfaces().Any((Type iType) => iType == interfaceType))
                {
                    return;
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Type '{0}' should implement at least one of the following interfaces, but does not:");
            foreach (Type type2 in interfaceTypes)
            {
                stringBuilder.AppendLine("  * " + type2.FullName);
            }

            string text = stringBuilder.ToString();
            throw new ArgumentException(text, paramName);
        }

        //
        // 摘要:
        //     Checks whether the specified instance is of the specified requiredType.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   instance:
        //     The instance to check.
        //
        //   requiredType:
        //     The type to check for.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The instance is null.
        //
        //   T:System.ArgumentNullException:
        //     The instance is null.
        //
        //   T:System.ArgumentException:
        //     The instance is not of type requiredType.
        public static void IsOfType(string paramName, object instance, Type requiredType)
        {
            ArgumentNullException.ThrowIfNull(instance, "instance");
            ArgumentNullException.ThrowIfNull(requiredType, "requiredType");
            IsOfType(paramName, instance.GetType(), requiredType);
        }

        //
        // 摘要:
        //     Checks whether the specified type is of the specified requiredType.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   type:
        //     The type to check.
        //
        //   requiredType:
        //     The type to check for.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     type
        //
        //   T:System.ArgumentException:
        //     The type is not of type requiredType.
        //
        //   T:System.ArgumentNullException:
        //     The type is null.
        //
        //   T:System.ArgumentNullException:
        //     The type is null.
        public static void IsOfType(string paramName, Type type, Type requiredType)
        {
            ArgumentNullException.ThrowIfNull(type, "type");
            ArgumentNullException.ThrowIfNull(requiredType, "requiredType");
            if (type.IsCOMObject || requiredType.IsAssignableFrom(type))
            {
                return;
            }

            string text = $"Type '{type.Name}' should be of type '{requiredType.Name}', but is not";
            throw new ArgumentException(text, paramName);
        }

        //
        // 摘要:
        //     Checks whether the specified instance is of at least one of the specified requiredTypes.
        //
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   instance:
        //     The instance to check.
        //
        //   requiredTypes:
        //     The types to check for.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The instance is null.
        //
        //   T:System.ArgumentException:
        //     The requiredTypes is null or an empty array.
        //
        //   T:System.ArgumentException:
        //     The instance is not at least one of the requiredTypes.
        public static void IsOfOneOfTheTypes(string paramName, object instance, Type[] requiredTypes)
        {
            ArgumentNullException.ThrowIfNull(instance, "instance");
            IsOfOneOfTheTypes(paramName, instance.GetType(), requiredTypes);
        }

        //
        // 摘要:
        //     Checks whether the specified type is of at least one of the specified requiredTypes.
        //
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   type:
        //     The type to check.
        //
        //   requiredTypes:
        //     The types to check for.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     type
        //
        //   T:System.ArgumentException:
        //     The requiredTypes is null or an empty array.
        //
        //   T:System.ArgumentException:
        //     The type is not at least one of the requiredTypes.
        //
        //   T:System.ArgumentNullException:
        //     The type is null.
        public static void IsOfOneOfTheTypes(string paramName, Type type, Type[] requiredTypes)
        {
            ArgumentNullException.ThrowIfNull(type, "type");
            IsNotNullOrEmptyArray("requiredTypes", requiredTypes);
            if (type.IsCOMObject)
            {
                return;
            }

            foreach (Type type2 in requiredTypes)
            {
                if (type2.IsAssignableFrom(type))
                {
                    return;
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Type '{0}' should implement at least one of the following types, but does not:");
            foreach (Type type3 in requiredTypes)
            {
                stringBuilder.AppendLine("  * " + type3.FullName);
            }

            string text = stringBuilder.ToString();
            throw new ArgumentException(text, paramName);
        }

        //
        // 摘要:
        //     Checks whether the specified instance is not of the specified notRequiredType.
        //
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   instance:
        //     The instance to check.
        //
        //   notRequiredType:
        //     The type to check for.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The instance is null.
        //
        //   T:System.ArgumentNullException:
        //     The notRequiredType is null.
        //
        //   T:System.ArgumentException:
        //     The instance is of type notRequiredType.
        public static void IsNotOfType(string paramName, object instance, Type notRequiredType)
        {
            ArgumentNullException.ThrowIfNull(instance, "instance");
            ArgumentNullException.ThrowIfNull(notRequiredType, "notRequiredType");
            IsNotOfType(paramName, instance.GetType(), notRequiredType);
        }

        //
        // 摘要:
        //     Checks whether the specified type is not of the specified notRequiredType.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   type:
        //     The type to check.
        //
        //   notRequiredType:
        //     The type to check for.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     type
        //
        //   T:System.ArgumentException:
        //     The type is of type notRequiredType.
        //
        //   T:System.ArgumentNullException:
        //     The type is null.
        //
        //   T:System.ArgumentNullException:
        //     The notRequiredType is null.
        public static void IsNotOfType(string paramName, Type type, Type notRequiredType)
        {
            ArgumentNullException.ThrowIfNull(type, "type");
            ArgumentNullException.ThrowIfNull(notRequiredType, "notRequiredType");
            if (type.IsCOMObject || !notRequiredType.IsAssignableFrom(type))
            {
                return;
            }

            string text = $"Type '{type.Name}' should not be of type '{notRequiredType.Name}', but is";
            throw new ArgumentException(text, paramName);
        }

        //
        // 摘要:
        //     Checks whether the specified instance is not of any of the specified notRequiredTypes.
        //
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   instance:
        //     The instance to check.
        //
        //   notRequiredTypes:
        //     The types to check for.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The instance is null.
        //
        //   T:System.ArgumentException:
        //     The notRequiredTypes is null or empty array.
        //
        //   T:System.ArgumentException:
        //     The instance is of one of the notRequiredTypes.
        public static void IsNotOfOneOfTheTypes(string paramName, object instance, Type[] notRequiredTypes)
        {
            ArgumentNullException.ThrowIfNull(instance, "instance");
            IsNotOfOneOfTheTypes(paramName, instance.GetType(), notRequiredTypes);
        }

        //
        // 摘要:
        //     Checks whether the specified type is not of any of the specified notRequiredTypes.
        //
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   type:
        //     The type to check.
        //
        //   notRequiredTypes:
        //     The types to check for.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     type
        //
        //   T:System.ArgumentException:
        //     The notRequiredTypes is null or empty array.
        //
        //   T:System.ArgumentException:
        //     The type is of one of the notRequiredTypes.
        //
        //   T:System.ArgumentNullException:
        //     The type is null.
        public static void IsNotOfOneOfTheTypes(string paramName, Type type, Type[] notRequiredTypes)
        {
            ArgumentNullException.ThrowIfNull(type, "type");
            IsNotNullOrEmptyArray("notRequiredTypes", notRequiredTypes);
            if (type.IsCOMObject)
            {
                return;
            }

            foreach (Type type2 in notRequiredTypes)
            {
                if (type2.IsAssignableFrom(type))
                {
                    string text = $"Type '{type.Name}' should not be of type '{type2.Name}', but is";
                    throw new ArgumentException(text, paramName);
                }
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument doesn't match with a given pattern.
        //
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   paramValue:
        //     The para value.
        //
        //   pattern:
        //     The pattern.
        //
        //   regexOptions:
        //     The regular expression options.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     The paramName is null or whitespace.
        //
        //   T:System.ArgumentException:
        //     The paramValue is null or whitespace.
        //
        //   T:System.ArgumentException:
        //     The pattern is null or whitespace.
        public static void IsNotMatch(string paramName, string paramValue, string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            IsNotNullOrWhitespace("paramName", paramName);
            IsNotNullOrWhitespace("paramValue", paramValue);
            IsNotNullOrWhitespace("pattern", pattern);
            if (Regex.IsMatch(paramValue, pattern, regexOptions))
            {
                string text = $"Argument '{paramName}' matches with pattern '{pattern}'";
                throw new ArgumentException(text, paramName);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument match with a given pattern.
        //
        // 参数:
        //   paramName:
        //     Name of the param.
        //
        //   paramValue:
        //     The param value.
        //
        //   pattern:
        //     The pattern.
        //
        //   regexOptions:
        //     The regular expression options.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     The paramName is null or whitespace.
        //
        //   T:System.ArgumentException:
        //     The paramValue is null or whitespace.
        //
        //   T:System.ArgumentException:
        //     The pattern is null or whitespace.
        public static void IsMatch(string paramName, string paramValue, string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            IsNotNullOrWhitespace("paramName", paramName);
            IsNotNullOrWhitespace("paramValue", paramValue);
            IsNotNullOrWhitespace("pattern", pattern);
            if (!Regex.IsMatch(paramValue, pattern, regexOptions))
            {
                string text = $"Argument '{paramName}' doesn't match with pattern '{pattern}'";
                throw new ArgumentException(text, paramName);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument is valid.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     The parameter value.
        //
        //   validation:
        //     The validation function.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the validation code returns false.
        //
        //   T:System.ArgumentNullException:
        //     The paramName is null.
        public static void IsValid<T>(string paramName, T paramValue, Func<bool> validation)
        {
            ArgumentNullException.ThrowIfNull(validation, "validation");
            IsValid(paramName, paramValue, validation());
        }

        //
        // 摘要:
        //     Determines whether the specified argument is valid.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     The parameter value.
        //
        //   validation:
        //     The validation function.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the validation code returns false.
        //
        //   T:System.ArgumentNullException:
        //     The paramName is null.
        //
        //   T:System.ArgumentNullException:
        //     The validation is null.
        public static void IsValid<T>(string paramName, T paramValue, Func<T, bool> validation)
        {
            ArgumentNullException.ThrowIfNull(validation, "validation");
            IsValid(paramName, paramValue, validation(paramValue));
        }

        //
        // 摘要:
        //     Determines whether the specified argument is valid.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     The parameter value.
        //
        //   validator:
        //     The validator.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the Catel.Data.IValueValidator`1.IsValid(`0) of validator returns false.
        //
        //   T:System.ArgumentNullException:
        //     The paramName is null.
        //
        //   T:System.ArgumentNullException:
        //     The validator is null.
        public static void IsValid<T>(string paramName, T paramValue, IValueValidator<T> validator)
        {
            ArgumentNullException.ThrowIfNull(validator, "validator");
            IsValid(paramName, paramValue, validator.IsValid(paramValue));
        }

        //
        // 摘要:
        //     Determines whether the specified argument is valid.
        //
        // 参数:
        //   paramName:
        //     Name of the parameter.
        //
        //   paramValue:
        //     The parameter value.
        //
        //   validation:
        //     The validation function.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the validation code returns false.
        //
        //   T:System.ArgumentNullException:
        //     The paramName is null.
        public static void IsValid<T>(string paramName, T paramValue, bool validation)
        {
            if (!validation)
            {
                string text = "Argument '" + ObjectToStringHelper.ToString(paramName) + "' is not valid";
                throw new ArgumentException(text, paramName);
            }
        }

        //
        // 摘要:
        //     Checks whether the passed in boolean check is true. If not, this method will
        //     throw a System.NotSupportedException.
        //
        // 参数:
        //   isSupported:
        //     if set to true, the action is supported; otherwise false.
        //
        //   errorFormat:
        //     The error format.
        //
        //   args:
        //     The arguments for the string format.
        //
        // 异常:
        //   T:System.NotSupportedException:
        //     The isSupported is false.
        //
        //   T:System.ArgumentException:
        //     The errorFormat is null or whitespace.
        public static void IsSupported(bool isSupported, string errorFormat, params object[] args)
        {
            IsNotNullOrEmpty("errorFormat", errorFormat);
            if (!isSupported)
            {
                string text = string.Format(errorFormat, args);
                throw new NotSupportedException(text);
            }
        }

        //
        // 摘要:
        //     The get parameter info.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        // 类型参数:
        //   T:
        //     The type of the parameter.
        //
        // 返回结果:
        //     The Catel.Argument.ParameterInfo`1.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        private static ParameterInfo<T> GetParameterInfo<T>(Expression<Func<T>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            return new ParameterInfo<T>(memberExpression.Member.Name, expression.Compile()());
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        // 类型参数:
        //   T:
        //     The parameter type.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     If expression value is null.
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void IsNotNull<T>(Expression<Func<T>> expression) where T : class
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsNotNull(parameterInfo.Name, parameterInfo.Value);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null or empty.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If expression value is null or empty.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsNotNullOrEmpty(Expression<Func<string>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<string> parameterInfo = GetParameterInfo(expression);
            IsNotNullOrEmpty(parameterInfo.Name, parameterInfo.Value);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not empty.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If expression value is null or empty.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsNotEmpty(Expression<Func<Guid>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<Guid> parameterInfo = GetParameterInfo(expression);
            IsNotEmpty(parameterInfo.Name, parameterInfo.Value);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null or empty.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If expression value is null or empty.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsNotNullOrEmpty(Expression<Func<Guid?>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<Guid?> parameterInfo = GetParameterInfo(expression);
            IsNotNullOrEmpty(parameterInfo.Name, parameterInfo.Value);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null or a whitespace.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If expression value is null or a whitespace.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsNotNullOrWhitespace(Expression<Func<string>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<string> parameterInfo = GetParameterInfo(expression);
            IsNotNullOrWhitespace(parameterInfo.Name, parameterInfo.Value);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not null or an empty array (.Length
        //     == 0).
        //
        // 参数:
        //   expression:
        //     The expression
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If expression value is null or an empty array.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsNotNullOrEmptyArray(Expression<Func<Array>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<Array> parameterInfo = GetParameterInfo(expression);
            IsNotNullOrEmptyArray(parameterInfo.Name, parameterInfo.Value);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not out of range.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   minimumValue:
        //     The minimum value.
        //
        //   maximumValue:
        //     The maximum value.
        //
        //   validation:
        //     The validation function to call for validation.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The validation is null.
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     If expression value is out of range.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void IsNotOutOfRange<T>(Expression<Func<T>> expression, T minimumValue, T maximumValue, Func<T, T, T, bool> validation)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsNotOutOfRange(parameterInfo.Name, parameterInfo.Value, minimumValue, maximumValue, validation);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is not out of range.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   minimumValue:
        //     The minimum value.
        //
        //   maximumValue:
        //     The maximum value.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentOutOfRangeException:
        //     If expression value is out of range.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsNotOutOfRange<T>(Expression<Func<T>> expression, T minimumValue, T maximumValue) where T : IComparable
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsNotOutOfRange(parameterInfo.Name, parameterInfo.Value, minimumValue, maximumValue);
        }

        //
        // 摘要:
        //     Determines whether the specified argument has a minimum value.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   minimumValue:
        //     The minimum value.
        //
        //   validation:
        //     The validation function to call for validation.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The validation is null.
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     If expression value is out of range.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void IsMinimal<T>(Expression<Func<T>> expression, T minimumValue, Func<T, T, bool> validation)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsMinimal(parameterInfo.Name, parameterInfo.Value, minimumValue, validation);
        }

        //
        // 摘要:
        //     Determines whether the specified argument has a minimum value.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   minimumValue:
        //     The minimum value.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentOutOfRangeException:
        //     If expression value is out of range.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsMinimal<T>(Expression<Func<T>> expression, T minimumValue) where T : IComparable
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsMinimal(parameterInfo.Name, parameterInfo.Value, minimumValue);
        }

        //
        // 摘要:
        //     Determines whether the specified argument has a maximum value.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   maximumValue:
        //     The maximum value.
        //
        //   validation:
        //     The validation function to call for validation.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The validation is null.
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     If expression value is out of range.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void IsMaximum<T>(Expression<Func<T>> expression, T maximumValue, Func<T, T, bool> validation)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsMaximum(parameterInfo.Name, parameterInfo.Value, maximumValue, validation);
        }

        //
        // 摘要:
        //     Determines whether the specified argument has a maximum value.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   maximumValue:
        //     The maximum value.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentOutOfRangeException:
        //     If expression value is out of range.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsMaximum<T>(Expression<Func<T>> expression, T maximumValue) where T : IComparable
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsMaximum(parameterInfo.Name, parameterInfo.Value, maximumValue);
        }

        //
        // 摘要:
        //     Checks whether the specified expression value implements the specified interfaceType.
        //
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   interfaceType:
        //     The type of the interface to check for.
        //
        // 类型参数:
        //   T:
        //     The type of the value.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The expression value is null.
        //
        //   T:System.ArgumentException:
        //     The expression value does not implement the interfaceType.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void ImplementsInterface<T>(Expression<Func<T>> expression, Type interfaceType) where T : class
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            if (parameterInfo.Value is Type type)
            {
                ImplementsInterface(parameterInfo.Name, type, interfaceType);
            }
            else
            {
                ImplementsInterface(parameterInfo.Name, parameterInfo.Value.GetType(), interfaceType);
            }
        }

        //
        // 摘要:
        //     Checks whether the specified expression value implements at least one of the
        //     specified interfaceTypes.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   interfaceTypes:
        //     The types of the interfaces to check for.
        //
        // 类型参数:
        //   T:
        //     The type of the value.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The expression value is null.
        //
        //   T:System.ArgumentException:
        //     The expression value does not implement at least one of the interfaceTypes.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void ImplementsOneOfTheInterfaces<T>(Expression<Func<T>> expression, Type[] interfaceTypes) where T : class
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            if (parameterInfo.Value is Type type)
            {
                ImplementsOneOfTheInterfaces(parameterInfo.Name, type, interfaceTypes);
            }
            else
            {
                ImplementsOneOfTheInterfaces(parameterInfo.Name, parameterInfo.Value.GetType(), interfaceTypes);
            }
        }

        //
        // 摘要:
        //     Checks whether the specified expression value is of the specified requiredType.
        //
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   requiredType:
        //     The type to check for.
        //
        // 类型参数:
        //   T:
        //     The type of the value.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The expression value is null.
        //
        //   T:System.ArgumentException:
        //     The expression value is not of type requiredType.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void IsOfType<T>(Expression<Func<T>> expression, Type requiredType) where T : class
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ArgumentNullException.ThrowIfNull(requiredType, "requiredType");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            if (parameterInfo.Value is Type type)
            {
                IsOfType(parameterInfo.Name, type, requiredType);
            }
            else
            {
                IsOfType(parameterInfo.Name, parameterInfo.Value.GetType(), requiredType);
            }
        }

        //
        // 摘要:
        //     Checks whether the specified expression value is of at least one of the specified
        //     requiredTypes.
        //
        // 参数:
        //   expression:
        //     The expression type.
        //
        //   requiredTypes:
        //     The types to check for.
        //
        // 类型参数:
        //   T:
        //     The type of the value.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The requiredTypes is null.
        //
        //   T:System.ArgumentException:
        //     The expression value is not at least one of the requiredTypes.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void IsOfOneOfTheTypes<T>(Expression<Func<T>> expression, Type[] requiredTypes) where T : class
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            if (parameterInfo.Value is Type type)
            {
                IsOfOneOfTheTypes(parameterInfo.Name, type, requiredTypes);
            }
            else
            {
                IsOfOneOfTheTypes(parameterInfo.Name, parameterInfo.Value.GetType(), requiredTypes);
            }
        }

        //
        // 摘要:
        //     Checks whether the specified expression value is not of the specified notRequiredType.
        //
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   notRequiredType:
        //     The type to check for.
        //
        // 类型参数:
        //   T:
        //     The type of the value.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The expression value is null.
        //
        //   T:System.ArgumentException:
        //     The expression value is of type notRequiredType.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void IsNotOfType<T>(Expression<Func<T>> expression, Type notRequiredType) where T : class
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            if (parameterInfo.Value is Type type)
            {
                IsNotOfType(parameterInfo.Name, type, notRequiredType);
            }
            else
            {
                IsNotOfType(parameterInfo.Name, parameterInfo.Value.GetType(), notRequiredType);
            }
        }

        //
        // 摘要:
        //     Checks whether the specified expression value is not of any of the specified
        //     notRequiredTypes.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   notRequiredTypes:
        //     The types to check for.
        //
        // 类型参数:
        //   T:
        //     The type of the value.
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The expression value is null.
        //
        //   T:System.ArgumentException:
        //     The expression value is of one of the notRequiredTypes.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        public static void IsNotOfOneOfTheTypes<T>(Expression<Func<T>> expression, Type[] notRequiredTypes) where T : class
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            if (parameterInfo.Value is Type type)
            {
                IsNotOfOneOfTheTypes(parameterInfo.Name, type, notRequiredTypes);
            }
            else
            {
                IsNotOfOneOfTheTypes(parameterInfo.Name, parameterInfo.Value.GetType(), notRequiredTypes);
            }
        }

        //
        // 摘要:
        //     Determines whether the specified argument doesn't match with a given pattern.
        //
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   pattern:
        //     The pattern.
        //
        //   regexOptions:
        //     The regular expression options.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     The pattern is null.
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsNotMatch(Expression<Func<string>> expression, string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<string> parameterInfo = GetParameterInfo(expression);
            IsNotMatch(parameterInfo.Name, parameterInfo.Value, pattern, regexOptions);
        }

        //
        // 摘要:
        //     Determines whether the specified argument match with a given pattern.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   pattern:
        //     The pattern.
        //
        //   regexOptions:
        //     The regular expression options.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     The pattern is null.
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsMatch(Expression<Func<string>> expression, string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<string> parameterInfo = GetParameterInfo(expression);
            IsMatch(parameterInfo.Name, parameterInfo.Value, pattern, regexOptions);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is valid.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   validation:
        //     The validation function.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the validation code returns false.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsValid<T>(Expression<Func<T>> expression, Func<T, bool> validation)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsValid(parameterInfo.Name, parameterInfo.Value, validation);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is valid.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   validation:
        //     The validation function.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the validation code returns false.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsValid<T>(Expression<Func<T>> expression, Func<bool> validation)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsValid(parameterInfo.Name, parameterInfo.Value, validation);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is valid.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   validation:
        //     The validation result.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the validation code returns false.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsValid<T>(Expression<Func<T>> expression, bool validation)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsValid(parameterInfo.Name, parameterInfo.Value, validation);
        }

        //
        // 摘要:
        //     Determines whether the specified argument is valid.
        //
        // 参数:
        //   expression:
        //     The expression.
        //
        //   validator:
        //     The validator.
        //
        // 类型参数:
        //   T:
        //     The value type.
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the Catel.Data.IValueValidator`1.IsValid(`0) of validator returns false.
        //
        //   T:System.ArgumentException:
        //     The expression body is not of type System.Linq.Expressions.MemberExpression.
        //
        //
        //   T:System.ArgumentNullException:
        //     The expression is null.
        public static void IsValid<T>(Expression<Func<T>> expression, IValueValidator<T> validator)
        {
            ArgumentNullException.ThrowIfNull(expression, "expression");
            ParameterInfo<T> parameterInfo = GetParameterInfo(expression);
            IsValid(parameterInfo.Name, parameterInfo.Value, validator);
        }
    }
}
