using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class VariableEntity
    {
        public string VarNum { get; set; }
        public string Label { get; set; }
        public string Address { get; set; }
        public double Offset { get; set; }
        public double Factor { get; set; }
        public string VarType { get; set; }

        public List<ConditionEntity> AlarmConditions { get; set; }
        public List<ConditionEntity> UnionConditions { get; set; }
    }
}
