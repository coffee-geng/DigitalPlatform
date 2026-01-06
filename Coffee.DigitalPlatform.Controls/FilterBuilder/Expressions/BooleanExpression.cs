using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class BooleanExpression : DataTypeExpression
    {
        public BooleanExpression()
        {
            BooleanValues = new List<bool> { true, false };
            Value = true;
            SelectedCondition = Condition.EqualTo;
            ValueControlType = ValueControlType.Boolean;
        }

        public bool Value { get; set; }

        public List<bool> BooleanValues { get; set; }

        public override bool CalculateResult(IPropertyMetadata propertyMetadata, object entity)
        {
            var entityValue = propertyMetadata.GetValue<bool>(entity);

            return SelectedCondition switch
            {
                Condition.EqualTo => entityValue == Value,
                Condition.NotEqualTo => entityValue != Value,
                _ => throw new NotSupportedException(string.Format(LanguageHelper.GetString("FilterBuilder_Exception_Message_ConditionIsNotSupported_Pattern"), SelectedCondition))
            };
        }

        public override string ToString()
        {
            return $"{SelectedCondition.Humanize()} '{Value}'";
        }
    }
}