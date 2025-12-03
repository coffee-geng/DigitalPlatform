using Coffee.DigitalPlatform.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class Condition
    {
        public Condition(Variable source, object targetValue, ConditionOperator @operator)
        {
            Source = source;
            TargetValue = targetValue;
            Operator = @operator;
        }

        public string ConditionNum {  get; private set; }

        public Variable Source { get; private set; }

        public ConditionOperator Operator { get; private set; }

        public object TargetValue {  get; private set; }
    }
}
