using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Ink;

namespace Coffee.DigitalPlatform.CommWPF
{
    // 扩展方法版本
    public static class ItemsControlExtensions
    {
        public static Panel GetLayoutContainer(DependencyObject obj)
        {
            return (Panel)obj.GetValue(LayoutContainerProperty);
        }
        public static void SetLayoutContainer(DependencyObject obj, Panel value)
        {
            obj.SetValue(LayoutContainerProperty, value);
        }
        public static readonly DependencyProperty LayoutContainerProperty =
            DependencyProperty.RegisterAttached("LayoutContainer", typeof(Panel), typeof(ItemsControlExtensions), new PropertyMetadata(null));

        public static Panel GetItemsPanel(this ItemsControl itemsControl)
        {
            if (itemsControl == null) return null;

            var itemsPresenter = FindVisualChild<ItemsPresenter>(itemsControl);
            if (itemsPresenter == null) return null;

            if (VisualTreeHelper.GetChildrenCount(itemsPresenter) > 0)
            {
                return VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
            }

            return null;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }

            return null;
        }
    }
}
