using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Coffee.DigitalPlatform.Views
{
    public class DataPagerExtension
    {
        public static bool GetIsPageNumber(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPageNumberProperty);
        }

        public static void SetIsPageNumber(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPageNumberProperty, value);
        }

        public static readonly DependencyProperty IsPageNumberProperty =
            DependencyProperty.RegisterAttached("IsPageNumber", typeof(bool), typeof(DataPagerExtension), new PropertyMetadata(false, OnIsPageNumberChanged));

        private static void OnIsPageNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, !(bool)e.NewValue);
                textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                if ((bool)e.NewValue)
                {
                    textBox.PreviewTextInput += TextBox_PreviewTextInput; ;
                }
            }
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int firstNum = GetFirstPageNumber(textBox);
            int lastNum = GetLastPageNumber(textBox);

            if (!int.TryParse(e.Text, out int inputNum))
            {
                e.Handled = true;
            }
            else
            {
                if (textBox.SelectionStart >= 0)
                {
                    int selectedLength = !string.IsNullOrEmpty(textBox.SelectedText) ? textBox.SelectedText.Length : 0;
                    string newText = textBox.Text.Remove(textBox.SelectionStart, selectedLength);
                    newText = newText.Insert(textBox.SelectionStart, e.Text);
                    if (int.TryParse(newText, out int newNum))
                    {
                        e.Handled = newNum < firstNum || newNum > lastNum;
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        public static int GetFirstPageNumber(DependencyObject obj)
        {
            return (int)obj.GetValue(FirstPageNumberProperty);
        }
        public static void SetFirstPageNumber(DependencyObject obj, int value)
        {
            obj.SetValue(FirstPageNumberProperty, value);
        }
        public static readonly DependencyProperty FirstPageNumberProperty =
            DependencyProperty.RegisterAttached("FirstPageNumber", typeof(int), typeof(DataPagerExtension), new PropertyMetadata(0));


        public static int GetLastPageNumber(DependencyObject obj)
        {
            return (int)obj.GetValue(LastPageNumberProperty);
        }
        public static void SetLastPageNumber(DependencyObject obj, int value)
        {
            obj.SetValue(LastPageNumberProperty, value);
        }
        public static readonly DependencyProperty LastPageNumberProperty =
            DependencyProperty.RegisterAttached("LastPageNumber", typeof(int), typeof(DataPagerExtension), new PropertyMetadata(0));
    }
}
