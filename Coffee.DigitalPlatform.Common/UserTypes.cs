using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Common
{
    public enum UserTypes
    {
        [Display(Name = "信息管理员")]
        Administrator,
        [Display(Name = "操作员")]
        Operator,
        [Display(Name = "技术员")]
        Engineer
    }
}
