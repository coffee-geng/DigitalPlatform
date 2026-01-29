using Coffee.DigitalPlatform.CommWPF;
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
using System.Windows.Shell;

namespace Coffee.DigitalPlatform.Components
{
    /// <summary>
    /// AirCompressor.xaml 的交互逻辑
    /// </summary>
    public partial class AirCompressor : ComponentBase
    {
        public AirCompressor()
        {
            InitializeComponent();

            this.anchor.OnResizeStart += Anchor_OnResizeStart;
            this.anchor.OnResizing += Anchor_OnResizing;
            this.anchor.OnResizeEnd += Anchor_OnResizeEnd;
        }

        private void Anchor_OnResizeStart()
        {
            doResizeStart();
        }

        private void Anchor_OnResizing(Vector delta, ResizeGripDirection resizeDirection, bool isAlign, bool isProportional)
        {
            doResizing(delta, resizeDirection, isAlign, isProportional);
        }

        private void Anchor_OnResizeEnd(Vector delta, ResizeGripDirection resizeDirection, bool isAlign, bool isProportional)
        {
            doResizeEnd(delta, resizeDirection, isAlign, isProportional);
        }

        private void variableListView_IsShowingPopupChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (e.NewValue == true)
            {
                manualListView.ResetToggleButtonState(false);
            }
        }

        private void manualListView_IsShowingPopupChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (e.NewValue == true)
            {
                variableListView.ResetToggleButtonState(false);
            }
        }
    }
}
