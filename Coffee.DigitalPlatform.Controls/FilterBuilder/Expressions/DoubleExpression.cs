using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class DoubleExpression : NumericExpression<double>
    {
        public DoubleExpression()
            : this(true)
        {
        }

        public DoubleExpression(bool isNullable)
        {
            IsDecimal = true;
            IsNullable = isNullable;
            IsSigned = true;
            ValueControlType = ValueControlType.Double;
        }
    }
}