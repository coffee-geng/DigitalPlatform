using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class ConditionEntity
    {
        public string CNum { get; set; }
        public string Operator { get; set; }
        public VariableEntity Operand1 { get; set; }
        public object Operand2 { get; set; }
    }
}
