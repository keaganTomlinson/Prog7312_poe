using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace prog_p1.Theme
{
    public class WatermarkPasswordBox : Control
    {
        static WatermarkPasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WatermarkPasswordBox), new FrameworkPropertyMetadata(typeof(WatermarkPasswordBox)));
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(WatermarkPasswordBox));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }
    }
}