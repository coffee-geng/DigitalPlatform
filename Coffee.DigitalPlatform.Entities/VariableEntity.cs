using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class VariableEntity
    {
        [Column(name: "var_num")]
        public string VarNum { get; set; }

        [Column(name: "var_name")]
        public string Label { get; set; }

        [Column(name: "var_address")]
        public string Address { get; set; }

        [Column(name: "offset")]
        public double Offset { get; set; }

        [Column(name: "modulus")]
        public double Factor { get; set; }

        [Column(name: "var_type")]
        public string VarType { get; set; }

        [NotMapped]
        public List<ConditionEntity> AlarmConditions { get; set; }

        [NotMapped]
        public List<ConditionEntity> UnionConditions { get; set; }
    }
}
