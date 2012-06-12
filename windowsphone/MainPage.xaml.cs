using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

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
            
            int minRelevance = Query.Text.Equals(lastQuery) ? 30 : 75;
            List<DictionaryRecord> results = s.Search(Query.Text, minRelevance);

            if (results.Count == 0 && s.Total > 0) // try again
                results = s.Search(Query.Text);

            // reset things that need to be reset :)
            prev[Results.Name] = -1; // override expansion marker
            disabledNotepadButtons.Clear(); // empty list of buttons that don't exist any more :)

            if (results.Count == 0)
            {
                Status.Text = String.Format("No results for '{0}'. Try another search.", Query.Text.Trim());
                Status.Visibility = System.Windows.Visibility.Visible;
                App.ViewModel.ClearData();
            }
            else // replace old search results with new
            {
                Status.Text = String.Format("Showing results for '{0}' (omitted '{1}')", s.LastQuery, s.Ignored);
                Status.Visibility = s.SmartSearch ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                App.ViewModel.LoadData(results);
            }

            lastQuery = Query.Text;
            Results.Focus();
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
        void Results_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            Results.SelectedIndex = -1; // reset
        }

        //Brush old;
        void ToggleView(ListBox list, int index, bool expand)
        {
            ListBoxItem item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(index);
            if (item == null)
                return;
            
            StackPanel defaultView = FindStackPanelByName(item, "DefaultView");
            StackPanel expandedView = FindStackPanelByName(item, "ExpandedView");
            StackPanel actionPanel = FindStackPanelByName(item, "ActionPanel");

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

        StackPanel FindStackPanelByName(DependencyObject parent, string name)
        {
            try
            {
                var count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child != null && child is StackPanel && ((StackPanel)child).Name.Equals(name))
                        return (StackPanel)child;
                    var result = FindStackPanelByName(child, name);
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

        #endregion

        #region notepad view (inc. add/remove items)

        MainViewModel notes;
        void pivot_LoadingPivotItem(object sender, PivotItemEventArgs e)
        {
            if (!e.Item.Equals(NotepadPane)) // only handle switching to Notepad
                return;
            if (notes == null)
                LoadNotes();
            UpdateNotepadStatus();
        }

        void UpdateNotepadStatus()
        {
            NotepadStatus.Text = (notes.Items.Count == 0)
                ? "\nThere are no entries in your notepad.\nSearch then use (+) button to add entries."
                : "";
        }

        void LoadNotes()
        {
            Settings settings = new Settings();
            List<DictionaryRecord> items = new List<DictionaryRecord>();
            foreach (int id in settings.NotepadItemsSetting)
                items.Add(d[id]);
            items.Reverse();
            notes = new MainViewModel();
            notes.LoadData(items);
            NotepadPane.DataContext = notes;
        }

        Dictionary<int, Button> disabledNotepadButtons = new Dictionary<int, Button>();
        void NotepadButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            List<int> items = settings.NotepadItemsSetting;
            Button button = (Button)sender;
            int i;
            int.TryParse(button.Tag.ToString(), out i);
            items.Add(i);
            settings.NotepadItemsSetting = items;
            LoadNotes();
            prev[NotepadItems.Name] = -1; // override expansion marker
            button.IsEnabled = false;
            disabledNotepadButtons[i] = button;
            pivot.SelectedIndex = pivot.Items.IndexOf(NotepadPane);
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
            prev[NotepadItems.Name] = -1; // override expansion marker
            if (disabledNotepadButtons.ContainsKey(i))
                disabledNotepadButtons[i].IsEnabled = true;
            UpdateNotepadStatus();
        }

        #endregion

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }
    }
}
