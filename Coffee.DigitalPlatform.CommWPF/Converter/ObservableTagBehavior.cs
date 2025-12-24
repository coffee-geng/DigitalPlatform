using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class ObservableTagBehavior : Behavior<FrameworkElement>
    {
        public object ObservableTag
        {
            get { return (object)GetValue(ObservableTagProperty); }
            set { SetValue(ObservableTagProperty, value); }
        }

        public static readonly DependencyProperty ObservableTagProperty =
            DependencyProperty.Register("ObservableTag", typeof(object), typeof(ObservableTagBehavior), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnObservableTagChanged));
        private static void OnObservableTagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (ObservableTagBehavior)d;
            if (behavior.AssociatedObject != null)
            {
                behavior.AssociatedObject.Tag = e.NewValue;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            // 初始化时同步Tag值
            if (AssociatedObject != null)
            {
                AssociatedObject.Tag = ObservableTag;
            }
        }
    }

    public static class ObservableTagPropertyExtension
    {
        public static readonly DependencyProperty ObservableTagProperty =
            DependencyProperty.RegisterAttached(
                "ObservableTag",
                typeof(object),
                typeof(ObservableTagPropertyExtension),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnObservableTagChanged));

        public static object GetObservableTag(DependencyObject obj)
        {
            return obj.GetValue(ObservableTagProperty);
        }

        public static void SetObservableTag(DependencyObject obj, object value)
        {
            obj.SetValue(ObservableTagProperty, value);

            // 同时更新原始的 Tag 属性（可选）
            if (obj is FrameworkElement element)
            {
                element.Tag = value;
            }
        }

        private static void OnObservableTagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 属性变化时的处理逻辑
            if (d is FrameworkElement element)
            {
                element.Tag = e.NewValue; // 同步到 Tag
            }
        }
    }
}
