using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Coffee.DigitalPlatform.Views
{
    public class PopupCloseBehavior : Behavior<Popup>
    {
        public bool CloseOnExternalClick
        {
            get { return (bool)GetValue(CloseOnExternalClickProperty); }
            set { SetValue(CloseOnExternalClickProperty, value); }
        }

        public static readonly DependencyProperty CloseOnExternalClickProperty =
            DependencyProperty.Register("CloseOnExternalClick", typeof(bool), typeof(PopupCloseBehavior), new PropertyMetadata(true));

        /// <summary>
        /// 弹窗当前Popup时的父窗口
        /// </summary>
        public Window Owner
        {
            get { return (Window)GetValue(OwnerProperty); }
            set { SetValue(OwnerProperty, value); }
        }

        public static readonly DependencyProperty OwnerProperty =
            DependencyProperty.Register("Owner", typeof(Window), typeof(PopupCloseBehavior), new PropertyMetadata(null));

        /// <summary>
        /// 在Popup鼠标命中测试中，指定一些元素作为默认命中对象，无论其物理坐标是否在当前这个Popup内。
        /// 换句话说，鼠标在这些元素中点击就像在Popup上点击是一样的效果。
        /// </summary>
        public IList<UIElement> AdditionalHitTestElements
        {
            get { return (IList<UIElement>)GetValue(AdditionalHitTestElementsProperty); }
            set { SetValue(AdditionalHitTestElementsProperty, value); }
        }

        public static readonly DependencyProperty AdditionalHitTestElementsProperty =
            DependencyProperty.Register("AdditionalHitTestElements", typeof(IList<UIElement>), typeof(PopupCloseBehavior), new PropertyMetadata(null));

        private MouseButtonEventHandler _mouseDownHandler;

        protected override void OnAttached()
        {
            base.OnAttached();

            _mouseDownHandler = new MouseButtonEventHandler(OnMouseDown);

            AssociatedObject.Opened += OnPopupOpened;
            AssociatedObject.Closed += OnPopupClosed;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Opened -= OnPopupOpened;
            AssociatedObject.Closed -= OnPopupClosed;
        }

        private void OnPopupOpened(object? sender, EventArgs e)
        {
            if (CloseOnExternalClick)
            {
                AddMouseListener();
            }
        }

        private void OnPopupClosed(object? sender, EventArgs e)
        {
            RemoveMouseListener();
        }

        private void AddMouseListener()
        {
            if (Owner != null)
            {
                Owner.PreviewMouseDown += _mouseDownHandler;
            }
        } 
        
        private void RemoveMouseListener()
        {
            if (Owner != null)
            {
                Owner.PreviewMouseDown -= _mouseDownHandler;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            if (!AssociatedObject.IsOpen)
                return;

            //只有当鼠标点击操作在当前Popup之外命中时，才会自动关闭这个Popup
            if (!IsMouseOverPopup(args))
            {
                AssociatedObject.IsOpen = false;
            }
        }

        //鼠标点击操作是否在当前Popup及其子元素中命中
        private bool IsMouseOverPopup(MouseButtonEventArgs args)
        {
            if (AssociatedObject.Child == null)
                return false;

            var popupElement = AssociatedObject.Child as UIElement;
            if (popupElement == null)
                return false;

            //存放所有在鼠标命中处理中被当作Popup内的元素，即将依赖属性AdditionalHitTestElements指定的元素集添加到这个集合中，即使其可能位于当前Popup之外
            IList<UIElement> hitTestInclusions = new List<UIElement>();
            hitTestInclusions.Add(popupElement);
            if (AdditionalHitTestElements != null && AdditionalHitTestElements.Any())
            {
                foreach (var ele in AdditionalHitTestElements)
                {
                    hitTestInclusions.Add(ele);
                }
            }

            foreach (UIElement element in hitTestInclusions)
            {
                Point mousePosition = args.GetPosition(element);
                bool isHitTest = mousePosition.X >= 0 && mousePosition.X <= popupElement.RenderSize.Width && mousePosition.Y >= 0 && mousePosition.Y <= popupElement.RenderSize.Height;
                if (isHitTest)
                    return true;
            }
            return false;
        }
    }
}
