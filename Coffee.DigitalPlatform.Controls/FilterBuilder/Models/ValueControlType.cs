using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public enum ValueControlType
    {
        Text,
        DateTime,
        Boolean,
        TimeSpan,
        Decimal,
        Double,
        Integer,
        Numeric,
        UnsignedInteger,
        Byte,
        SByte,
        Short,
        UnsignedShort,
        Long,
        UnsignedLong,
        Float,
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
}
