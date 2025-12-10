using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Coffee.DigitalPlatform.CommWPF
{
    public interface IComponentContext : IUIElementContext
    {
        /// <summary>
        /// 缩放时，用于计算是否要进行对齐判断的组件集合
        /// </summary>
        IEnumerable<IComponentContext> GetComponentsToCheckAlign();

        /// <summary>
        /// 显示组件长或宽的标尺
        /// </summary>
        IEnumerable<IAuxiliaryLineContext> GetRulers();

        IEnumerable<IAuxiliaryLineContext> GetRulers(AuxiliaryLineTypes auxiliaryType);

        /// <summary>
        /// 显示组件对齐的标线
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAuxiliaryLineContext> GetLinesToAlign();
        
        IEnumerable<IAuxiliaryLineContext> GetLinesToAlign(AuxiliaryLineTypes auxiliaryType);

        string DeviceType { get; set; }
    }

    public interface IUIElementContext
    {
        double X { get; set; }

        double Y { get; set; }

        int Z { get; set; }

        double Width { get; set; }

        double Height { get; set; }
    }

    public enum AuxiliaryLineTypes
    {
        HorizontalLine,
        VerticalLine,
        HorizontalRuler,
        VerticalRuler
    }
}
