using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace Coffee.DigitalPlatform.Views
{
    /// <summary>
    /// ConfigureComponentDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigureComponentDialog : Window
    {
        public ConfigureComponentDialog()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<RepaintAuxiliaryMessage>(this, receivePaintAuxiliaryMessage);
        }
        
        BooleanToVisibilityConverter _boolToVisibilityConverter { get; set; } = new BooleanToVisibilityConverter();

        private void ItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            ItemsControl control = sender as ItemsControl;
            if (control == null)
                return;
            Panel panel = ItemsControlExtensions.GetItemsPanel(control);
            ItemsControlExtensions.SetLayoutContainer(control, panel);
        }

        private void receivePaintAuxiliaryMessage(object sender, RepaintAuxiliaryMessage message)
        {
            if (message == null || message.Value == null) 
                return;
            var auxiliaryInfo = message.Value;
            if (auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.VerticalLine || auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.HorizontalLine)
            {
                IAuxiliaryLineContext lineContext = auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.VerticalLine ? findVerticalLine() : findHorizontalLine();
                if (auxiliaryInfo.IsVisible)
                {
                    if (lineContext != null)
                    {
                        if (auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.VerticalLine)
                        {
                            lineContext.X = auxiliaryInfo.X;
                        }
                        else
                        {
                            lineContext.Y = auxiliaryInfo.Y;
                        }
                        lineContext.IsVisible = true;
                    }
                    else
                    {
                        Line line = createAuxiliarylLine(auxiliaryInfo);
                        if (!auxiliaryLayer.Children.Contains(line))
                        {
                            auxiliaryLayer.Children.Add(line);
                            if (!_auxiliaryCacheDict.ContainsKey(line.DataContext as IAuxiliaryLineContext))
                            {
                                _auxiliaryCacheDict.Add(line.DataContext as IAuxiliaryLineContext, line);
                            }
                            else
                            {
                                _auxiliaryCacheDict[line.DataContext as IAuxiliaryLineContext] = line;
                            }
                        }
                    }
                }
                else
                {
                    if (lineContext != null)
                    {
                        lineContext.IsVisible = false;
                    }
                }
            }
        }

        private Dictionary<IAuxiliaryLineContext, FrameworkElement> _auxiliaryCacheDict = new Dictionary<IAuxiliaryLineContext, FrameworkElement>();

        private Line createAuxiliarylLine(AuxiliaryInfo auxiliaryInfo)
        {
            if (!(auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.VerticalLine || auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.HorizontalLine))
            {
                throw new ArgumentException($"Only support VerticalLine or HorizontalLine to paint auxiliary line.");
            }
            IAuxiliaryLineContext lineContext = new AuxiliaryLine()
            {
                IsVisible = true,
                X = auxiliaryInfo.X,
                Y = auxiliaryInfo.Y,
                Z = auxiliaryInfo.Z,
                Width = auxiliaryInfo.Width,
                Height = auxiliaryInfo.Height
            };
            if (auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.VerticalLine)
            {
                lineContext.AuxiliaryType = AuxiliaryLineTypes.VerticalLine;
            }
            else if (auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.HorizontalLine)
            {
                lineContext.AuxiliaryType |= AuxiliaryLineTypes.HorizontalLine;
            }
            Line auxLine = new Line
            {
                DataContext = lineContext,
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 3.0, 3.0 },
                ClipToBounds = true,
            };

            Binding binding = new Binding("IsVisible");
            binding.Converter = _boolToVisibilityConverter;
            auxLine.SetBinding(Line.VisibilityProperty, binding);

            if (auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.VerticalLine)
            {
                auxLine.Y1 = 0;
                auxLine.Y2 = 2000;
                binding = new Binding("X");
                auxLine.SetBinding(Line.X1Property, binding);
                binding = new Binding("X");
                auxLine.SetBinding(Line.X2Property, binding);
            }
            else if (auxiliaryInfo.AuxiliaryType == AuxiliaryLineTypes.HorizontalLine)
            {
                auxLine.X1 = 0;
                auxLine.X2 = 2000;
                binding = new Binding("Y");
                auxLine.SetBinding(Line.Y1Property, binding);
                binding = new Binding("Y");
                auxLine.SetBinding(Line.Y2Property, binding);
            }
            return auxLine;
        }

        //在画布中查找可用的水平对齐线
        private IAuxiliaryLineContext? findHorizontalLine()
        {
            return _auxiliaryCacheDict.Where(p => p.Key.AuxiliaryType == AuxiliaryLineTypes.HorizontalLine && p.Value.DataContext is IAuxiliaryLineContext).Select(p => p.Value.DataContext).Cast<IAuxiliaryLineContext>().FirstOrDefault();
        }

        private IAuxiliaryLineContext? findVerticalLine()
        {
            return _auxiliaryCacheDict.Where(p => p.Key.AuxiliaryType == AuxiliaryLineTypes.VerticalLine && p.Value.DataContext is IAuxiliaryLineContext).Select(p => p.Value.DataContext).Cast<IAuxiliaryLineContext>().FirstOrDefault();
        }

        private IAuxiliaryLineContext? findHorizontalRuler()
        {
            return _auxiliaryCacheDict.Where(p => p.Key.AuxiliaryType == AuxiliaryLineTypes.HorizontalRuler && p.Value.DataContext is IAuxiliaryLineContext).Select(p => p.Value.DataContext).Cast<IAuxiliaryLineContext>().FirstOrDefault();
        }

        private IAuxiliaryLineContext? findVerticalRuler()
        {
            return _auxiliaryCacheDict.Where(p => p.Key.AuxiliaryType == AuxiliaryLineTypes.VerticalRuler && p.Value.DataContext is IAuxiliaryLineContext).Select(p => p.Value.DataContext).Cast<IAuxiliaryLineContext>().FirstOrDefault();
        }
    }
}
