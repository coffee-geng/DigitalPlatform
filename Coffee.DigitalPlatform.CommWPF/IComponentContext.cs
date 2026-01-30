using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
        IEnumerable<IAuxiliaryLineContext> GetRulers();

        IEnumerable<IAuxiliaryLineContext> GetRulers(AuxiliaryLineTypes auxiliaryType);

        /// <summary>
        /// 显示组件对齐的标线
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAuxiliaryLineContext> GetLinesToAlign();
        
        IEnumerable<IAuxiliaryLineContext> GetLinesToAlign(AuxiliaryLineTypes auxiliaryType);

        string DeviceType { get; set; }

        bool? IsVisible { get; set; }

        double X { get; set; }

        double Y { get; set; }

        int Z { get; set; }

        double Width { get; set; }

        double Height { get; set; }
    }

    public interface ISaveState
    {
        bool IsDirty { get; }

        void Save();
    }

    public enum AuxiliaryLineTypes
    {
        HorizontalLine,
        VerticalLine,
        HorizontalRuler,
        VerticalRuler
    }

    public enum HorizontalAlignmentModes
    {
        None,
        LeftToLeft, //当前组件左侧对齐到某组件左侧
        LeftToRight, //当前组件左侧对齐到某组件右侧
        RightToRight, //当前组件右侧对齐到某组件右侧
        RightToLeft //当前组件右侧对齐到某组件左侧
    }

    public enum VerticalAlignmentModes
    {
        None,
        TopToTop, //当前组件上端对齐到某组件上端
        TopToBottom, //当前组件上端对齐到某组件下端
        BottomToBottom, //当前组件下端对齐到某组件下端
        BottomToTop //当前组件下端对齐到某组件上端
    }
}
