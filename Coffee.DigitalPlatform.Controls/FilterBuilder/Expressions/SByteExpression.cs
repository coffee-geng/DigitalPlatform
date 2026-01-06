using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class SByteExpression : NumericExpression<sbyte>
    {
        public SByteExpression()
            : this(true)
        {
        }

        public SByteExpression(bool isNullable)
        {
            IsDecimal = false;
            IsNullable = isNullable;
            IsSigned = true;
            ValueControlType = ValueControlType.SByte;
        }
    }
}