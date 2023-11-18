using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace prog_p1.Theme
{
    public static class WatermarkBehavior
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(WatermarkBehavior), new PropertyMetadata(OnWatermarkChanged));

        public static string GetWatermark(DependencyObject obj)
        {
            return (string)obj.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, string value)
        {
            obj.SetValue(WatermarkProperty, value);
        }

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = e.NewValue as string;
                }

                textBox.GotKeyboardFocus += (sender, args) =>
                {
                    if (textBox.Text == GetWatermark(textBox))
                    {
                        textBox.Text = string.Empty;
                        textBox.Foreground = System.Windows.Media.Brushes.White; // Change the color as needed
                    }
                };

                textBox.LostKeyboardFocus += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.Text = GetWatermark(textBox);
                        textBox.Foreground = System.Windows.Media.Brushes.Gray; // Change the color as needed
                    }
                };
            }
        }
    }
}