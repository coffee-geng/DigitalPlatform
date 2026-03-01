using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Shell;
using CommunityToolkit.Mvvm.Messaging;
using System.Runtime.CompilerServices;

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

        public bool IsShowingManualListPopup
        {
            get { return (bool)GetValue(IsShowingManualListPopupProperty); }
            set { SetValue(IsShowingManualListPopupProperty, value); }
        }
        public static readonly DependencyProperty IsShowingManualListPopupProperty =
            DependencyProperty.Register("IsShowingManualListPopup", typeof(bool), typeof(ComponentBase), new PropertyMetadata(false));

        public bool IsShowingVariableListPopup
        {
            get { return (bool)GetValue(IsShowingVariableListPopupProperty); }
            set { SetValue(IsShowingVariableListPopupProperty, value); }
        }
        public static readonly DependencyProperty IsShowingVariableListPopupProperty =
            DependencyProperty.Register("IsShowingVariableListPopup", typeof(bool), typeof(ComponentBase), new PropertyMetadata(false));

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

        public ICommand ChangeManualListPopupVisibilityCommand
        {
            get { return (ICommand)GetValue(ChangeManualListPopupVisibilityCommandProperty); }
            set { SetValue(ChangeManualListPopupVisibilityCommandProperty, value); }
        }
        public static readonly DependencyProperty ChangeManualListPopupVisibilityCommandProperty =
            DependencyProperty.Register("ChangeManualListPopupVisibilityCommand", typeof(ICommand), typeof(ComponentBase), new PropertyMetadata(null));


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

            bool isShowAuxiliary = false;
            if (resizeDirection == ResizeGripDirection.Left || resizeDirection == ResizeGripDirection.Right)
            {
                double alignX = 0;
                if (isAlign && _componentsToCheckAlign != null && _componentsToCheckAlign.Any())
                {
                    var componentToMatchRight = findComponentToMatchRight(vm, delta.X, out double matchWidth, out alignX);
                    if (componentToMatchRight != null)
                    {
                        isShowAuxiliary = true;
                    }
                }
                if (isShowAuxiliary)
                {
                    WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.VerticalLine)
                            {
                                X = alignX,
                                IsVisible = true
                            }));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.VerticalLine)
                            {
                                IsVisible = false
                            }));
                }
                vm.Width = _oldWidth + delta.X;
            }
            else if (resizeDirection == ResizeGripDirection.Top || resizeDirection == ResizeGripDirection.Bottom)
            {
                double alignY = 0;
                if (isAlign && _componentsToCheckAlign != null && _componentsToCheckAlign.Any())
                {
                    var componentToMatchBottom = findComponentToMatchBottom(vm, delta.Y, out double matchHeight, out alignY);
                    if (componentToMatchBottom != null)
                    {
                        isShowAuxiliary = true;
                    }
                }
                if (isShowAuxiliary)
                {
                    WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.HorizontalLine)
                            {
                                Y = alignY,
                                IsVisible = true
                            }));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.HorizontalLine)
                            {
                                IsVisible = false
                            }));
                }
                vm.Height = _oldHeight + delta.Y;
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

        protected void doResizeEnd(Vector delta, ResizeGripDirection resizeDirection, bool isAlign, bool isProportional)
        {
            IComponentContext vm = this.DataContext as IComponentContext;
            if (vm != null && Canvas != null)
            {
                if (isAlign && _componentsToCheckAlign != null && _componentsToCheckAlign.Any())
                {
                    if (resizeDirection == ResizeGripDirection.Left || resizeDirection == ResizeGripDirection.Right)
                    {
                        var alignXComponent = findComponentToMatchRight(vm, delta.X, out double matchWidth, out double alignX);
                        if (alignXComponent != null)
                        {
                            vm.Width = alignXComponent.X + alignXComponent.Width - vm.X;
                        }
                    }
                    else if (resizeDirection == ResizeGripDirection.Top || resizeDirection == ResizeGripDirection.Bottom)
                    {
                        var alignYComponent = findComponentToMatchBottom(vm, delta.Y, out double matchHeight, out double alignY);
                        if (alignYComponent != null)
                        {
                            vm.Height = alignYComponent.Y + alignYComponent.Height - vm.Y;
                        }
                    }
                }
                WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                    new AuxiliaryInfo(AuxiliaryLineTypes.VerticalLine)
                    {
                        IsVisible = false
                    }));
                WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                        new AuxiliaryInfo(AuxiliaryLineTypes.HorizontalLine)
                        {
                            IsVisible = false
                        }));
            }
        }

        private IComponentContext findComponentToMatchRight(IComponentContext curComponent, double deltaX, out double matchWidth, out double alignX)
        {
            double minDeltaX = 20; //相距20像素之内，就需要对齐

            //判断是否有其他组件的右端与当前组件右端对齐
            IComponentContext alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.X + comp.Width - (curComponent.X + _oldWidth + deltaX)) < minDeltaX).MinBy(c => Math.Abs(c.X + c.Width - (curComponent.X + _oldWidth + deltaX)));
            if (alignComponent != null)
            {
                matchWidth = _oldWidth + deltaX;
                alignX = alignComponent.X + alignComponent.Width;
            }
            else
            {
                matchWidth = 0;
                alignX = 0;
            }
            return alignComponent;
        }

        private IComponentContext findComponentToMatchBottom(IComponentContext curComponent, double deltaY, out double matchHeight, out double alignY)
        {
            double minDeltaY = 20; //相距20像素之内，就需要对齐

            //判断是否有其他组件的下端与当前组件下端对齐
            IComponentContext alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.Y + comp.Height - (curComponent.Y + _oldHeight + deltaY)) < minDeltaY).MinBy(c => Math.Abs(c.Y + c.Height - (curComponent.Y + _oldHeight + deltaY)));
            if (alignComponent != null)
            {
                matchHeight = _oldHeight + deltaY;
                alignY = alignComponent.Y + alignComponent.Height;
            }
            else
            {
                matchHeight = 0;
                alignY = 0;
            }
            return alignComponent;
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
            IComponentContext vm = this.DataContext as IComponentContext;
            if (vm != null && Canvas != null)
            {
                if (_isMoving)
                {
                    bool isAlign = Keyboard.Modifiers != ModifierKeys.Alt;
                    Point p = e.GetPosition(Canvas);
                    double deltaX = p.X - startPoint.X;
                    double deltaY = p.Y - startPoint.Y;

                    if (isAlign && _componentsToCheckAlign != null && _componentsToCheckAlign.Any())
                    {
                        //计算水平方向的对齐线VerticalLine
                        double alignX = 0;
                        HorizontalAlignmentModes? alignXMode;
                        var alignXComponent = findComponentToHorizontalAlign(vm, out alignX, out alignXMode, out double y1, out double y2);
                        if (alignXComponent != null && alignXMode.HasValue)
                        {
                            switch (alignXMode.Value)
                            {
                                case HorizontalAlignmentModes.LeftToLeft:
                                    vm.X = alignXComponent.X;
                                    break;
                                case HorizontalAlignmentModes.LeftToRight:
                                    vm.X = alignXComponent.X + alignXComponent.Width;
                                    break;
                                case HorizontalAlignmentModes.RightToRight:
                                    vm.X = alignXComponent.X + alignXComponent.Width - vm.Width;
                                    break;
                                case HorizontalAlignmentModes.RightToLeft:
                                    vm.X = alignXComponent.X - vm.Width;
                                    break;
                            }
                        }

                        //计算垂直方向的对齐线HorizontalLine
                        double alignY = 0;
                        VerticalAlignmentModes? alignYMode;
                        var alignYComponent = findComponentToVerticalAlign(vm, out alignY, out alignYMode, out double x1, out double x2);
                        if (alignYComponent != null && alignYMode.HasValue)
                        {
                            switch (alignYMode.Value)
                            {
                                case VerticalAlignmentModes.TopToTop:
                                    vm.Y = alignYComponent.Y;
                                    break;
                                case VerticalAlignmentModes.TopToBottom:
                                    vm.Y = alignYComponent.Y + alignYComponent.Height;
                                    break;
                                case VerticalAlignmentModes.BottomToBottom:
                                    vm.Y = alignYComponent.Y + alignYComponent.Height - vm.Height;
                                    break;
                                case VerticalAlignmentModes.BottomToTop:
                                    vm.Y = alignYComponent.Y - vm.Height;
                                    break;
                            }
                        }
                    }
                    WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.VerticalLine)
                            {
                                IsVisible = false
                            }));
                    WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.HorizontalLine)
                            {
                                IsVisible = false
                            }));
                }
            }

            startPoint = new Point(0, 0);
            _oldX = 0;
            _oldY = 0;
            _isMoving = false;
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
                    //计算水平方向的对齐线VerticalLine
                    double alignX = 0;
                    HorizontalAlignmentModes? alignXMode;
                    var alignXComponent = findComponentToHorizontalAlign(vm, out alignX, out alignXMode, out double y1, out double y2);
                    if (alignXComponent != null) //有可以对齐的组件
                    {
                        WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.VerticalLine)
                            {
                                X = alignX,
                                IsVisible = true,
                                Y = y1,
                                Height = y2 - y1
                            }));
                    }
                    else
                    {
                        WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.VerticalLine)
                            {
                                IsVisible = false
                            }));
                    }

                    //计算垂直方向的对齐线HorizontalLine
                    double alignY = 0;
                    VerticalAlignmentModes? alignYMode;
                    var alignYComponent = findComponentToVerticalAlign(vm, out alignY, out alignYMode, out double x1, out double x2);
                    if (alignYComponent != null) //有可以对齐的组件
                    {
                        WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.HorizontalLine)
                            {
                                Y = alignY,
                                IsVisible = true,
                                X = x1,
                                Width = x2 - x1
                            }));
                    }
                    else
                    {
                        WeakReferenceMessenger.Default.Send<RepaintAuxiliaryMessage>(new RepaintAuxiliaryMessage(
                            new AuxiliaryInfo(AuxiliaryLineTypes.HorizontalLine)
                            {
                                IsVisible = false
                            }));
                    }
                }
                vm.X = _oldX + deltaX;
                vm.Y = _oldY + deltaY;
            }
        }

        private IComponentContext findComponentToHorizontalAlign(IComponentContext curComponent, out double alignX, out HorizontalAlignmentModes? alignXMode, out double y1, out double y2)
        {
            double minDeltaX = 20; //相距20像素之内，就需要对齐
            alignXMode = null;
            if (curComponent == null)
            {
                alignX = 0;
                y1 = 0;
                y2 = 0;
                return null;
            }

            //判断是否有其他组件的左端与当前组件左端对齐
            IComponentContext alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.X - curComponent.X) < minDeltaX).MinBy(c => Math.Abs(c.X - curComponent.X));
            if (alignComponent == null)
            {
                //判断是否有其他组件的右端与当前组件左端对齐
                alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.X + comp.Width - curComponent.X) < minDeltaX).MinBy(c => Math.Abs(c.X + c.Width - curComponent.X));
                if (alignComponent == null)
                {
                    //判断是否有其他组件的右端与当前组件右端对齐
                    alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.X + comp.Width - (curComponent.X + curComponent.Width)) < minDeltaX).MinBy(c => Math.Abs(c.X + c.Width - (curComponent.X + curComponent.Width)));
                    if (alignComponent == null)
                    {
                        //判断是否有其他组件的左端与当前组件右端对齐
                        alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.X - (curComponent.X + curComponent.Width)) < minDeltaX).MinBy(c => Math.Abs(c.X - (curComponent.X + curComponent.Width)));
                        if (alignComponent != null)
                        {
                            alignX = alignComponent.X;
                            alignXMode = HorizontalAlignmentModes.RightToLeft;
                        }
                        else
                        {
                            alignX = 0;
                        }
                    }
                    else
                    {
                        alignX = alignComponent.X + alignComponent.Width;
                        alignXMode = HorizontalAlignmentModes.RightToRight;
                    }
                }
                else
                {
                    alignX = alignComponent.X + alignComponent.Width;
                    alignXMode = HorizontalAlignmentModes.LeftToRight;
                }
            }
            else
            {
                alignX = alignComponent.X;
                alignXMode = HorizontalAlignmentModes.LeftToLeft;
            }

            if (alignComponent != null)
            {
                if (alignComponent.Y > curComponent.Y) //对齐目标组件在当前组件下方
                {
                    y1 = curComponent.Y;
                    y2 = alignComponent.Y + alignComponent.Height;
                }
                else //对齐目标组件在当前组件上方
                {
                    y1 = alignComponent.Y;
                    y2 = curComponent.Y + curComponent.Height;
                }
            }
            else
            {
                y1 = 0;
                y2 = 0;
            }
            return alignComponent;
        }

        private IComponentContext findComponentToVerticalAlign(IComponentContext curComponent, out double alignY, out VerticalAlignmentModes? alignYMode, out double x1, out double x2)
        {
            double minDeltaY = 20; //相距20像素之内，就需要对齐
            alignYMode = null;
            if (curComponent == null)
            {
                alignY = 0;
                x1 = 0;
                x2 = 0;
                return null;
            }

            //判断是否有其他组件的上端与当前组件上端对齐
            IComponentContext alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.Y - curComponent.Y) < minDeltaY).MinBy(c => Math.Abs(c.Y - curComponent.Y));
            if (alignComponent == null)
            {
                //判断是否有其他组件的下端与当前组件上端对齐
                alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.Y + comp.Height - curComponent.Y) < minDeltaY).MinBy(c => Math.Abs(c.Y + c.Height - curComponent.Y));
                if (alignComponent == null)
                {
                    //判断是否有其他组件的下端与当前组件下端对齐
                    alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.Y + comp.Height - (curComponent.Y + curComponent.Height)) < minDeltaY).MinBy(c => Math.Abs(c.Y + c.Height - (curComponent.Y + curComponent.Height)));
                    if (alignComponent == null)
                    {
                        //判断是否有其他组件的上端与当前组件下端对齐
                        alignComponent = _componentsToCheckAlign.Where(comp => Math.Abs(comp.Y - (curComponent.Y + curComponent.Height)) < minDeltaY).MinBy(c => Math.Abs(c.Y - (curComponent.Y + curComponent.Height)));
                        if (alignComponent != null)
                        {
                            alignY = alignComponent.Y;
                            alignYMode = VerticalAlignmentModes.BottomToTop;
                        }
                        else
                        {
                            alignY = 0;
                        }
                    }
                    else
                    {
                        alignY = alignComponent.Y + alignComponent.Height;
                        alignYMode = VerticalAlignmentModes.BottomToBottom;
                    }
                }
                else
                {
                    alignY = alignComponent.Y + alignComponent.Height;
                    alignYMode = VerticalAlignmentModes.TopToBottom;
                }
            }
            else
            {
                alignY = alignComponent.Y;
                alignYMode = VerticalAlignmentModes.TopToTop;
            }

            if (alignComponent != null)
            {
                if (alignComponent.X > curComponent.X) //对齐目标组件在当前组件右侧
                {
                    x1 = curComponent.X;
                    x2 = alignComponent.X + alignComponent.Width;
                }
                else //对齐目标组件在当前组件上方
                {
                    x1 = alignComponent.X;
                    x2 = curComponent.X + curComponent.Width;
                }
            }
            else
            {
                x1 = 0;
                x2 = 0;
            }
            return alignComponent;
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteCommand?.Execute(DeleteParameter);
        }
        #endregion
    }
}
