using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAdapter
{
    public class DataTypeHelper
    {
        public static Type GetTypeFromDataType(DataType dataType)
        {
            Type type = typeof(byte[]);
            switch (dataType)
            {
                case DataType.Bit:
                    type = typeof(bool);
                    break;
                case DataType.Byte:
                    type = typeof(byte);
                    break;
                case DataType.Int16:
                    type = typeof(short);
                    break;
                case DataType.UInt16:
                    type = typeof(ushort);
                    break;
                case DataType.Int32:
                    type = typeof(int);
                    break;
                case DataType.UInt32:
                    type = typeof(uint);
                    break;
                case DataType.Float:
                    type = typeof(float);
                    break;
                case DataType.Double:
                    type = typeof(double);
                    break;
                case DataType.String:
                    type = typeof(string);
                    break;
                case DataType.ByteArray:
                    type = typeof(byte[]);
                    break;
            }
            return type;
        }

        public static DataType? GetDataTypeFromType(Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (type == typeof(bool))
            {
                return DataType.Bit;
            }
            else if (type == typeof(byte))
            {
                return DataType.Byte;
            }
            else if (type == typeof(short))
            {
                return DataType.Int16;
            }
            else if (type == typeof(ushort))
            {
                return DataType.UInt16;
            }
            else if (type == typeof(int))
            {
                return DataType.Int32;
            }
            else if (type == typeof(uint))
            {
                return DataType.UInt32;
            }
            else if (type == typeof(float))
            {
                return DataType.Float;
            }
            else if (type == typeof(double))
            {
                return DataType.Double;
            }
            else if (type == typeof(string))
            {
                return DataType.String;
            }
            else if (type == typeof(byte[]))
            {
                return DataType.ByteArray;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 将数据对象转换为泛型指定类型的数组。
        /// </summary>
        /// <typeparam name="T">泛型指定的类型</typeparam>
        /// <param name="value">原数据对象</param>
        /// <returns>返回泛型指定类型的数组</returns>
        /// <exception cref="Exception">数据类型与泛型不匹配</exception>
        public static T[] ConvertToDataArray<T>(object value)
        {
            if (value == null)
                return Enumerable.Empty<T>().ToArray();
            if (value is IEnumerable<T>)
            {
                return new List<T>(value as IEnumerable<T>).ToArray();
            }
            else if (TryCast<T>(value, out T val))
            {
                return new T[] { val };
            }
            else
            {
                throw new Exception($"数据类型与{typeof(T)}不匹配，无法转换！");
            }
        }

        /// <summary>
        /// 将数据对象转换为泛型指定类型的数组，并且计算出数组的长度。
        /// </summary>
        /// <typeparam name="T">泛型指定的类型</typeparam>
        /// <param name="value">原数据对象</param>
        /// <returns>返回转换后的数组长度</returns>
        /// <exception cref="Exception">数据类型与泛型不匹配</exception>
        public static int GetLengthOfDataArray<T>(object value)
        {
            if (value == null)
                return 0;
            if (value is IEnumerable<T>)
            {
                return new List<T>(value as IEnumerable<T>).Count();
            }
            else if (TryCast<T>(value, out T val))
            {
                return 1;
            }
            else
            {
                throw new Exception($"数据类型与{typeof(T)}不匹配，无法转换！");
            }
        }

        /// <summary>
        /// 尝试将一个对象转换为指定类型，如果转换成功则返回true，并将转换结果赋值给result参数；如果转换失败则返回false，并将result参数设置为默认值。
        /// </summary>
        /// <typeparam name="T">泛型指定转换的类型</typeparam>
        /// <param name="obj">转换对象</param>
        /// <param name="result">转换结果</param>
        /// <returns>是否成功</returns>
        public static bool TryCast<T>(object obj, out T result)
        {
            result = default;

            if (obj is T t)
            {
                result = t;
                return true;
            }

            try
            {
                result = (T)Convert.ChangeType(obj, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试将一个对象转换为指定类型，如果转换成功则返回true，并将转换结果赋值给result参数；如果转换失败则返回false，并将result参数设置为null。
        /// </summary>
        /// <typeparam name="T">泛型指定转换的类型</typeparam>
        /// <param name="obj">转换对象（单一或集合对象）</param>
        /// <param name="result">转换结果，不管转换对象是单一还是集合对象，转换结果都是数组</param>
        /// <returns>是否成功</returns>
        public static bool TryCast<T>(object obj, out T[] result)
        {
            result = null;
            if (obj == null)
            {
                return false;
            }
            if (obj is IEnumerable<T> enumerable)
            {
                result = new List<T>(enumerable).ToArray();
                return true;
            }
            try
            {
                if (IsGenericEnumerable(obj.GetType(), out Type? enumerableType))
                {
                    IList<T> collection = new List<T>();
                    foreach (var item in (IEnumerable<object>)obj)
                    {
                        var item2 = Convert.ChangeType(item, typeof(T));
                        collection.Add((T)item2);
                    }
                    result = collection.ToArray();
                    return true;
                }
                else
                {
                    try
                    {
                        result = new T[] { (T)Convert.ChangeType(obj, typeof(T)) };
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 判断对象是否实现了泛型 IEnumerable<> 接口，如果实现了则返回true，并将元素类型赋值给elementType参数；如果没有实现则返回false，并将elementType参数设置为null。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="elementType">泛型集合的元素类型</param>
        /// <returns>是否是泛型集合</returns>
        public static bool TryGetIEnumerableElementType(object obj, out Type elementType)
        {
            elementType = null;

            if (obj == null) return false;

            Type type = obj.GetType();

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    elementType = interfaceType.GetGenericArguments()[0];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断类型是否是泛型集合
        /// <param name="type">要判断的类型</param>
        /// <param name="enumerableType">如果type是泛型集合，则返回type实现的泛型集合接口类型（如IEnumerable<>、ICollection<>、IReadOnlyCollection<>、IList<>、ISet<>或IDictionary<,>）；如果type不是泛型集合，则返回null</param>
        /// <return>是否是泛型集合</return>
        /// </summary>
        public static bool IsGenericEnumerable(Type type, out Type? enumerableType)
        {
            if (type == null)
            {
                enumerableType = null;
                return false;
            }

            // 检查是否实现了泛型 IEnumerable<> 接口
            var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                (i.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                                 i.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                                 i.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>) ||
                                 i.GetGenericTypeDefinition() == typeof(IList<>) ||
                                 i.GetGenericTypeDefinition() == typeof(ISet<>) ||
                                 i.GetGenericTypeDefinition() == typeof(IDictionary<,>))).ToList();
            if (interfaces.Any())
            {
                if (interfaces.Any(itm => itm == typeof(IDictionary<,>)))
                    enumerableType = typeof(IDictionary<,>);
                else if (interfaces.Any(itm => itm == typeof(ISet<>)))
                    enumerableType = typeof(ISet<>);
                else if (interfaces.Any(itm => itm == typeof(IList<>)))
                    enumerableType = typeof(IList<>);
                else if (interfaces.Any(itm => itm == typeof(IReadOnlyCollection<>)))
                    enumerableType = typeof(IReadOnlyCollection<>);
                else if (interfaces.Any(itm => itm == typeof(ICollection<>)))
                    enumerableType = typeof(ICollection<>);
                else
                    enumerableType = typeof(IEnumerable<>);
                return true;
            }
            else
            {
                enumerableType = null;
                return false;
            }
        }
    }
}
