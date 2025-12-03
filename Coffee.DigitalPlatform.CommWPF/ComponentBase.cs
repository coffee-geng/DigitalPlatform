using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class ComponentBase : UserControl
    {
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

        public object VarList
        {
            get { return (object)GetValue(VarListProperty); }
            set { SetValue(VarListProperty, value); }
        }
        public static readonly DependencyProperty VarListProperty =
            DependencyProperty.Register("VarList", typeof(object), typeof(ComponentBase), new PropertyMetadata(null));



        public object ControlList
        {
            get { return (object)GetValue(ControlListProperty); }
            set { SetValue(ControlListProperty, value); }
        }
        public static readonly DependencyProperty ControlListProperty =
            DependencyProperty.Register("ControlList", typeof(object), typeof(ComponentBase), new PropertyMetadata(null));



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


        bool is_move = false;
        Point start = new Point(0, 0);
        public void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //// 修改ShwoWidth/ShowHeight
            //is_move = true;

            //// 获取相对Canvas的按下坐标
            //start = e.GetPosition(GetParent(this));
            //Mouse.Capture((IInputElement)sender);
            //e.Handled = true;

            ResizeDownCommand?.Execute(e);
        }

        public void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 修改ShwoWidth/ShowHeight
            //is_move = false;
            //e.Handled = true;
            //Mouse.Capture(null);

            ResizeUpCommand?.Execute(e);
        }

        public void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            ResizeMoveCommand?.Execute(e);

            // 修改ShwoWidth/ShowHeight
            //if (is_move)
            //{
            //    // 鼠标光标的新位置
            //    Point current = e.GetPosition(GetParent(this));
            //    // 根据光标类型判断是如何变化 
            //    var c = (sender as Ellipse).Cursor;
            //    if (c != null)
            //    {
            //        if (c == Cursors.SizeWE)// 水平方向
            //        {
            //            this.ShowWidth += current.X - start.X;
            //        }
            //        else if (c == Cursors.SizeNS)// 垂直方向
            //        {
            //            this.ShowHeight += current.Y - start.Y;
            //        }
            //        else if (c == Cursors.SizeNWSE)// 右下方向
            //        {
            //            if (Keyboard.Modifiers == ModifierKeys.Control)
            //            {
            //                // 没有锁定（如果拖动的时候同时按下了Ctrl键 ）
            //                this.ShowWidth += current.X - start.X;
            //                this.ShowHeight += current.Y - start.Y;
            //            }
            //            else
            //            {
            //                // 锁定比例
            //                var rate = this.ShowWidth / this.ShowHeight;
            //                this.ShowWidth += current.X - start.X;
            //                this.ShowHeight = this.ShowWidth / rate;
            //            }
            //        }
            //        start = current;
            //    }
            //    e.Handled = true;
            //}
        }
        //private Canvas GetParent(DependencyObject d)
        //{
        //    var obj = VisualTreeHelper.GetParent(d);
        //    if (obj != null && obj is Canvas)
        //        return obj as Canvas;

        //    return GetParent(obj);
        //}


        public void Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteCommand?.Execute(DeleteParameter);
        }
    }
}
