using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class DecimalExpression : NumericExpression<decimal>
    {
        public DecimalExpression()
            : this(true)
        {
        }

        public DecimalExpression(bool isNullable)
        {
            IsDecimal = true;
            IsNullable = isNullable;
            IsSigned = true;
            ValueControlType = ValueControlType.Decimal;
        }
    }
}