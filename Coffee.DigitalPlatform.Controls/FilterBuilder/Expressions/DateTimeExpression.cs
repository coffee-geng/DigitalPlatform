using System;
using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class DateTimeExpression : ValueDataTypeExpression<DateTime>
    {
        public DateTimeExpression()
            : this(true)
        {
        }

        public DateTimeExpression(bool isNullable)
        {
            IsNullable = isNullable;
            Value = DateTime.Now;
            ValueControlType = ValueControlType.DateTime;
        }
    }
}