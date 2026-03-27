using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

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

            //如果当前鼠标点击的源是某个下拉框组件，且其在当前的Popup中，则默认命中状态
            //因为下拉框的弹窗部分可能已经跨越当前的Popup，但是如果不是默认命中状态，则点击某个下拉选项，可能会导致当前的Popup关闭，从而不会触发这个下拉框的选中事件
            if (args.OriginalSource != null && args.Source is DependencyObject)
            {
                //沿着视觉树从下拉选项向上查找不能直接找到ComboBox，只能找到ComboBoxItem，因为它在数据控件模板中
                var comboBoxItem = VisualTreeHelperEx.FindAncestorByType<ComboBoxItem>(args.OriginalSource as DependencyObject);
                if (comboBoxItem != null)
                {
                    ComboBox comboBox = ItemsControl.ItemsControlFromItemContainer(comboBoxItem) as ComboBox;

                    var rootElementInPopup = AssociatedObject.Child;
                    if (IsTargetAsAncestorOfElement(comboBox, rootElementInPopup)) //在某个位于当前Popup下的下拉框进行鼠标点击操作
                    {
                        return true;
                    }
                }
            }

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

        /// <summary>
        /// 判断指定组件是否在目标组件的视觉链上。即判断目标组件是否是这个组件本身或其父级组件。
        /// </summary>
        /// <param name="element">指定组件</param>
        /// <param name="targetElement">目标组件</param>
        private bool IsTargetAsAncestorOfElement(DependencyObject element, DependencyObject targetElement)
        {
            if (element == null || targetElement == null)
                return false;
            if (element == targetElement)
                return true;

            var parentElement = VisualTreeHelper.GetParent(element);
            while (parentElement != null)
            {
                if (parentElement == targetElement) 
                    return true;
                parentElement = VisualTreeHelper.GetParent(parentElement);
            }
            return false;
        }
    }
}
