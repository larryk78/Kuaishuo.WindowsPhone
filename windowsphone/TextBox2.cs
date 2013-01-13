using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace kuaishuo2
{
    public class TextBox2 : TextBox
    {
        public TextBox2()
        {
            DefaultStyleKey = typeof(TextBox);
        }

        public static readonly DependencyProperty VisibilityChangedProperty = DependencyProperty.Register(
            "VisibilityChanged",
            typeof(string),
            typeof(TextBox2),
            new PropertyMetadata("Set the VisibilityChanged event handler"));

        public event VisibilityChangedEventHandler VisibilityChanged;

        public delegate void VisibilityChangedEventHandler(object sender, EventArgs e);

        public new Visibility Visibility
        {
            get
            {
                return base.Visibility;
            }
            set
            {
                if (base.Visibility != value)
                {
                    base.Visibility = value;
                    VisibilityChanged(this, new EventArgs());
                }
            }
        }
    }
}
