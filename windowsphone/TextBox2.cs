using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace kuaishuo2
{
    public class TextBox2 : TextBox
    {
        public event VisibilityChangedEventHandler VisibilityChanged;
        public delegate void VisibilityChangedEventHandler(object sender, EventArgs e);
        public static readonly DependencyProperty VisibilityChangedProperty = DependencyProperty.Register(
            "VisibilityChanged", typeof(VisibilityChangedEventHandler), typeof(TextBox2), null);

        static readonly DependencyProperty MirrorVisibilityProperty = DependencyProperty.Register(
            "MirrorVisibility", typeof(Visibility), typeof(TextBox2), new PropertyMetadata(MirrorVisibilityChanged));

        public TextBox2()
        {
            SetBinding(TextBox2.MirrorVisibilityProperty, new Binding("Visibility") { Source = this });
        }

        static void MirrorVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.IsInDesignTool)
                ((TextBox2)obj).VisibilityChanged(obj, null);
        }
    }
}
