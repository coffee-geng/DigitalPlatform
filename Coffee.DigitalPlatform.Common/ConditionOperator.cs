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
            Name = Enum.GetName(typeof(ConditionOperators), operators);
            if (string.IsNullOrEmpty(operatorName))
            {
                DisplayName = operators.GetDisplayName();
            }
        }

        public  ConditionOperators Operator { get; private set; }

        public string Name { get; private set; }

        public string DisplayName {  get; private set; }
    }

    public enum ConditionOperators
    {
        [Display(Name = "包含")]
        Contains,
        [Display(Name = "头部包含")]
        StartsWith,
        [Display(Name = "尾部包含")]
        EndsWith,
        [Display(Name = "等于")]
        EqualTo,
        [Display(Name = "不等于")]
        NotEqualTo,
        [Display(Name = "大于")]
        GreaterThan,       
        [Display(Name = "小于")]
        LessThan,        
        [Display(Name = "大于等于")]
        GreaterThanOrEqualTo,   
        [Display(Name = "小于等于")]
        LessThanOrEqualTo,
        [Display(Name = "是空字符")]
        IsEmpty,
        [Display(Name = "不是空字符")]
        NotIsEmpty,
        [Display(Name = "是空值")]
        IsNull,
        [Display(Name = "不是空值")]
        NotIsNull,
        [Display(Name = "内容匹配")]
        Matches,
        [Display(Name = "内容不匹配")]
        DoesNotMatch,
        [Display(Name = "不包含")]
        DoesNotContain,
        [Display(Name = "头部不包含")]
        DoesNotStartWith,
        [Display(Name = "尾部不包含")]
        DoesNotEndWith
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
