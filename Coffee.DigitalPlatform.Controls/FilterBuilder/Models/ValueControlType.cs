using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls
{
    public enum ValueControlType
    {
        [ValueType(typeof(string))]
        Text,
        [ValueType(typeof(DateTime))]
        DateTime,
        [ValueType(typeof(bool))]
        Boolean,
        [ValueType(typeof(TimeSpan))]
        TimeSpan,
        [ValueType(typeof(decimal))]
        Decimal,
        [ValueType(typeof(double))]
        Double,
        [ValueType(typeof(int))]
        Integer,
        [ValueType(typeof(byte))]
        [ValueType(typeof(short))]
        [ValueType(typeof(ushort))]
        [ValueType(typeof(int))]
        [ValueType(typeof(uint))]
        [ValueType(typeof(long))]
        [ValueType(typeof(ulong))]
        [ValueType(typeof(float))]
        [ValueType(typeof(double))]
        [ValueType(typeof(decimal))]
        Numeric,
        [ValueType(typeof(uint))]
        UnsignedInteger,
        [ValueType(typeof(byte))]
        Byte,
        [ValueType(typeof(byte[]))]
        SByte,
        [ValueType(typeof(short))]
        Short,
        [ValueType(typeof(ushort))]
        UnsignedShort,
        [ValueType(typeof(long))]
        Long,
        [ValueType(typeof(ulong))]
        UnsignedLong,
        [ValueType(typeof(float))]
        Float,
        [ValueType(typeof(Enum))]
        Enum
    }

    public enum TimeSpanType
    {
        Years,
        Months,
        Days,
        Hours,
        Minutes,
        Seconds
    }

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class ValueTypeAttribute : Attribute
    {
        public Type VarType { get; }
        public ValueTypeAttribute(Type varType)
        {
            VarType = varType;
        }
    }
}
