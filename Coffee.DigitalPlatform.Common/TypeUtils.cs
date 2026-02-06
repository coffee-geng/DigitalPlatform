using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Common
{
    public class TypeUtils
    {
        public static Type GetTypeFromAssemblyQualifiedName(string assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName))
            {
                throw new ArgumentException("Assembly qualified name cannot be null or empty.");
            }
            try
            {
                // 方法1：直接使用 Type.GetType()
                Type type = Type.GetType(assemblyQualifiedName, false, true);

                if (type != null)
                {
                    return type;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading type: {ex.Message}");
            }

            // 方法2：如果上面失败，尝试解析程序集
            int commaIndex = assemblyQualifiedName.IndexOf(',');
            if (commaIndex > 0)
            {
                string typeName = assemblyQualifiedName.Substring(0, commaIndex).Trim();
                string assemblyNamePart = assemblyQualifiedName.Substring(commaIndex + 1).Trim();

                // 提取程序集名称（可能包含版本等信息）
                string simpleAssemblyName = assemblyNamePart.Split(',')[0].Trim();

                try
                {
                    string dllFile = System.IO.Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, $"{simpleAssemblyName}.dll");
                    Assembly assembly = Assembly.LoadFrom(dllFile);
                    return assembly.GetType(typeName, true, true);
                }
                catch
                {
                    //如果简单名称加载失败，尝试完整名称
                    Assembly assembly = Assembly.Load(assemblyNamePart);
                    return assembly.GetType(typeName, true, true);
                }
            }

            throw new TypeLoadException($"Unable to load type from: {assemblyQualifiedName}");
        }

        public static bool EqualCollection<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1 == null && list2 == null)
                return true;
            if (list1 == null || list2 == null)
                return false;
            if (ReferenceEquals(list1, list2))
                return true;
            if (list1.Count() != list2.Count())
                return false;
            foreach (T item in list1)
            {
                if (!list2.Contains(item))
                    return false;
            }
            return true;
        }

        public static Type CreateGenericTypeBaseOnNonGeneric(Type nonGenericType, Type[] typeArguments)
        {
            // 前提条件是对应的泛型和非泛型类是在同一个程序集
            Assembly assembly = nonGenericType.Assembly;

            // 查找参数指定类对应的泛型类型定义
            var genericTypeDefinitionArray = assembly.GetTypes().Where(t => t.IsGenericTypeDefinition);
            IList<Type> myGenericTypeDefinitions = new List<Type>();
            foreach (var genericType in genericTypeDefinitionArray)
            {
                string typeName = nonGenericType.GetType().Name;
                if (genericType.Name == typeName)
                {
                    myGenericTypeDefinitions.Add(genericType);
                }
                else if (genericType.Name.StartsWith(typeName))
                {
                    var perfix = genericType.Name.Substring(typeName.Length);
                    if (perfix.Length > 1 && perfix.First() == '`')
                    {
                        if (int.TryParse(perfix.Substring(1), out int paramCount) && paramCount > 0)
                        {
                            //符合Student的泛型类Student`1，其数字1是泛型参数的个数
                            myGenericTypeDefinitions.Add(genericType);
                        }
                    }
                }
            }
            var myGenericType = myGenericTypeDefinitions.FirstOrDefault();
            if (myGenericType == null)
            {
                throw new TypeLoadException($"找不到泛型类型定义: {nonGenericType.FullName}");
            }

            // 创建构造泛型类型
            return myGenericType.MakeGenericType(typeArguments);
        }
    }

    public static class ObjectToStringConverter
    {
        // 配置Json序列化选项
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() } // 枚举转换为字符串名称
        };

        /// <summary>
        /// 将对象转换为字符串表示形式
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>对象的字符串表示</returns>
        public static string ConvertToString(object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            Type type = obj.GetType();

            // 检查是否为基本类型或系统提供的类型
            if (IsBuiltInType(type))
            {
                return ConvertBuiltInTypeToString(obj, type);
            }

            // 检查是否为枚举类型
            if (type.IsEnum)
            {
                return obj.ToString(); // 枚举值名称
            }

            // 检查是否为可空类型且包含值
            if (IsNullableType(type, out Type underlyingType))
            {
                if (underlyingType.IsEnum)
                {
                    return obj.ToString(); // 可空枚举的值名称
                }
                else if (IsBuiltInType(underlyingType))
                {
                    return ConvertBuiltInTypeToString(obj, underlyingType);
                }
            }

            // 自定义类型 - 转换为JSON
            try
            {
                return JsonSerializer.Serialize(obj, _jsonOptions);
            }
            catch (JsonException)
            {
                // 如果JSON序列化失败，回退到ToString()
                return obj.ToString();
            }
        }

        /// <summary>
        /// 字符串转换为对象
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的对象</returns>
        public static object ConvertFromString(string str, Type targetType)
        {
            if (str == null)
            {
                return GetDefaultValue(targetType);
            }

            // 处理空字符串的特殊情况
            if (str == "null" || string.IsNullOrEmpty(str))
            {
                // 对于可空类型，返回null
                if (IsNullableType(targetType, out _) || !targetType.IsValueType)
                {
                    return null;
                }
                // 对于值类型，返回默认值
                return GetDefaultValue(targetType);
            }

            try
            {
                // 检查是否为内置类型
                if (IsBuiltInType(targetType))
                {
                    return ConvertStringToBuiltInType(str, targetType);
                }

                // 检查是否为枚举类型
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, str);
                }

                // 检查是否为可空类型
                if (IsNullableType(targetType, out Type underlyingType))
                {
                    if (underlyingType.IsEnum)
                    {
                        return Enum.Parse(underlyingType, str);
                    }
                    else if (IsBuiltInType(underlyingType))
                    {
                        return ConvertStringToBuiltInType(str, underlyingType);
                    }
                }

                // 自定义类型 - 从JSON反序列化
                try
                {
                    return JsonSerializer.Deserialize(str, targetType, _jsonOptions);
                }
                catch (JsonException jsonEx)
                {
                    // 如果JSON反序列化失败，尝试使用TypeConverter
                    return ConvertUsingTypeConverter(str, targetType, jsonEx);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"无法将字符串 '{str}' 转换为类型 {targetType.Name}", ex);
            }
        }

        /// <summary>
        /// 泛型版本的字符串转换为对象
        /// </summary>
        public static T ConvertFromString<T>(string str)
        {
            return (T)ConvertFromString(str, typeof(T));
        }

        /// <summary>
        /// 扩展方法版本
        /// </summary>
        public static string ToStringExt(this object obj)
        {
            return ConvertToString(obj);
        }

        /// <summary>
        /// 扩展方法：字符串转对象
        /// </summary>
        public static T ToObjectExt<T>(this string str)
        {
            return ConvertFromString<T>(str);
        }

        /// <summary>
        /// 判断是否为内置类型
        /// </summary>
        private static bool IsBuiltInType(Type type)
        {
            // 基本值类型
            if (type == typeof(bool) ||
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(char) ||
                type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(float) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(short) ||
                type == typeof(ushort))
            {
                return true;
            }

            // 字符串
            if (type == typeof(string))
            {
                return true;
            }

            // 日期时间相关类型
            if (type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(DateOnly) ||
                type == typeof(TimeOnly))
            {
                return true;
            }

            // Guid
            if (type == typeof(Guid))
            {
                return true;
            }

            // 大整数
            if (type == typeof(System.Numerics.BigInteger))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 转换内置类型为字符串
        /// </summary>
        private static string ConvertBuiltInTypeToString(object obj, Type type)
        {
            if (type == typeof(string))
            {
                return (string)obj;
            }

            if (type == typeof(DateTime))
            {
                return ((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss.fff");
            }

            if (type == typeof(DateTimeOffset))
            {
                return ((DateTimeOffset)obj).ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
            }

            if (type == typeof(TimeSpan))
            {
                return ((TimeSpan)obj).ToString("c");
            }

            if (type == typeof(DateOnly))
            {
                return ((DateOnly)obj).ToString("yyyy-MM-dd");
            }

            if (type == typeof(TimeOnly))
            {
                return ((TimeOnly)obj).ToString("HH:mm:ss.fff");
            }

            if (type == typeof(Guid))
            {
                return ((Guid)obj).ToString();
            }

            // 其他内置类型使用默认的ToString()
            return obj.ToString();
        }

        /// <summary>
        /// 判断是否为可空类型
        /// </summary>
        private static bool IsNullableType(Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }

        /// <summary>
        /// 将字符串转换为内置类型
        /// </summary>
        private static object ConvertStringToBuiltInType(string str, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return str;
            }

            if (targetType == typeof(bool))
            {
                if (bool.TryParse(str, out bool result))
                    return result;

                // 支持0/1、yes/no等常见格式
                str = str.ToLower();
                if (str == "1" || str == "yes" || str == "y" || str == "true")
                    return true;
                if (str == "0" || str == "no" || str == "n" || str == "false")
                    return false;

                throw new FormatException($"无法将 '{str}' 转换为布尔值");
            }

            if (targetType == typeof(byte))
            {
                if (byte.TryParse(str, out byte result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为字节");
            }

            if (targetType == typeof(sbyte))
            {
                if (sbyte.TryParse(str, out sbyte result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为有符号字节");
            }

            if (targetType == typeof(char))
            {
                if (char.TryParse(str, out char result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为字符");
            }

            if (targetType == typeof(decimal))
            {
                if (decimal.TryParse(str, out decimal result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为Decimal");
            }

            if (targetType == typeof(double))
            {
                if (double.TryParse(str, out double result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为Double");
            }

            if (targetType == typeof(float))
            {
                if (float.TryParse(str, out float result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为Float");
            }

            if (targetType == typeof(int))
            {
                if (int.TryParse(str, out int result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为整数");
            }

            if (targetType == typeof(uint))
            {
                if (uint.TryParse(str, out uint result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为无符号整数");
            }

            if (targetType == typeof(long))
            {
                if (long.TryParse(str, out long result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为长整数");
            }

            if (targetType == typeof(ulong))
            {
                if (ulong.TryParse(str, out ulong result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为无符号长整数");
            }

            if (targetType == typeof(short))
            {
                if (short.TryParse(str, out short result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为短整数");
            }

            if (targetType == typeof(ushort))
            {
                if (ushort.TryParse(str, out ushort result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为无符号短整数");
            }

            if (targetType == typeof(DateTime))
            {
                // 支持多种日期格式
                string[] formats = {
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "yyyy/MM/dd HH:mm:ss",
                "yyyy/MM/dd",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy"
            };

                if (DateTime.TryParseExact(str, formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime result))
                    return result;

                if (DateTime.TryParse(str, out result))
                    return result;

                throw new FormatException($"无法将 '{str}' 转换为DateTime");
            }

            if (targetType == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(str, out DateTimeOffset result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为DateTimeOffset");
            }

            if (targetType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(str, out TimeSpan result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为TimeSpan");
            }

            if (targetType == typeof(DateOnly))
            {
                if (DateOnly.TryParse(str, out DateOnly result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为DateOnly");
            }

            if (targetType == typeof(TimeOnly))
            {
                if (TimeOnly.TryParse(str, out TimeOnly result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为TimeOnly");
            }

            if (targetType == typeof(Guid))
            {
                if (Guid.TryParse(str, out Guid result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为Guid");
            }

            if (targetType == typeof(System.Numerics.BigInteger))
            {
                if (System.Numerics.BigInteger.TryParse(str, out System.Numerics.BigInteger result))
                    return result;
                throw new FormatException($"无法将 '{str}' 转换为BigInteger");
            }

            // 其他类型使用ChangeType
            try
            {
                return Convert.ChangeType(str, targetType);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"不支持的类型转换: {targetType.Name}", ex);
            }
        }

        /// <summary>
        /// 使用TypeConverter进行转换
        /// </summary>
        private static object ConvertUsingTypeConverter(string str, Type targetType, Exception originalException)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(targetType);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    return converter.ConvertFromString(str);
                }
                catch
                {
                    // 如果TypeConverter也失败，抛出原始异常
                    throw originalException;
                }
            }

            // 尝试使用Constructor
            if (targetType.GetConstructor(new[] { typeof(string) }) != null)
            {
                try
                {
                    return Activator.CreateInstance(targetType, str);
                }
                catch
                {
                    throw originalException;
                }
            }

            throw originalException;
        }

        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        private static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
