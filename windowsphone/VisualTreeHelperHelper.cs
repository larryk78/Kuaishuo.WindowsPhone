using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace kuaishuo2
{
    public class VisualTreeHelperHelper
    {
        public static T FindFrameworkElementByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            try
            {
                var count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child != null && child is T && ((FrameworkElement)child).Name.Equals(name))
                        return (T)child;
                    var result = FindFrameworkElementByName<T>(child, name);
                    if (result != null)
                        return result;
                }
                return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
