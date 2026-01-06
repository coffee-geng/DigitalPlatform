using System;
using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class NumericExpression : NumericExpression<double>
    {
        public NumericExpression()
        {
            IsDecimal = true;
            IsSigned = true;
            ValueControlType = ValueControlType.Numeric;
        }

        public NumericExpression(Type type)
            : this()
        {
            IsNullable = type.IsNullable();
        }
    }
}