using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class UnsignedLongExpression : NumericExpression<ulong>
    {
        public UnsignedLongExpression()
            : this(true)
        {
        }

        public UnsignedLongExpression(bool isNullable)
        {
            IsDecimal = false;
            IsNullable = isNullable;
            IsSigned = false;
            ValueControlType = ValueControlType.UnsignedLong;
        }
    }
}