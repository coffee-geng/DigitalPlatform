using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class ByteExpression : NumericExpression<byte>
    {
        public ByteExpression()
            : this(true)
        {
        }

        public ByteExpression(bool isNullable)
        {
            IsDecimal = false;
            IsNullable = isNullable;
            IsSigned = false;
            ValueControlType = ValueControlType.Byte;
        }
    }
}