using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Coffee.DigitalPlatform.Views
{
    public class NumericTextBoxExtension
    {
        public static readonly DependencyProperty IsNumericOnlyProperty = DependencyProperty.RegisterAttached("IsNumericOnly", typeof(bool), typeof(NumericTextBoxExtension), new PropertyMetadata(false, OnIsNumericOnlyChanged));

        public static void SetIsNumericOnly(TextBox element, bool value)
        {
            element.SetValue(IsNumericOnlyProperty, value);
        }

        public static bool GetIsNumericOnly(TextBox element)
        {
            return (bool)element.GetValue(IsNumericOnlyProperty);
        }

        private static void OnIsNumericOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as TextBox;
            if (textBox == null) return;

            if ((bool)e.NewValue)
            {
                // 禁用输入法（这会阻止中文输入法的显示）
                InputMethod.SetIsInputMethodEnabled(textBox, false);
                textBox.PreviewTextInput += OnPreviewTextInput;
                DataObject.AddPastingHandler(textBox, OnPaste);
            }
            else
            {
                InputMethod.SetIsInputMethodEnabled(textBox, true);
                textBox.PreviewTextInput -= OnPreviewTextInput;
                DataObject.RemovePastingHandler(textBox, OnPaste);
            }
        }

        public static bool GetIsDouble(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDoubleProperty);
        }

        public static void SetIsDouble(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDoubleProperty, value);
        }
        //是否支持小数
        public static readonly DependencyProperty IsDoubleProperty =
            DependencyProperty.RegisterAttached("IsDouble", typeof(bool), typeof(NumericTextBoxExtension), new PropertyMetadata(false));

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            bool isDouble = GetIsDouble(sender as TextBox);
            e.Handled = !IsTextNumeric(e.Text, isDouble);
        }

        private static void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                bool isDouble = GetIsDouble(sender as TextBox);
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextNumeric(text, isDouble))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static bool IsTextNumeric(string text, bool isDouble = true)
        {
            string pattern = isDouble ? @"^\d*\.?\d*$" : "^[0-9]+$";
            return Regex.IsMatch(text, pattern);
        }
    }
}
