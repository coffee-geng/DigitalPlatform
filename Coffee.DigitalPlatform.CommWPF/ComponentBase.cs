using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Shell;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class ComponentBase : UserControl
    {
        public ComponentBase()
        {
        }

        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }
        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(ComponentBase),
                new PropertyMetadata(null, new PropertyChangedCallback((d, e) =>
                {
                })));

        public object DeleteParameter
        {
            get { return (object)GetValue(DeleteParameterProperty); }
            set { SetValue(DeleteParameterProperty, value); }
        }
        public static readonly DependencyProperty DeleteParameterProperty =
            DependencyProperty.Register("DeleteParameter", typeof(object), typeof(ComponentBase), new PropertyMetadata(null));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ComponentBase), new PropertyMetadata(false));

        // 
        //public double ShowWidth
        //{
        //    get { return (double)GetValue(ShowWidthProperty); }
        //    set { SetValue(ShowWidthProperty, value); }
        //}
        //public static readonly DependencyProperty ShowWidthProperty =
        //    DependencyProperty.Register("ShowWidth", typeof(double), typeof(ComponentBase), new PropertyMetadata(0.0));

        //public double ShowHeight
        //{
        //    get { return (double)GetValue(ShowHeightProperty); }
        //    set { SetValue(ShowHeightProperty, value); }
        //}
        //public static readonly DependencyProperty ShowHeightProperty =
        //    DependencyProperty.Register("ShowHeight", typeof(double), typeof(ComponentBase), new PropertyMetadata(0.0));

        public ICommand ResizeDownCommand
        {
            get { return (ICommand)GetValue(ResizeDownCommandProperty); }
            set { SetValue(ResizeDownCommandProperty, value); }
        }
        public static readonly DependencyProperty ResizeDownCommandProperty =
            DependencyProperty.Register("ResizeDownCommand", typeof(ICommand), typeof(ComponentBase), new PropertyMetadata(null));

        public ICommand ResizeMoveCommand
        {
            get { return (ICommand)GetValue(ResizeMoveCommandProperty); }
            set { SetValue(ResizeMoveCommandProperty, value); }
        }
        public static readonly DependencyProperty ResizeMoveCommandProperty =
            DependencyProperty.Register("ResizeMoveCommand", typeof(ICommand), typeof(ComponentBase), new PropertyMetadata(null));

        public ICommand ResizeUpCommand
        {
            get { return (ICommand)GetValue(ResizeUpCommandProperty); }
            set { SetValue(ResizeUpCommandProperty, value); }
        }
        public static readonly DependencyProperty ResizeUpCommandProperty =
            DependencyProperty.Register("ResizeUpCommand", typeof(ICommand), typeof(ComponentBase), new PropertyMetadata(null));


        public int RotateAngle
        {
            get { return (int)GetValue(RotateAngleProperty); }
            set { SetValue(RotateAngleProperty, value); }
        }

        public static readonly DependencyProperty RotateAngleProperty =
            DependencyProperty.Register("RotateAngle", typeof(int), typeof(ComponentBase), new PropertyMetadata(0));


        public int FlowDirection
        {
            get { return (int)GetValue(FlowDirectionProperty); }
            set { SetValue(FlowDirectionProperty, value); }
        }
        public static readonly DependencyProperty FlowDirectionProperty =
            DependencyProperty.Register("FlowDirection", typeof(int), typeof(ComponentBase), new PropertyMetadata(0, (d, e) =>
            {
                var state = VisualStateManager.GoToState(d as ComponentBase, e.NewValue.ToString() == "1" ? "EWFlowState" : "WEFlowState", false);
            }));

        public bool IsWarning
        {
            get { return (bool)GetValue(IsWarningProperty); }
            set { SetValue(IsWarningProperty, value); }
        }
        public static readonly DependencyProperty IsWarningProperty =
            DependencyProperty.Register("IsWarning", typeof(bool), typeof(ComponentBase), new PropertyMetadata(false, (d, e) =>
            {
                if (e.NewValue.ToString() != e.OldValue.ToString())
                    VisualStateManager.GoToState(d as ComponentBase, (bool)e.NewValue ? "WarningState" : "NormalState", false);
            }));

        public string WarningMessage
        {
            get { return (string)GetValue(WarningMessageProperty); }
            set { SetValue(WarningMessageProperty, value); }
        }
        public static readonly DependencyProperty WarningMessageProperty =
            DependencyProperty.Register("WarningMessage", typeof(string), typeof(ComponentBase), new PropertyMetadata(""));


        // 是否属于监控状态
        public bool IsMonitor
        {
            get { return (bool)GetValue(IsMonitorProperty); }
            set { SetValue(IsMonitorProperty, value); }
        }
        public static readonly DependencyProperty IsMonitorProperty =
            DependencyProperty.Register("IsMonitor", typeof(bool), typeof(ComponentBase), new PropertyMetadata(false));

        public object VariableList
        {
            get { return (object)GetValue(VariableListProperty); }
            set { SetValue(VariableListProperty, value); }
        }
        public static readonly DependencyProperty VariableListProperty =
            DependencyProperty.Register("VariableList", typeof(object), typeof(ComponentBase), new PropertyMetadata(null));

        public object ManualList
        {
            get { return (object)GetValue(ManualListProperty); }
            set { SetValue(ManualListProperty, value); }
        }
        public static readonly DependencyProperty ManualListProperty =
            DependencyProperty.Register("ManualList", typeof(object), typeof(ComponentBase), new PropertyMetadata(null));

        public ICommand ManualControlCommand
        {
            get { return (ICommand)GetValue(ManualControlCommandProperty); }
            set { SetValue(ManualControlCommandProperty, value); }
        }
        public static readonly DependencyProperty ManualControlCommandProperty =
            DependencyProperty.Register("ManualControlCommand", typeof(ICommand), typeof(ComponentBase), new PropertyMetadata(null));

        public ICommand AlarmDetailCommand
        {
            get { return (ICommand)GetValue(AlarmDetailCommandProperty); }
            set { SetValue(AlarmDetailCommandProperty, value); }
        }

        public static readonly DependencyProperty AlarmDetailCommandProperty =
            DependencyProperty.Register("AlarmDetailCommand", typeof(ICommand), typeof(ComponentBase), new PropertyMetadata(null));



        public Canvas Canvas
        {
            get { return (Canvas)GetValue(CanvasProperty); }
            set { SetValue(CanvasProperty, value); }
        }
        public static readonly DependencyProperty CanvasProperty =
            DependencyProperty.Register("Canvas", typeof(Canvas), typeof(ComponentBase), new PropertyMetadata(null, OnCanvasPropertyChanged));
        private static void OnCanvasPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #region 处理组件尺寸缩放逻辑

        //缩放前组件的尺寸
        double _oldWidth, _oldHeight;
        //缩放时，用于计算是否要进行对齐判断的组件集合
        private IEnumerable<IComponentContext> _componentsToCheckAlign;
        //显示组件长或宽的标尺
        private IEnumerable<IAuxiliaryLineContext> _rulers;
        //显示组件对齐的标线
        private IEnumerable<IAuxiliaryLineContext> _alignLines;

        protected void doResizeStart()
        {
            _oldWidth = this.ActualWidth;
            _oldHeight = this.ActualHeight;

            IComponentContext vm = this.DataContext as IComponentContext;
            if (vm != null)
            {
                _componentsToCheckAlign = vm.GetComponentsToCheckAlign();
            }
        }

        /// <summary>
        /// 拖拉鼠标执行缩放操作。
        /// </summary>
        /// <param name="delta">缩放时，鼠标拖放指定的缩放大小</param>
        /// <param name="resizeDirection">缩放的方向，有水平，垂直，任意三个方向</param>
        /// <param name="isAlign">缩放时是否对齐其他的组件</param>
        /// <param name="isProportional">是否等比例缩放，仅对任意方向缩放有效</param>
        protected void doResizing(Vector delta, ResizeGripDirection resizeDirection, bool isAlign, bool isProportional)
        {
            IComponentContext vm = this.DataContext as IComponentContext;

            if (resizeDirection == ResizeGripDirection.Left || resizeDirection == ResizeGripDirection.Right)
            {
                if (isAlign)
                {
                   
                }
                else
                {
                    vm.Width = _oldWidth + delta.X;
                }
            }
            else if (resizeDirection == ResizeGripDirection.Top || resizeDirection == ResizeGripDirection.Bottom)
            {
                if (isAlign)
                {

                }
                else
                {
                    vm.Height = _oldHeight + delta.Y;
                }
            }
            else if (resizeDirection == ResizeGripDirection.TopLeft || resizeDirection == ResizeGripDirection.TopRight || resizeDirection == ResizeGripDirection.BottomLeft || resizeDirection == ResizeGripDirection.BottomRight)
            {
                if (isProportional)
                {

                }
                else
                {

                }
            }
        }

        protected void doResizeEnd()
        {

        }
        #endregion

        #region 处理组件移动逻辑
        bool _isMoving = false;
        Point startPoint = new Point(0, 0);
        double _oldX = 0;
        double _oldY = 0;

        public void OnComponent_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IComponentContext vm = this.DataContext as IComponentContext;
            if (vm == null)
                return;
            if (Canvas == null)
                return;

            _componentsToCheckAlign = vm.GetComponentsToCheckAlign();
            _rulers = vm.GetRulers();

            _oldX = vm.X;
            _oldY = vm.Y;
            startPoint = e.GetPosition(Canvas);
            _isMoving = true;

            Mouse.Capture((IInputElement)sender);
            e.Handled = true;
        }

        public void OnComponent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            startPoint = new Point(0, 0);
            _oldX = 0;
            _oldY = 0;
            _isMoving = false;
            foreach (var item in _rulers)
            {
                if (item is FrameworkElement ele)
                {
                    ele.Visibility = Visibility.Collapsed;
                }
            }
            Mouse.Capture(null);
        }

        public void OnComponent_MouseMove(object sender, MouseEventArgs e)
        {
            IComponentContext vm = this.DataContext as IComponentContext;
            if (vm == null)
                return;
            if (Canvas == null)
                return;

            if (_isMoving)
            {
                bool isAlign = Keyboard.Modifiers != ModifierKeys.Alt;

                Point p = e.GetPosition(Canvas);
                double deltaX = p.X - startPoint.X;
                double deltaY = p.Y - startPoint.Y;

                if (isAlign && _componentsToCheckAlign != null && _componentsToCheckAlign.Any())
                {
                    //计算水平方向的对齐线
                    double minDeltaX = 20; //相距20像素之内，就需要对齐
                    //判断是否有其他组件的左端与当前组件左端对齐
                    IComponentContext alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.X - vm.X) < minDeltaX).Min();
                    if (alignComponent == null)
                    {
                        //判断是否有其他组件的右端与当前组件左端对齐
                        alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.X + comp.Width - vm.X) < minDeltaX).Min();
                        if (alignComponent == null)
                        {
                            //判断是否有其他组件的右端与当前组件右端对齐
                            alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.X + comp.Width - (vm.X + vm.Width)) < minDeltaX).Min();
                            if (alignComponent == null)
                            {
                                //判断是否有其他组件的左端与当前组件右端对齐
                                alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.X - (vm.X + vm.Width)) < minDeltaX).Min();
                            }
                        }
                    }
                    if (alignComponent != null) //有可以对齐的组件
                    {
                        var verticalLines = vm.GetLinesToAlign(AuxiliaryLineTypes.VerticalLine);
                        if (verticalLines != null && verticalLines.Any())
                        {
                            var alignXLine = verticalLines.First();
                            alignXLine.IsVisible = true;
                            foreach(var line in verticalLines)
                            {
                                if (line == alignXLine)
                                    continue;
                                line.IsVisible = false;
                            }
                        }
                    }
                }
                else
                {
                    vm.X = _oldX + deltaX;
                    vm.Y = _oldY + deltaY;
                }
            }
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteCommand?.Execute(DeleteParameter);
        }
        #endregion
    }
}
