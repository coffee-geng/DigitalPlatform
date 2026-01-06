using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class ShortExpression : NumericExpression<short>
    {
        public ShortExpression()
            : this(true)
        {
        }

        public ShortExpression(bool isNullable)
        {
            IsDecimal = false;
            IsNullable = isNullable;
            IsSigned = true;
            ValueControlType = ValueControlType.Short;
        }
    }
}