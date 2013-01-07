using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

using CC_CEDICT.WindowsPhone;
using SevenZip.Compression.LZMA.WindowsPhone;

namespace kuaishuo2
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            SearchPane.DataContext = App.ViewModel;
            TextToSpeech dummy = new TextToSpeech(null); // to initialise XNA framework
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        bool ok = false;
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ok) // already loaded (e.g. coming back from settings page)
                return;

            List<string> files = new List<string> { "cedict_ts.u8", "english.index", "hanzi.index", "pinyin.index" };
            foreach (string file in files)
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    if (!store.FileExists(file))
                        ExtractFile(file);

            if (inProgress == 0)
                LoadDictionary();
        }

        #region decompress LZMA resources (dictionary, indexes)

        int inProgress = 0;
        void ExtractFile(string file)
        {
            inProgress++;
            Resource2IsolatedStorageDecoder decoder = new Resource2IsolatedStorageDecoder();
            Resource2IsolatedStorageDecoder.AllowConcurrentDecoding = false;
            decoder.ProgressChanged += new ProgressChangedEventHandler(decoder_ProgressChanged);
            decoder.RunWorkerCompleted += new RunWorkerCompletedEventHandler(decoder_RunWorkerCompleted);
            Progress.Visibility = System.Windows.Visibility.Visible;
            decoder.DecodeAsync(file + ".lzma", file);
        }

        void decoder_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string file = (string)e.UserState;
            Status.Text = String.Format("Extracting {0}... {1}%", file, e.ProgressPercentage);
            Progress.Value = e.ProgressPercentage;
        }

        private void decoder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (--inProgress > 0) // still busy
                return;
            Progress.Visibility = System.Windows.Visibility.Collapsed; // don't need this any more
            LoadDictionary();
        }

        #endregion

        #region load dictionary (and indexes)

        Dictionary d;
        Searcher s;
        void LoadDictionary()
        {
            d = new Dictionary("cedict_ts.u8");
            s = new Searcher(d, new Index("english.index"), new Index("pinyin.index"), new Index("hanzi.index"));
            Status.Text = "Enter your search phrase above.";
            ok = true;
        }

        #endregion

        #region toggle search query placeholder text

        string defaultText = null;
        bool appBarVisibility;
        void Query_GotFocus(object sender, RoutedEventArgs e)
        {
            if (defaultText == null)
                defaultText = Query.Text;
            if (Query.Text.Equals(defaultText))
                Query.Text = "";
            else
                Query.SelectAll();
            appBarVisibility = ApplicationBar.IsVisible;
            ApplicationBar.IsVisible = false; // otherwise annoying to accidentally touch ... when pressing -> (enter)
        }

        void Query_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Query.Text.Length == 0)
                Query.Text = defaultText;
            ApplicationBar.IsVisible = appBarVisibility;
        }

        #endregion

        #region search

        string lastQuery = "";
        void Query_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || !ok)
                return;

            Query.Text = Query.Text.Trim();
            int minRelevance = Query.Text.Equals(lastQuery) ? 30 : 75;
            TriggerSearch(Query.Text, minRelevance);
        }

        void TriggerSearch(string query, int minRelevance)
        {
            List<DictionaryRecord> results = s.Search(query, minRelevance);

            if (results.Count == 0 && s.Total > 0) // try again
                results = s.Search(query);

            // reset things that need to be reset :)
            prev[Results.Name] = -1; // override expansion marker

            if (results.Count == 0)
            {
                Status.Text = String.Format("No results for '{0}'. Try another search.", query);
                Status.Visibility = System.Windows.Visibility.Visible;
                App.ViewModel.ClearData();
            }
            else // replace old search results with new
            {
                Status.Text = String.Format("Showing results for '{0}' (omitted '{1}')", s.LastQuery, s.Ignored);
                Status.Visibility = s.SmartSearch ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                App.ViewModel.LoadData(results);
            }

            lastQuery = query;
            Results.Focus();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            DictionaryRecord record = d[i];
            Query.Text = record.Chinese.Simplified;
            TriggerSearch(Query.Text, 30);
            pivot.SelectedIndex = pivot.Items.IndexOf(SearchPane);
        }

        #endregion

        #region pivot switching

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0: // search page
                    ApplicationBar = ((ApplicationBar)Resources["AppBar_SearchPivotPage"]);
                    break;
                case 1: // lists page
                    ApplicationBar = ((ApplicationBar)Resources["AppBar_ListsPivotPage"]);
                    ConvertNotepadToList(); // upgrade stored data from 0.8 to 0.9
                    LoadLists();
                    break;
            }
            ApplicationBar.IsVisible = true;
        }

        #endregion

        #region Chinese decomposition

        private void DecomposeButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            DictionaryRecord record = d[i];
            List<DictionaryRecord> results = new List<DictionaryRecord>();
            results.Add(record);
            foreach (Chinese.Character c in record.Chinese.Characters)
                results.AddRange(s.Search(c.Simplified.ToString(), 100));
            Query.Text = record.Chinese.Simplified + " (split)";
            prev[Results.Name] = -1; // override expansion marker
            Status.Visibility = System.Windows.Visibility.Collapsed;
            App.ViewModel.LoadData(results);
            Results.ScrollIntoView(Results.Items[0]);
            pivot.SelectedIndex = pivot.Items.IndexOf(SearchPane);
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

        #region list handling

        bool once = false;
        void ConvertNotepadToList()
        {
            Settings s = new Settings();
            if (!once) // simulate a pre-existing notepad
            {
                s.NotepadItemsSetting.Add(8734);
                s.NotepadItemsSetting.Add(1234);
                once = true;
            }
            if (s.NotepadItemsSetting.Count == 0) // done (or nothing to do)
                return;
            Debug.WriteLine("UPGRADE: Converting <=0.8 notepad data to >=0.9 list data.");
            App app = (App)Application.Current;
            foreach (int id in s.NotepadItemsSetting)
                app.ListManager["notepad"].Add(d[id]);
            app.ListManager["notepad"].Save();
            s.NotepadItemsSetting.Clear(); // empty old notepad
        }

        void LoadLists()
        {
            ListViewModel lvm = new ListViewModel();
            lvm.LoadData();
            ListsPane.DataContext = lvm;
        }

        private void NewList_Click(object sender, EventArgs e)
        {
            ListViewModel lvm = (ListViewModel)ListsPane.DataContext;
            ListItemViewModel item = new ListItemViewModel { Name = "", LineTwo = "Enter a name and hit return.", IsEditable = true };
            lvm.Items.Insert(0, item);
        }

        void NewList_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Visibility == System.Windows.Visibility.Collapsed)
                return;
            textBox.Focus();
        }

        private void NewList_GotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = false;
        }

        private void NewList_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
        }

        private void NewList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;
            TextBox textBox = (TextBox)sender;
            App app = (App)Application.Current;
            if (app.ListManager.ContainsKey(textBox.Text))
            {
                MessageBox.Show(String.Format("There is already a list called '{0}'. Enter a unique name.", textBox.Text));
                return;
            }
            app.ListManager[textBox.Text].Save();
            LoadLists();
        }

        private void DeleteList_Click(object sender, EventArgs e)
        {
        }

        private void ListListBox_SelectionChanged_OpenList(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = (ListBox)sender;
            int item = list.SelectedIndex;
            if (item == -1)
                return;
            list.SelectedIndex = -1; // reset
            ListItemViewModel ivm = (ListItemViewModel)list.Items[item];
            string uri = String.Format("/ListPage.xaml?name={0}", ivm.Name);
            NavigationService.Navigate(new Uri(uri, UriKind.Relative));
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

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }
    }
}
