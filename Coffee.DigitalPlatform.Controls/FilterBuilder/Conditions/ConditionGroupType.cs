using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public enum ConditionGroupType
    {
        [Display(Name = "FilterBuilder_And")]
        And,
        [Display(Name = "FilterBuilder_Or")]
        Or
    }
}
