using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class LongExpression : NumericExpression<long>
    {
        public LongExpression()
            : this(true)
        {
        }

        public LongExpression(bool isNullable)
        {
            IsDecimal = false;
            IsNullable = isNullable;
            IsSigned = true;
            ValueControlType = ValueControlType.Long;
        }
    }
}