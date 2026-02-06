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
    }
}
