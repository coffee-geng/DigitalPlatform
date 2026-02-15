using Coffee.DigitalPlatform.Controls.FilterBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Coffee.DigitalPlatform.Controls
{
    public class NumericUpDown : ContentControl
    {
        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new System.Windows.FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        public NumericUpDown()
        {
            Loaded += NumericUpDown_Loaded;
            updateContent();
        }

        private void NumericUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            updateContent();
        }

        #region Value
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(NumericUpDown), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)d;
            if (e.NewValue == null)
            {
                control.ValueControlType = ValueControlType.Double;
            }
            else
            {
                if (e.NewValue is byte)
                    control.ValueControlType = ValueControlType.Byte;
                else if (e.NewValue is short)
                    control.ValueControlType = ValueControlType.Short;
                else if (e.NewValue is ushort)
                    control.ValueControlType = ValueControlType.UnsignedShort;
                else if (e.NewValue is int)
                    control.ValueControlType = ValueControlType.Integer;
                else if (e.NewValue is uint)
                    control.ValueControlType = ValueControlType.UnsignedShort;
                else if (e.NewValue is long)
                    control.ValueControlType = ValueControlType.Long;
                else if (e.NewValue is ulong)
                    control.ValueControlType = ValueControlType.UnsignedLong;
                else if (e.NewValue is float)
                    control.ValueControlType = ValueControlType.Float;
                else if (e.NewValue is double)
                    control.ValueControlType = ValueControlType.Double;
                else if (e.NewValue is decimal)
                    control.ValueControlType = ValueControlType.Decimal;
                else
                {
                    control.ValueControlType = ValueControlType.Double;
                }
            }
            control.updateContent();
            control.updateValue();
        }
        #endregion

        #region ValueControlType
        public ValueControlType ValueControlType
        {
            get { return (ValueControlType)GetValue(ValueControlTypeProperty); }
            set { SetValue(ValueControlTypeProperty, value); }
        }

        public static readonly DependencyProperty ValueControlTypeProperty =
            DependencyProperty.Register("ValueControlType", typeof(ValueControlType), typeof(NumericUpDown), new System.Windows.PropertyMetadata(ValueControlType.Double, OnValueControlTypeChanged));

        private static void OnValueControlTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)d;
            control.updateContent();
        }
        #endregion

        #region Increment
        public object Increment
        {
            get => GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }

        public static readonly DependencyProperty IncrementProperty =
        DependencyProperty.Register("Increment", typeof(object), typeof(NumericUpDown), new System.Windows.PropertyMetadata(1.0, onIncrementPropertyChanged));

        private static void onIncrementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown instance = (NumericUpDown)d;
            instance.updateIncrement();
        }
        #endregion

        #region Minimum
        public object Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register("Minimum", typeof(object), typeof(NumericUpDown), new System.Windows.PropertyMetadata(0.0, onMinimumPropertyChanged));

        private static void onMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown instance = (NumericUpDown)d;
            instance.updateMinimum();
        }
        #endregion

        #region Maximum
        public object Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register("Maximum", typeof(object), typeof(NumericUpDown), new System.Windows.PropertyMetadata(100.0, onMaximumPropertyChanged));

        private static void onMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown instance = (NumericUpDown)d;
            instance.updateMaximum();
        }
        #endregion

        private void updateContent()
        {
            Type? controlType = Content != null ? Content.GetType() : null;

            if (ValueControlType == ValueControlType.Short && controlType != typeof(ShortUpDown))
            {
                var shortUpDown = createShortUpDown();
                Content = shortUpDown;
            }
            else if (ValueControlType == ValueControlType.Integer && controlType != typeof(IntegerUpDown))
            {
                var intUpDown = createIntegerUpDown();
                Content = intUpDown;
            }
            else if (ValueControlType == ValueControlType.Double & controlType != typeof(DoubleUpDown))
            {
                var doubleUpDown = createDoubleUpDown();
                Content = doubleUpDown;
            }
            else if (ValueControlType == ValueControlType.Decimal & controlType != typeof(DecimalUpDown))
            {
                var decimalUpDown = createDecimalUpDown();
                Content = decimalUpDown;
            }
        }

        private ShortUpDown createShortUpDown()
        {
            if (this.Content != null && this.Content is ShortUpDown shortUpDown)
            {
                return shortUpDown;
            }
            shortUpDown = new ShortUpDown
            {
                Value = Value != null ? (short?)Convert.ToInt16(Value) : null,
                Increment = Increment != null ? (short?)Convert.ToInt16(Increment) : null,
                Minimum = Minimum != null ? (short?)Convert.ToInt16(Minimum) : null,
                Maximum = Maximum != null ? (short?)Convert.ToInt16(Maximum) : null,
            };
            DependencyPropertyDescriptor.FromProperty(ShortUpDown.ValueProperty, typeof(ShortUpDown))
                .AddValueChanged(shortUpDown, (s, e) =>
                {
                    this.Value = (s as ShortUpDown).Value;
                });
            DependencyPropertyDescriptor.FromProperty(ShortUpDown.IncrementProperty, typeof(ShortUpDown))
                .AddValueChanged(shortUpDown, (s, e) =>
                {
                    this.Increment = (s as ShortUpDown).Increment;
                });
            DependencyPropertyDescriptor.FromProperty(ShortUpDown.MinimumProperty, typeof(ShortUpDown))
                .AddValueChanged(shortUpDown, (s, e) =>
                {
                    this.Minimum = (s as ShortUpDown).Minimum;
                });
            DependencyPropertyDescriptor.FromProperty(ShortUpDown.MaximumProperty, typeof(ShortUpDown))
                .AddValueChanged(shortUpDown, (s, e) =>
                {
                    this.Maximum = (s as ShortUpDown).Maximum;
                });
            return shortUpDown;
        }

        private IntegerUpDown createIntegerUpDown()
        {
            if (this.Content != null && this.Content is IntegerUpDown intUpDown)
            {
                return intUpDown;
            }
            intUpDown = new IntegerUpDown
            {
                Value = Value != null ? (int?)Convert.ToInt32(Value) : null,
                Increment = Increment != null ? (int?)Convert.ToInt32(Increment) : null,
                Minimum = Minimum != null ? (int?)Convert.ToInt32(Minimum) : null,
                Maximum = Maximum != null ? (int?)Convert.ToInt32(Maximum) : null,
            };
            DependencyPropertyDescriptor.FromProperty(IntegerUpDown.ValueProperty, typeof(IntegerUpDown))
                .AddValueChanged(intUpDown, (s, e) =>
                {
                    this.Value = (s as IntegerUpDown).Value;
                });
            DependencyPropertyDescriptor.FromProperty(IntegerUpDown.IncrementProperty, typeof(IntegerUpDown))
                .AddValueChanged(intUpDown, (s, e) =>
                {
                    this.Increment = (s as IntegerUpDown).Increment;
                });
            DependencyPropertyDescriptor.FromProperty(IntegerUpDown.MinimumProperty, typeof(IntegerUpDown))
                .AddValueChanged(intUpDown, (s, e) =>
                {
                    this.Minimum = (s as IntegerUpDown).Minimum;
                });
            DependencyPropertyDescriptor.FromProperty(IntegerUpDown.MaximumProperty, typeof(IntegerUpDown))
                .AddValueChanged(intUpDown, (s, e) =>
                {
                    this.Maximum = (s as IntegerUpDown).Maximum;
                });
            return intUpDown;
        }

        private DoubleUpDown createDoubleUpDown()
        {
            if (this.Content != null && this.Content is DoubleUpDown doubleUpDown)
            {
                return doubleUpDown;
            }
            doubleUpDown = new DoubleUpDown
            {
                Value = Value != null ? (double?)Convert.ToDouble(Value) : null,
                Increment = Increment != null ? (int?)Convert.ToDouble(Increment) : null,
                Minimum = Minimum != null ? (int?)Convert.ToDouble(Minimum) : null,
                Maximum = Maximum != null ? (int?)Convert.ToDouble(Maximum) : null,
            };
            DependencyPropertyDescriptor.FromProperty(DoubleUpDown.ValueProperty, typeof(DoubleUpDown))
                .AddValueChanged(doubleUpDown, (s, e) =>
                {
                    this.Value = (s as DoubleUpDown).Value;
                });
            DependencyPropertyDescriptor.FromProperty(DoubleUpDown.IncrementProperty, typeof(DoubleUpDown))
                .AddValueChanged(doubleUpDown, (s, e) =>
                {
                    this.Increment = (s as DoubleUpDown).Increment;
                });
            DependencyPropertyDescriptor.FromProperty(DoubleUpDown.MinimumProperty, typeof(DoubleUpDown))
                .AddValueChanged(doubleUpDown, (s, e) =>
                {
                    this.Minimum = (s as DoubleUpDown).Minimum;
                });
            DependencyPropertyDescriptor.FromProperty(DoubleUpDown.MaximumProperty, typeof(DoubleUpDown))
                .AddValueChanged(doubleUpDown, (s, e) =>
                {
                    this.Maximum = (s as DoubleUpDown).Maximum;
                });
            return doubleUpDown;
        }

        private DecimalUpDown createDecimalUpDown()
        {
            if (this.Content != null && this.Content is DecimalUpDown decimalUpDown)
            {
                return decimalUpDown;
            }
            decimalUpDown = new DecimalUpDown
            {
                Value = Value != null ? (decimal?)Convert.ToDecimal(Value) : null,
                Increment = Increment != null ? (decimal?)Convert.ToDecimal(Increment) : null,
                Minimum = Minimum != null ? (decimal?)Convert.ToDecimal(Minimum) : null,
                Maximum = Maximum != null ? (decimal?)Convert.ToDecimal(Maximum) : null,
            };
            DependencyPropertyDescriptor.FromProperty(DecimalUpDown.ValueProperty, typeof(DecimalUpDown))
                .AddValueChanged(decimalUpDown, (s, e) =>
                {
                    this.Value = (s as DecimalUpDown).Value;
                });
            DependencyPropertyDescriptor.FromProperty(DecimalUpDown.IncrementProperty, typeof(DecimalUpDown))
                .AddValueChanged(decimalUpDown, (s, e) =>
                {
                    this.Increment = (s as DecimalUpDown).Increment;
                });
            DependencyPropertyDescriptor.FromProperty(DecimalUpDown.MinimumProperty, typeof(DecimalUpDown))
                .AddValueChanged(decimalUpDown, (s, e) =>
                {
                    this.Minimum = (s as DecimalUpDown).Minimum;
                });
            DependencyPropertyDescriptor.FromProperty(DecimalUpDown.MaximumProperty, typeof(DecimalUpDown))
                .AddValueChanged(decimalUpDown, (s, e) =>
                {
                    this.Maximum = (s as DecimalUpDown).Maximum;
                });
            return decimalUpDown;
        }

        private void updateValue()
        {
            if (Content == null)
                return;
            try
            {
                if (Content is ShortUpDown shortUpDown)
                {
                    shortUpDown.Value = Value != null ? (short?)Convert.ToInt16(Value) : null;
                }
                else if (Content is IntegerUpDown intUpDown)
                {
                    intUpDown.Value = Value != null ? (int?)Convert.ToInt32(Value) : null;
                }
                else if (Content is DoubleUpDown doubleUpDown)
                {
                    doubleUpDown.Value = Value != null ? (double?)Convert.ToDouble(Value) : null;
                }
                else if (Content is DecimalUpDown decimalUpDown)
                {
                    decimalUpDown.Value = Value != null ? (decimal?)Convert.ToDecimal(Value) : null;
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void updateIncrement()
        {
            if (Content == null)
                return;
            if (Content is ShortUpDown shortUpDown)
            {
                shortUpDown.Increment = Increment != null ? (short?)Convert.ToInt16(Increment) : null;
            }
            else if (Content is IntegerUpDown intUpDown)
            {
                intUpDown.Increment = Increment != null ? (int?)Convert.ToInt32(Increment) : null;
            }
            else if (Content is DoubleUpDown doubleUpDown)
            {
                doubleUpDown.Increment = Increment != null ? (double?)Convert.ToDouble(Increment) : null;
            }
            else if (Content is DecimalUpDown decimalUpDown)
            {
                decimalUpDown.Increment = Increment != null ? (decimal?)Convert.ToDecimal(Increment) : null;
            }
        }

        private void updateMinimum()
        {
            if (Content == null)
                return;
            if (Content is ShortUpDown shortUpDown)
            {
                shortUpDown.Minimum = Minimum != null ? (short?)Convert.ToInt16(Minimum) : null;
            }
            else if (Content is IntegerUpDown intUpDown)
            {
                intUpDown.Minimum = Minimum != null ? (int?)Convert.ToInt32(Minimum) : null;
            }
            else if (Content is DoubleUpDown doubleUpDown)
            {
                doubleUpDown.Minimum = Minimum != null ? (double?)Convert.ToDouble(Minimum) : null;
            }
            else if (Content is DecimalUpDown decimalUpDown)
            {
                decimalUpDown.Minimum = Minimum != null ? (decimal?)Convert.ToDecimal(Minimum) : null;
            }
        }

        private void updateMaximum()
        {
            if (Content == null)
                return;
            if (Content is ShortUpDown shortUpDown)
            {
                shortUpDown.Maximum = Maximum != null ? (short?)Convert.ToInt16(Maximum) : null;
            }
            else if (Content is IntegerUpDown intUpDown)
            {
                intUpDown.Maximum = Maximum != null ? (int?)Convert.ToInt32(Maximum) : null;
            }
            else if (Content is DoubleUpDown doubleUpDown)
            {
                doubleUpDown.Maximum = Maximum != null ? (double?)Convert.ToDouble(Maximum) : null;
            }
            else if (Content is DecimalUpDown decimalUpDown)
            {
                decimalUpDown.Maximum = Maximum != null ? (decimal?)Convert.ToDecimal(Maximum) : null;
            }
        }
    }
}
