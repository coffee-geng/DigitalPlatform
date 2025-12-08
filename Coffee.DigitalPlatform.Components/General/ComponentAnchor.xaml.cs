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
    /// ComponentAnchor.xaml 的交互逻辑
    /// </summary>
    public partial class ComponentAnchor : UserControl
    {
        public ComponentAnchor()
        {
            InitializeComponent();
        }

        public Canvas Container
        {
            get { return (Canvas)GetValue(ContainerProperty); }
            set { SetValue(ContainerProperty, value); }
        }
        public static readonly DependencyProperty ContainerProperty =
            DependencyProperty.Register("Container", typeof(Canvas), typeof(ComponentAnchor), new PropertyMetadata(null));


        Point startP = new Point(0, 0);

        bool _isResize = false;

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Container == null)
                return;
            _isResize = true;
            startP = e.GetPosition(Container);

            // 获取相对Canvas的按下坐标
            Mouse.Capture((IInputElement)e.Source);
            e.Handled = true;

            if (OnResizeStart != null)
            {
                OnResizeStart();
            }
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (Container == null)
                return;

            if (_isResize)
            {
                ResizeGripDirection resizeDirection = ResizeGripDirection.None;
                double deltaX = 0;
                double deltaY = 0;
                bool isAlign = true; //默认对齐操作
                bool isProportional = false; //默认不进行等比例操作
                // 鼠标光标的新位置
                Point current = e.GetPosition(Container);
                // 根据光标类型判断是如何变化 
                var c = (e.Source as Ellipse).Cursor;
                if (c != null)
                {
                    if (c == Cursors.SizeWE)// 水平方向
                    {
                        deltaX = current.X - startP.X;
                        resizeDirection = ResizeGripDirection.Right;
                        if (Keyboard.Modifiers == ModifierKeys.Alt)  // 移动过程中检查Alt按下，不做对齐
                        {
                            isAlign = false;
                        }
                    }
                    else if (c == Cursors.SizeNS)// 垂直方向
                    {
                        deltaY = current.Y - startP.Y;
                        resizeDirection = ResizeGripDirection.Bottom;
                        if (Keyboard.Modifiers == ModifierKeys.Alt)
                        {
                            isAlign = false;
                        }
                    }
                    else if (c == Cursors.SizeNWSE)// 右下方向
                    {
                        deltaX = current.X - startP.X;
                        deltaY = current.Y - startP.Y;
                        if (Keyboard.Modifiers == ModifierKeys.Alt)
                        {
                            isAlign = false;
                        }
                        if (Keyboard.Modifiers == ModifierKeys.Shift)
                        {
                            isProportional = true;
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isResize = false;
            e.Handled = true;
            Mouse.Capture(null);

            if (OnResizeEnd != null)
            {
                OnResizeEnd();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OnDelete != null)
            {
                OnDelete();
            }
        }

        #region 组件缩放操作委托

        public event Action OnResizeStart;

        /// <summary>
        /// 当拖拉组件进行缩放操作时触发此事件。传递的参数分别是：
        /// 参数1：缩放时，鼠标拖放指定的缩放大小
        /// 参数2：缩放的方向，有水平，垂直，任意三个方向
        /// 参数3：缩放时是否对齐其他的组件
        /// 参数4：是否等比例缩放，仅对任意方向缩放有效
        /// </summary>
        public event Action<Vector, ResizeGripDirection, bool, bool> OnResizing;

        public event Action OnResizeEnd;
        #endregion

        public event Action OnDelete;
    }
}
