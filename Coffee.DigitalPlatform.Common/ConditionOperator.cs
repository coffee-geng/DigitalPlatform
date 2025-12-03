using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Common
{
    public class ConditionOperator
    {
        public ConditionOperator(ConditionOperators operators, string operatorName = null) 
        {
            Operator = operators;

            if (string.IsNullOrEmpty(operatorName))
            {
                Name = operators.GetDisplayName();
            }
        }

        public  ConditionOperators Operator { get; private set; }

        public string Name { get; private set; }
    }

    public enum ConditionOperators
    {
        [Display(Name = "等于")]
        Equal,           
        [Display(Name = "大于")]
        GreatThan,       
        [Display(Name = "小于")]
        LessThan,        
        [Display(Name = "大于等于")]
        GreateOrEqual,   
        [Display(Name = "小于等于")]
        LessOrEqual,     
        [Display(Name = "包含")]
        Contains
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            DisplayAttribute attribute = field.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? value.ToString();
        }
    }
}
