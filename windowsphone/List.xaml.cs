using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    public partial class List : PhoneApplicationPage
    {
        public List()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
        }

        Dictionary d;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            d = new Dictionary("cedict_ts.u8");
        }

        #region notepad view (inc. add/remove items)

        MainViewModel notes;
        void LoadNotes()
        {
            Settings settings = new Settings();
            List<DictionaryRecord> items = new List<DictionaryRecord>();
            foreach (int id in settings.NotepadItemsSetting)
                items.Add(d[id]);
            items.Reverse();
            notes = new MainViewModel();
            notes.LoadData(items);
            ApplicationBarIconButton emailButton = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
            if (notes.Items.Count != 0)
            {
                emailButton.IsEnabled = true;
                ApplicationBar.Mode = ApplicationBarMode.Default;
            }
            else
            {
                emailButton.IsEnabled = false;
                ApplicationBar.Mode = ApplicationBarMode.Minimized;
            }
        }

        void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            List<int> items = settings.NotepadItemsSetting;
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            items.Remove(i);
            settings.NotepadItemsSetting = items;
            LoadNotes();
        }

        #endregion
    }
}