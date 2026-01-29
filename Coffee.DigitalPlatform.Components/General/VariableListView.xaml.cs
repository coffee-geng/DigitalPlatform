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

namespace Coffee.DigitalPlatform.Components
{
    /// <summary>
    /// VariableListView.xaml 的交互逻辑
    /// </summary>
    public partial class VariableListView : UserControl
    {
        public VariableListView()
        {
            InitializeComponent();
        }

        public bool IsShowingPopup
        {
            get { return (bool)GetValue(IsShowingPopupProperty); }
            set { SetValue(IsShowingPopupProperty, value); }
        }
        public static readonly DependencyProperty IsShowingPopupProperty =
            DependencyProperty.Register("IsShowingPopup", typeof(bool), typeof(VariableListView), new PropertyMetadata(false, OnIsShowingPopupChanged));
        private static void OnIsShowingPopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VariableListView control = d as VariableListView;
            if (control != null)
            {
                bool newValue = (bool)e.NewValue;
                if (!newValue)
                {
                    control.btnInfo.IsChecked = false;
                }
            }
        }

        public void ResetToggleButtonState(bool state)
        {
            this.btnInfo.IsChecked = state;
        }

        private void btnInfo_Checked(object sender, RoutedEventArgs e)
        {
            this.IsShowingPopup = true;
            if (this.IsShowingPopupChanged != null)
            {
                this.IsShowingPopupChanged(this, new RoutedPropertyChangedEventArgs<bool>(false, true));
            }
        }

        private void btnInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            this.IsShowingPopup = false;
        }

        public event RoutedPropertyChangedEventHandler<bool> IsShowingPopupChanged;
    }
}
