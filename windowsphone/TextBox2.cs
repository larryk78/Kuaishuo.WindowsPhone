using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace kuaishuo2
{
    public class TextBox2 : TextBox
    {
        /// <summary>
        /// Extends System.Windows.Controls.TextBox with a VisibilityChanged event.
        /// </summary>
        public TextBox2()
        {
            DefaultStyleKey = typeof(TextBox);
        }

        public static readonly DependencyProperty VisibilityChangedProperty = DependencyProperty.Register(
            "VisibilityChanged",
            typeof(string),
            typeof(TextBox2),
            new PropertyMetadata("Set the VisibilityChanged event handler"));

        /// <summary>
        /// Sets the VisibilityChanged event handler.
        /// </summary>
        public event VisibilityChangedEventHandler VisibilityChanged;

        /// <summary>
        /// Defines the VisibilityChanged event handler delegate.
        /// </summary>
        /// <param name="sender">The TextBox2 object on which the Visibility was changed.</param>
        /// <param name="e">Default (empty) event args.</param>
        public delegate void VisibilityChangedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Gets or sets the underlying TextBox.Visibility. Set raises the VisibilityChanged event.
        /// </summary>
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
