using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.CommWPF
{
    public interface IComponentContext
    {
        /// <summary>
        /// 缩放时，用于计算是否要进行对齐判断的组件集合
        /// </summary>
        IEnumerable<IComponentContext> GetComponentsToCheckAlign();

        /// <summary>
        /// 显示组件长或宽的标尺
        /// </summary>
        IEnumerable<IComponentContext> GetRulers();
    }
}
