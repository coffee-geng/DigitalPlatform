using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class UnsignedShortExpression : NumericExpression<ushort>
    {
        public UnsignedShortExpression()
            : this(true)
        {
        }

        public UnsignedShortExpression(bool isNullable)
        {
            IsDecimal = false;
            IsNullable = isNullable;
            IsSigned = false;
            ValueControlType = ValueControlType.UnsignedShort;
        }
    }
}