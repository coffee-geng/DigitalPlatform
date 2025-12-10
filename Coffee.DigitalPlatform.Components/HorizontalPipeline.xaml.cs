using Coffee.DigitalPlatform.CommWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace Coffee.DigitalPlatform.Components
{
    /// <summary>
    /// HorizontalPipeline.xaml 的交互逻辑
    /// </summary>
    public partial class HorizontalPipeline : ComponentBase
    {
        public HorizontalPipeline()
        {
            InitializeComponent();

            this.anchor.OnResizeStart += Anchor_OnResizeStart;
            this.anchor.OnResizing += Anchor_OnResizing;
            this.anchor.OnResizeEnd += Anchor_OnResizeEnd;

            var state = VisualStateManager.GoToState(this, "WEFlowState", false);
        }

        /// <summary>
        /// 流体的颜色
        /// </summary>
        public Brush LiquidColor
        {
            get { return (Brush)GetValue(LiquidColorProperty); }
            set { SetValue(LiquidColorProperty, value); }
        }
        public static readonly DependencyProperty LiquidColorProperty =
            DependencyProperty.Register("LiquidColor", typeof(Brush), typeof(HorizontalPipeline),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9916a1ff"))));

        private void Anchor_OnResizeStart()
        {
            doResizeStart();
        }

        private void Anchor_OnResizing(Vector delta, ResizeGripDirection resizeDirection, bool isAlign, bool isProportional)
        {
            doResizing(delta, resizeDirection, isAlign, isProportional);
        }

        private void Anchor_OnResizeEnd()
        {
            doResizeEnd();
        }
    }
}
