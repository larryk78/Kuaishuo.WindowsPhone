using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace kuaishuo2
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        Settings settings = new Settings();

        public SettingsPage()
        {
            InitializeComponent();
        }
    }
}
