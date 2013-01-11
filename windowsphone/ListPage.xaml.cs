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
    public partial class ListPage : PhoneApplicationPage
    {
        public ListPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
        }

        Dictionary d;
        DictionaryRecordList list;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App app = (App)Application.Current;
            list = app.ListManager[NavigationContext.QueryString["name"]];
            NotepadPane.Header = list.Name;
            MainViewModel mvm = new MainViewModel();
            mvm.LoadData(list);
            this.DataContext = mvm;
            //d = new Dictionary("cedict_ts.u8");
        }

        #region notepad view (inc. add/remove items)

        void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            List<int> items = settings.NotepadItemsSetting;
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            items.Remove(i);
            settings.NotepadItemsSetting = items;
        }

        #endregion
        
        #region expand/collapse list items

        Dictionary<string, int> prev = new Dictionary<string, int>();
        // IMPORTANT: this event handler is used by both search and notepad lists
        // DO NOT USE Results or NotepadItems objects directly
        // DO USE (ListBox)sender to ensure the correct list is manipulated
        void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = (ListBox)sender;
            int item = list.SelectedIndex;
            if (item == -1)
                return;

            int previous = prev.ContainsKey(list.Name) ? prev[list.Name] : -1;
            if (previous != item)
            {
                ToggleView(list, item, true);
                if (previous != -1)
                    ToggleView(list, previous, false);
                prev[list.Name] = item;
            }

            list.SelectedIndex = -1; // reset
            list.ScrollIntoView(list.Items[item]);
        }

        //Brush old;
        void ToggleView(ListBox list, int index, bool expand)
        {
            ListBoxItem item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(index);
            if (item == null)
                return;
            
            StackPanel defaultView = VisualTreeHelperHelper.FindFrameworkElementByName<StackPanel>(item, "DefaultView");
            StackPanel expandedView = VisualTreeHelperHelper.FindFrameworkElementByName<StackPanel>(item, "ExpandedView");
            StackPanel actionPanel = VisualTreeHelperHelper.FindFrameworkElementByName<StackPanel>(item, "ActionPanel");

            if (expand)
            {
                defaultView.Visibility = Visibility.Collapsed;
                expandedView.Visibility = Visibility.Visible;
                actionPanel.Visibility = Visibility.Visible;
                //old = item.BorderBrush;
                //item.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            }
            else
            {
                defaultView.Visibility = Visibility.Visible;
                expandedView.Visibility = Visibility.Collapsed;
                actionPanel.Visibility = Visibility.Collapsed;
                //item.BorderBrush = old;
            }
        }

        #endregion

        #region colour Pinyin

        private void Pinyin_Loaded(object sender, EventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            ItemViewModel item = (ItemViewModel)textBlock.DataContext;
            DictionaryRecord record = item.Record;
            PinyinColorizer p = new PinyinColorizer();
            Settings settings = new Settings();
            p.Colorize(textBlock, record, (PinyinColorScheme)settings.PinyinColorSetting);
        }

        #endregion

        #region Text-to-Speech button

        void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            TextToSpeech tts = new TextToSpeech(d[i]);
            Settings settings = new Settings();
            if (!tts.Speak(settings.AudioQualitySetting == 0 ? false : true))
            {
                App.ViewModel.UpdateNetworkStatus(false);
                MessageBox.Show("Sorry, Text-to-Speech is only available with a network connection.");
                return;
            }
        }

        #endregion

        #region Copy-to-Clipboard action

        //Button lastCopy;
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            DictionaryRecord r = d[i];
            Clipboard.SetText(r.Chinese.Simplified);
            /*
            button.Background = new SolidColorBrush(SystemColors.HighlightColor);
            if (lastCopy != null)
                lastCopy.Background = new SolidColorBrush();
            lastCopy = button;
             */
        }

        #endregion

        #region email

        private void EmailButton_Click(object sender, EventArgs e)
        {
            /*
            StringBuilder sb = new StringBuilder();
            StringBuilder s2 = new StringBuilder();

            foreach (ItemViewModel item in NotepadItems.Items)
            {
                sb.AppendLine(item.Pinyin);
                sb.AppendLine(item.EnglishWithNewlines);
                sb.AppendLine(item.Chinese);
                sb.AppendLine();
                s2.AppendLine(item.Record.ToString());
            }

            sb.AppendLine("-- Kuaishuo Chinese Dictionary http://www.knibb.co.uk/kuaishuo");
            sb.AppendLine("________________________________________");
            sb.AppendLine("CC-CEDICT ed. " + d.Header["date"]);
            sb.AppendLine();
            sb.AppendLine(s2.ToString());
            sb.AppendLine("This extract redistributed under license. " + d.Header["license"]);

            EmailComposeTask email = new EmailComposeTask();
            email.Subject = "[Kuaishuo] notepad";
            email.Body = sb.ToString();
            email.Show();
             */
        }

        #endregion
    }
}