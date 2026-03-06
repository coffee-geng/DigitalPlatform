using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Globalization;
using System.Windows.Media;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using Microsoft.Extensions.Logging.Abstractions;
using static System.Net.Mime.MediaTypeNames;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Coffee.DigitalPlatform.Controls
{
    public static class ComboBoxWidthBehavior
    {
        #region AutoWidth
        public static readonly DependencyProperty AutoWidthProperty = DependencyProperty.RegisterAttached(
            "AutoWidth", typeof(bool), typeof(ComboBoxWidthBehavior), new PropertyMetadata(false, OnAutoWidthChanged));

        public static void SetAutoWidth(ComboBox element, bool value)
        {
            element.SetValue(AutoWidthProperty, value);
        }

        public static bool GetAutoWidth(ComboBox element)
        {
            return (bool)element.GetValue(AutoWidthProperty);
        }

        private static void OnAutoWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
                if ((bool)e.NewValue)
                {
                    var eventManager = new ComboBoxEventManager(comboBox, UpdateComboBoxWidth);
                    comboBox.SetValue(ComboBoxEventManagerProperty, eventManager);
                }
                else
                {
                    if (comboBox.GetValue(ComboBoxEventManagerProperty) is ComboBoxEventManager eventManager && eventManager != null)
                    {
                        eventManager.Detach();
                    }
                }
            }
        }
        #endregion

        #region ComboBoxEventManager
        public static ComboBoxEventManager GetComboBoxEventManager(DependencyObject obj)
        {
            return (ComboBoxEventManager)obj.GetValue(ComboBoxEventManagerProperty);
        }

        public static void SetComboBoxEventManager(DependencyObject obj, ComboBoxEventManager value)
        {
            obj.SetValue(ComboBoxEventManagerProperty, value);
        }

        public static readonly DependencyProperty ComboBoxEventManagerProperty =
            DependencyProperty.RegisterAttached("ComboBoxEventManager", typeof(ComboBoxEventManager), typeof(ComboBoxWidthBehavior), new PropertyMetadata(null));
        #endregion

        private static void UpdateComboBoxWidth(ComboBox comboBox)
        {
            if (comboBox == null) return;

            double maxWidth = 0;
            var typeface = new Typeface(comboBox.FontFamily, comboBox.FontStyle,
            comboBox.FontWeight, comboBox.FontStretch);

            if (!string.IsNullOrEmpty(comboBox.Text))
            {
                var formattedText = new FormattedText(comboBox.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        typeface, comboBox.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(comboBox).PixelsPerDip);
                maxWidth = formattedText.Width;
            }

            foreach (var item in comboBox.Items)
            {
                if (item != null)
                {
                    double itemWidth = GetItemWidth(item, comboBox, typeface);
                    maxWidth = Math.Max(maxWidth, itemWidth);
                }
            }

            // 添加内边距和下拉按钮的宽度
            maxWidth = maxWidth + 30;
            maxWidth = Math.Max(maxWidth, comboBox.MinWidth);
            maxWidth = Math.Min(maxWidth, comboBox.MaxWidth);
            comboBox.Width = maxWidth;
        }

        private static double GetItemWidth(object item, ComboBox comboBox, Typeface typeface)
        {
            if (comboBox.ItemTemplate != null) // 如果使用模板，需要更复杂的处理
            {
                var contentPresenter = new ContentPresenter
                {
                    Content = item,
                    ContentTemplate = comboBox.ItemTemplate
                };
                contentPresenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                return contentPresenter.ActualWidth;
            }
            else //默认文本模板
            {
                if (item != null && !string.IsNullOrEmpty(item.ToString()))
                {
                    var formattedText = new FormattedText(item.ToString(),
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            typeface,
                            comboBox.FontSize,
                            Brushes.Black,
                            VisualTreeHelper.GetDpi(comboBox).PixelsPerDip);
                    return formattedText.Width;
                }
                else
                {
                    return 0;
                }
            }
        }
    }

    public class ComboBoxEventManager
    {
        private readonly WeakReference<ComboBox> _comboBoxRef;
        private readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;
        private readonly ItemsChangedEventHandler _itemsChangedHandler;
        private readonly RoutedEventHandler _loadedHandler;

        private readonly Action<ComboBox> _updateWidthHandler;

        public ComboBoxEventManager(ComboBox comboBox, Action<ComboBox> updateWidth)
        {
            _comboBoxRef = new WeakReference<ComboBox>(comboBox);

            _loadedHandler = OnLoaded;
            _collectionChangedHandler = OnCollectionChanged;
            _itemsChangedHandler = OnItemsChanged;

            _updateWidthHandler = updateWidth;

            // 订阅事件
            comboBox.Loaded += _loadedHandler;

            if (comboBox.Items is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += _collectionChangedHandler;
            }

            comboBox.ItemContainerGenerator.ItemsChanged += _itemsChangedHandler;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox && _updateWidthHandler != null)
            {
                _updateWidthHandler(comboBox);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_comboBoxRef.TryGetTarget(out ComboBox comboBox) && comboBox != null && _updateWidthHandler != null)
            {
                _updateWidthHandler(comboBox);
            }
        }

        private void OnItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            if (_comboBoxRef.TryGetTarget(out ComboBox comboBox) && comboBox != null && _updateWidthHandler != null)
            {
                _updateWidthHandler(comboBox);
            }
        }

        public void Detach()
        {
            if (_comboBoxRef.TryGetTarget(out ComboBox comboBox) && comboBox != null)
            {
                comboBox.Loaded -= _loadedHandler;

                if (comboBox.Items is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged -= _collectionChangedHandler;
                }

                comboBox.ItemContainerGenerator.ItemsChanged -= _itemsChangedHandler;
            }
        }
    }
}
