using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class FloatExpression : NumericExpression<float>
    {
        public FloatExpression()
            : this(true)
        {
        }

        public FloatExpression(bool isNullable)
        {
            IsDecimal = true;
            IsNullable = isNullable;
            IsSigned = true;
            ValueControlType = ValueControlType.Float;
        }
    }
}