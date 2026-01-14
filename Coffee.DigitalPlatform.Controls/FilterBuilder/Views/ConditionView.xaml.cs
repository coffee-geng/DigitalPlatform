using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    /// <summary>
    /// ConditionView.xaml 的交互逻辑
    /// </summary>
    public partial class ConditionView : UserControl
    {
        public ConditionView()
        {
            InitializeComponent();

            this.DataContextChanged += ConditionView_DataContextChanged;
        }

        private void ConditionView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is ConditionViewModel vm)
            {
                CurrentFilterScheme = vm.FilterScheme;
            }
        }

        public string PreviewConditionText
        {
            get { return (string)GetValue(PreviewConditionTextProperty); }
            set { SetValue(PreviewConditionTextProperty, value); }
        }

        public static readonly DependencyProperty PreviewConditionTextProperty =
            DependencyProperty.Register("PreviewConditionText", typeof(string), typeof(ConditionView), new UIPropertyMetadata(null));


        public FilterScheme CurrentFilterScheme
        {
            get { return (FilterScheme)GetValue(CurrentFilterSchemeProperty); }
            set { SetValue(CurrentFilterSchemeProperty, value); }
        }

        public static readonly DependencyProperty CurrentFilterSchemeProperty =
            DependencyProperty.Register("CurrentFilterScheme", typeof(FilterScheme), typeof(ConditionView), new UIPropertyMetadata(null, onCurrentFilterSchemePropertyChanged));

        private static void onCurrentFilterSchemePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var instance = sender as ConditionView;
            Task.Delay(500).ContinueWith(t =>
            {
                instance.RaiseEvent(new RoutedPropertyChangedEventArgs<FilterScheme>((FilterScheme)e.OldValue, (FilterScheme)e.NewValue, CurrentFilterSchemeChangedEvent));
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static readonly RoutedEvent CurrentFilterSchemeChangedEvent = EventManager.RegisterRoutedEvent(" CurrentFilterSchemeChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<FilterScheme>), typeof(ConditionView));

        public event RoutedPropertyChangedEventHandler<FilterScheme> CurrentFilterSchemeChanged
        {
            add { AddHandler(CurrentFilterSchemeChangedEvent, value); }
            remove { RemoveHandler(CurrentFilterSchemeChangedEvent, value); }
        }
    }
}
