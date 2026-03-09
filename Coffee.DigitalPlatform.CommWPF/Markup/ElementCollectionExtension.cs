using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xaml;

namespace Coffee.DigitalPlatform.CommWPF
{
    [MarkupExtensionReturnType(typeof(IList<UIElement>))]
    [ContentProperty("Items")]
    public class ElementCollectionExtension : MarkupExtension
    {
        public ElementCollectionExtension()
        {
            Items = new ElementLookupCollection();
        }

        //Items属性作为ElementCollectionExtension这个XAML对象的Content
        [ConstructorArgument("items")]
        public ElementLookupCollection Items { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var result = new List<UIElement>();

            // 获取根对象
            var rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            if (rootObjectProvider?.RootObject is FrameworkElement rootElement)
            {
                foreach (var item in Items)
                {
                    try
                    {
                        UIElement resultItem = null;
                        // 1. 先通过名称查找
                        if (!string.IsNullOrEmpty(item.ByName))
                        {
                            resultItem = FindElementByName(rootElement, item.ByName);
                        }
                        // 2. 找不到，则再通过相对源查找
                        if (resultItem == null && item.ByRelativeSource != null)
                        {
                            //从当前元素位置开始网上查找
                            var currentElement = provideValueTarget?.TargetObject as DependencyObject;
                            resultItem = FindElementByRelativeSource(currentElement, item.ByRelativeSource);
                        }

                        if (resultItem != null)
                        {
                            result.Add(resultItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"查找元素失败: {ex.Message}");
                    }
                }
            }
            return result;
        }

        private UIElement FindElementByName(FrameworkElement root, string name)
        {
            var element = root.FindName(name) as UIElement;
            if (element != null)
                return element;

            return FindChildByName(root, name);
        }

        /// <summary>
        /// 递归查找根元素下的所有子元素
        /// </summary>
        /// <param name="parent">根元素</param>
        /// <param name="name">匹配节点名</param>
        /// <returns>返回匹配的元素</returns>
        private UIElement FindChildByName(DependencyObject parent, string name)
        {
            if (parent == null || string.IsNullOrEmpty(name))
                return null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is FrameworkElement fe && fe.Name == name)
                    return fe;

                var result = FindChildByName(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private UIElement FindElementByRelativeSource(DependencyObject target, RelativeSource relativeSource)
        {
            if (target == null || relativeSource == null)
                return null;

            switch (relativeSource.Mode)
            {
                case RelativeSourceMode.FindAncestor:
                    return FindAncestor(target, relativeSource.AncestorType, relativeSource.AncestorLevel);
                case RelativeSourceMode.Self:
                    return target as UIElement;
                case RelativeSourceMode.TemplatedParent:
                    if (target is FrameworkElement fe)
                    {
                        return fe.TemplatedParent as UIElement;
                    }
                    break;
                case RelativeSourceMode.PreviousData:
                    // 在数据上下文中查找
                    break;
            }
            return null;
        }

        /// <summary>
        /// 根据匹配的元素类型和匹配次数查找符合匹配条件的元素
        /// </summary>
        /// <param name="target">从当前元素位置开始向上追溯</param>
        /// <param name="ancestorType">匹配的元素类型</param>
        /// <param name="level">向上查找到第几个匹配的元素</param>
        /// <returns>返回匹配的元素</returns>
        private UIElement FindAncestor(DependencyObject target, Type ancestorType, int level = 1)
        {
            if (target == null)
                return null;
            if (ancestorType == null)
                return null;

            var current = target;
            int foundLevel = 0;

            while (current != null)
            {
                if (ancestorType.IsAssignableFrom(current.GetType()))
                {
                    foundLevel++;
                    if (foundLevel >= level)
                        return current as UIElement;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }

    public class ElementLookupCollection : ObservableCollection<ElementLookup>
    {

    }

    public class ElementLookup
    {
        /// <summary>
        /// 通过名称查找
        /// </summary>
        public string ByName { get; set; }

        /// <summary>
        /// 通过相对源查找
        /// </summary>
        public RelativeSource ByRelativeSource { get; set; }
    }
}
