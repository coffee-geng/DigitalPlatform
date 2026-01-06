using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    //
    // 摘要:
    //     The value validator interface
    //
    // 类型参数:
    //   TValue:
    //     The type of the value
    public interface IValueValidator<in TValue>
    {
        //
        // 摘要:
        //     Determines whether the specified value is valid.
        //
        // 参数:
        //   value:
        //     The value.
        //
        // 返回结果:
        //     true if is valid, otherwise false.
        bool IsValid(TValue value);
    }
}
