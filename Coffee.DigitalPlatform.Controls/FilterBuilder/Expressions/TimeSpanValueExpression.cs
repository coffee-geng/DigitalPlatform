using System;
using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class TimeSpanValueExpression : ValueDataTypeExpression<TimeSpan>
    {
        public TimeSpanValueExpression()
            : this(true)
        {
        }

        public TimeSpanValueExpression(bool isNullable)
        {
            IsNullable = isNullable;
            Value = TimeSpan.Zero;
            ValueControlType = ValueControlType.TimeSpan;
        }
    }
}