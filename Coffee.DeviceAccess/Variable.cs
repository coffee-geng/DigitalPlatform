using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAccess
{
    public abstract class VariableBase
    {
        // 变量编码
        public string VarNum { get; set; }

        // 变量地址
        public string VarAddress { get; set; }
    }

    /// <summary>
    /// 仅读写一个点位值。
    /// 这个点位值的数据类型由VarType指定。
    /// </summary>
    public class Variable : VariableBase
    {
        // 变量类型
        public Type VarType { get; set; }

        // 变量值
        public object VarValue { get; set; }
    }

    /// <summary>
    /// 泛型版本，仅读写一个点位值。
    /// 这个点位值的数据类型由泛型指定。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Variable<T> : VariableBase
    {
        public T VarValue { get; set; }
    }

    /// <summary>
    /// 变量块，可读写连续区域的多个点位值。
    /// 注意：这个点位值的数据类型由VarType指定，并且点位置的数据类型必须相同。
    /// </summary>
    public class VariableBlock : VariableBase
    {
        // 变量类型
        public Type VarType { get; set; }

        // 读写的变量个数
        public int Count { get; private set; }

        // 变量值
        private object[] _varValues;
        public object[] VarValues
        {
            get { return _varValues; }
            set
            {
                _varValues = value;
                Count = value != null ? value.Length : 0;
            }
        }
    }

    /// <summary>
    /// 变量块，可读写连续区域的多个点位值。
    /// 注意：这个点位值的数据类型由泛型指定，并且点位置的数据类型必须相同。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VariableBlock<T> : VariableBase
    {
        // 读写的变量个数
        public int Count { get; private set; }

        private T[] _varValues;
        public T[] VarValues
        {
            get { return _varValues; }
            set
            {
                _varValues = value;
                Count = value != null ? value.Length : 0;
            }
        }
    }
}
