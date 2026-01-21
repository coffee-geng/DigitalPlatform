using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class ConditionEntity
    {
        [Column(name: "id")]
        public int Id {  get; set; }

        [Column(name: "v_num")]
        public string VarNum {  get; set; }

        [Column(name: "operator")]
        public string Operator { get; set; }

        [Column(name: "value")]
        public object Value { get; set; }

        [Column(name: "c_num")]
        public string CNum { get; set; }

        //条件表达式或条件组
        [Column(name: "c_type")]
        public ConditionNodeTypes ConditionNodeTypes { get; set; }

        //当前表单式或条件组在当前条件链的上一级条件项。如果为空，则其就是顶级条件项。
        [Column(name: "c_num_parent")]
        public string? CNum_Parent {  get; set; }

        public int Level { get; set; }
    }

    public enum ConditionNodeTypes
    {
        ConditionExpression,
        ConditionGroup
    }
}
