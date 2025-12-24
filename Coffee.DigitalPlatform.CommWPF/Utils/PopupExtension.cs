using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class PopupExtension
    {
        // 创建附加属性来监听Popup关闭前事件
        public static readonly DependencyProperty ClosingCommandProperty =
            DependencyProperty.RegisterAttached(
                "ClosingCommand",
                typeof(ICommand),
                typeof(PopupExtension),
                new PropertyMetadata(null, OnClosingCommandPropertyChanged));

        public static ICommand GetClosingCommand(Popup popup) =>
            (ICommand)popup.GetValue(ClosingCommandProperty);

        public static void SetClosingCommand(Popup popup, ICommand value) =>
            popup.SetValue(ClosingCommandProperty, value);

        private static void OnClosingCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Popup popup)
            {
                if (e.NewValue != null)
                {
                    // 监听IsOpen属性变化
                    var descriptor = DependencyPropertyDescriptor.FromProperty(
                        Popup.IsOpenProperty, typeof(Popup));
                    descriptor.AddValueChanged(popup, OnIsOpenChanging);
                }
                else
                {
                    var descriptor = DependencyPropertyDescriptor.FromProperty(
                        Popup.IsOpenProperty, typeof(Popup));
                    descriptor.RemoveValueChanged(popup, OnIsOpenChanging);
                }
            }
        }

        private static void OnIsOpenChanging(object sender, EventArgs e)
        {
            if (sender is Popup popup)
            {
                // 获取旧值和新值
                bool isOpen = popup.IsOpen;

                // 这里需要跟踪之前的状态
                // 可以使用附加属性存储之前的状态
                bool wasOpen = GetWasOpen(popup);
                SetWasOpen(popup, isOpen);

                // 如果从打开变为关闭
                if (wasOpen && !isOpen)
                {
                    // 触发关闭前事件
                    var command = GetClosingCommand(popup);
                    if (command?.CanExecute(popup) == true)
                    {
                        command.Execute(popup);
                    }

                    //// 也可以触发路由事件
                    //var args = new CancelRoutedEventArgs(ClosingEvent, popup);
                    //popup.RaiseEvent(args);

                    //// 如果取消关闭
                    //if (args.Cancel)
                    //{
                    //    // 阻止关闭
                    //    popup.IsOpen = true;
                    //    SetWasOpen(popup, true);
                    //}
                }
            }
        }


        public static object GetClosingCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(ClosingCommandParameterProperty);
        }

        public static void SetClosingCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(ClosingCommandParameterProperty, value);
        }

        public static readonly DependencyProperty ClosingCommandParameterProperty =
            DependencyProperty.RegisterAttached("ClosingCommandParameter", typeof(object), typeof(PopupExtension), new PropertyMetadata(null));

        #region 存储之前状态的附加属性
        private static readonly DependencyProperty WasOpenProperty =
            DependencyProperty.RegisterAttached(
                "WasOpen",
                typeof(bool),
                typeof(PopupExtension),
                new PropertyMetadata(false));

        private static bool GetWasOpen(Popup popup) => (bool)popup.GetValue(WasOpenProperty);
        private static void SetWasOpen(Popup popup, bool value) => popup.SetValue(WasOpenProperty, value);
        #endregion

        // 路由事件
        //public static readonly RoutedEvent ClosingEvent =
        //    EventManager.RegisterRoutedEvent(
        //        "Closing",
        //        RoutingStrategy.Bubble,
        //        typeof(CancelRoutedEventHandler),
        //        typeof(PopupExtension));

        //public static void AddBeforeCloseHandler(DependencyObject d, CancelRoutedEventHandler handler)
        //{
        //    if (d is UIElement element)
        //    {
        //        element.AddHandler(ClosingEvent, handler);
        //    }
        //}

        //public static void RemoveBeforeCloseHandler(DependencyObject d, CancelRoutedEventHandler handler)
        //{
        //    if (d is UIElement element)
        //    {
        //        element.RemoveHandler(ClosingEvent, handler);
        //    }
        //}
    }
}
