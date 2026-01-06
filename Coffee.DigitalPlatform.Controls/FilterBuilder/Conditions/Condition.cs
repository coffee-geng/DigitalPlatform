using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public enum Condition
    {
        [Display(Name = "FilterBuilder_Contains")]
        Contains,
        [Display(Name = "FilterBuilder_StartsWith")]
        StartsWith,
        [Display(Name = "FilterBuilder_EndsWith")]
        EndsWith,
        [Display(Name = "FilterBuilder_EqualTo")]
        EqualTo,
        [Display(Name = "FilterBuilder_NotEqualTo")]
        NotEqualTo,
        [Display(Name = "FilterBuilder_GreaterThan")]
        GreaterThan,
        [Display(Name = "FilterBuilder_LessThan")]
        LessThan,
        [Display(Name = "FilterBuilder_GreaterThanOrEqualTo")]
        GreaterThanOrEqualTo,
        [Display(Name = "FilterBuilder_LessThanOrEqualTo")]
        LessThanOrEqualTo,
        [Display(Name = "FilterBuilder_IsEmpty")]
        IsEmpty,
        [Display(Name = "FilterBuilder_NotIsEmpty")]
        NotIsEmpty,
        [Display(Name = "FilterBuilder_IsNull")]
        IsNull,
        [Display(Name = "FilterBuilder_NotIsNull")]
        NotIsNull,
        [Display(Name = "FilterBuilder_Matches")]
        Matches,
        [Display(Name = "FilterBuilder_DoesNotMatch")]
        DoesNotMatch,
        [Display(Name = "FilterBuilder_DoesNotContain")]
        DoesNotContain,
        [Display(Name = "FilterBuilder_DoesNotStartWith")]
        DoesNotStartWith,
        [Display(Name = "FilterBuilder_DoesNotEndWith")]
        DoesNotEndWith
    }
}
