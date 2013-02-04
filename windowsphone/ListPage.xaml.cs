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
using System.Text;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

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

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App app = (App)Application.Current;
            d = app.Dictionary;
            list = app.ListManager[NavigationContext.QueryString["name"]];
            NotepadPane.Header = list.Name;
            LoadListData();
            if (app.Transition == App.TransitionType.PostAdd) // scroll the added item into view
            {
                //NotepadItems.ScrollIntoView // TODO: find the newly-added item
            }
        }

        void LoadListData()
        {
            MainViewModel mvm = new MainViewModel();
            mvm.LoadData(list);
            this.DataContext = mvm;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = (list.Count == 0) ? false : true;
        }

        int previous = -1;
        void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = (ListBox)sender;
            int item = list.SelectedIndex;
            if (item == -1)
                return;

            if (previous != item)
            {
                ToggleView(list, item, true);
                if (previous != -1)
                    ToggleView(list, previous, false);
                previous = item;
            }

            list.SelectedIndex = -1; // reset
            list.ScrollIntoView(list.Items[item]);
        }

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
            }
            else
            {
                defaultView.Visibility = Visibility.Visible;
                expandedView.Visibility = Visibility.Collapsed;
                actionPanel.Visibility = Visibility.Collapsed;
            }
        }

        void Pinyin_Loaded(object sender, EventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            ItemViewModel item = (ItemViewModel)textBlock.DataContext;
            DictionaryRecord record = item.Record;
            PinyinColorizer p = new PinyinColorizer();
            Settings settings = new Settings();
            p.Colorize(textBlock, record, (PinyinColorScheme)settings.PinyinColorSetting);
        }

        void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DictionaryRecord record = ((ItemViewModel)button.DataContext).Record;
            TextToSpeech tts = new TextToSpeech(record);
            Settings settings = new Settings();
            if (!tts.Speak(settings.AudioQualitySetting == 0 ? false : true))
            {
                App.ViewModel.UpdateNetworkStatus(false);
                MessageBox.Show("Sorry, Text-to-Speech is only available with a network connection.");
                return;
            }
        }

        void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DictionaryRecord record = ((ItemViewModel)button.DataContext).Record;
            Clipboard.SetText(record.Chinese.Simplified);
        }

        void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            App app = (App)Application.Current;
            app.Transition = App.TransitionType.ListUpdate;
            Button button = (Button)sender;
            DictionaryRecord record = ((ItemViewModel)button.DataContext).Record;
            list.Remove(record);
            LoadListData();
            previous = -1;
        }

        void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DictionaryRecord record = ((ItemViewModel)button.DataContext).Record;
            App app = (App)Application.Current;
            app.Transition = App.TransitionType.Specialise;
            app.TransitionData = record;
            NavigationService.GoBack();
        }

        void DecomposeButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DictionaryRecord record = ((ItemViewModel)button.DataContext).Record;
            App app = (App)Application.Current;
            app.Transition = App.TransitionType.Decompose;
            app.TransitionData = record;
            NavigationService.GoBack();
        }

        void EmailButton_Click(object sender, EventArgs e)
        {
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
            
            if (Encoding.UTF8.GetBytes(sb.ToString()).Length < 16384)
                sb.AppendLine(s2.ToString());
            
            sb.AppendLine("Redistributed under license. " + d.Header["license"]);

            try
            {
                EmailComposeTask email = new EmailComposeTask();
                email.Subject = String.Format("[Kuaishuo] {0}", list.Name);
                email.Body = sb.ToString();
                email.Show();
            }
            catch (ArgumentOutOfRangeException)
            {
                int size = Encoding.UTF8.GetBytes(sb.ToString()).Length / 1024;
                MessageBox.Show(String.Format(
                    "Sorry, Windows Phone has a 64KB size limit for emails sent from applications. " +
                    "Your notepad contains too many items to email ({0}KB). Please remove some and try again.", size));
            }
        }

        void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }
    }
}
