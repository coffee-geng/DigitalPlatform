using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class UnsignedIntegerExpression : NumericExpression<uint>
    {
        public UnsignedIntegerExpression()
            : this(true)
        {
        }

        public UnsignedIntegerExpression(bool isNullable)
        {
            IsDecimal = false;
            IsNullable = isNullable;
            IsSigned = false;
            ValueControlType = ValueControlType.UnsignedInteger;
        }
    }
}