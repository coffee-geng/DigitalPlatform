using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.CommWPF
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;

    public static class FocusHelper
    {
        // 获取指定容器中所有具有键盘焦点的控件
        public static List<UIElement> GetAllKeyboardFocusedElements(DependencyObject container)
        {
            var focusedElements = new List<UIElement>();

            if (container == null) return focusedElements;

            // 遍历视觉树
            TraverseVisualTree(container, (element) =>
            {
                //if (element is UIElement uiElement && uiElement.IsKeyboardFocused)
                if (element is UIElement uiElement && uiElement is TextBox)
                {
                    focusedElements.Add(uiElement);
                }
            });

            return focusedElements;
        }

        // 获取指定容器中所有具有逻辑焦点的控件
        public static List<UIElement> GetAllLogicalFocusedElements(DependencyObject container)
        {
            var focusedElements = new List<UIElement>();

            if (container == null) return focusedElements;

            // 获取容器内的所有焦点作用域
            var focusScopes = GetFocusScopes(container);

            foreach (var scope in focusScopes)
            {
                var focusedElement = FocusManager.GetFocusedElement(scope) as UIElement;
                if (focusedElement != null && !focusedElements.Contains(focusedElement))
                {
                    focusedElements.Add(focusedElement);
                }
            }

            return focusedElements;
        }

        // 递归遍历视觉树
        private static void TraverseVisualTree(DependencyObject parent, Action<DependencyObject> action)
        {
            if (parent == null) return;

            // 对当前元素执行操作
            action(parent);
            // 递归处理子元素
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            if (childrenCount > 0)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    TraverseVisualTree(child, action);
                }
            }
            else if (parent is ContentControl contentControl && contentControl.Content is DependencyObject content1)
            {
                TraverseVisualTree(content1, action);
            }
            else if (parent is ContentPresenter contentPresenter && contentPresenter.Content is DependencyObject content2)
            {
                TraverseVisualTree(content2, action);
            }
            else if (parent is Decorator decorator && decorator.Child is DependencyObject child)
            {
                TraverseVisualTree(child, action);
            }
            else if (parent is Popup popup)
            {
                TraverseVisualTree(popup.Child, action);
            }
            else if (parent is ItemsControl itemsControl)
            {
                foreach (var item in itemsControl.Items)
                {
                    if (item is DependencyObject itemDependencyObject)
                    {
                        TraverseVisualTree(itemDependencyObject, action);
                    }
                }
            }
        }

        // 获取所有焦点作用域
        private static List<DependencyObject> GetFocusScopes(DependencyObject container)
        {
            var scopes = new List<DependencyObject>();
            TraverseVisualTree(container, (element) =>
            {
                if (FocusManager.GetIsFocusScope(element))
                {
                    scopes.Add(element);
                }
            });
            return scopes;
        }

        // 检查元素是否在视觉树中具有焦点（包括子元素）
        public static bool HasFocusInVisualTree(DependencyObject element)
        {
            return element is UIElement uiElement &&
                   (uiElement.IsKeyboardFocused || uiElement.IsFocused);
        }
    }
}
