using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
            this.NetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
        }

        public ObservableCollection<ItemViewModel> Items { get; private set; }

        public bool IsDataLoaded { get; private set; }

        public void LoadData(List<DictionaryRecord> items)
        {
            ClearData();
            this.NetworkAvailable = NetworkInterface.GetIsNetworkAvailable();

            Settings settings = new Settings();
            bool trad = settings.TraditionalChineseSetting;

            foreach (DictionaryRecord r in items)
            {
                // determine what Hanzi to show to the user
                string chinese = (!trad || r.Chinese.Simplified.Equals(r.Chinese.Traditional))
                    ? r.Chinese.Simplified                                                     // show only simplified
                    : String.Format("{0} ({1})", r.Chinese.Simplified, r.Chinese.Traditional); // else "simple (trad)"

                this.Items.Add(new ItemViewModel()
                {
                    Record = r,
                    Pinyin = r.Chinese.Pinyin,
                    English = String.Join("; ", r.English),
                    EnglishWithNewlines = String.Join("\n", r.English),
                    Chinese = chinese,
                    Index = r.Index
                });
            }
            this.IsDataLoaded = true;
        }

        public void ClearData()
        {
            this.Items.Clear();
        }

        // TODO: enable play buttons for those where the audio has already been downloaded (even in no-network scenario)
        public bool NetworkAvailable { get; private set; }
        public void UpdateNetworkStatus(bool status)
        {
            this.NetworkAvailable = status;
            NotifyPropertyChanged("NetworkAvailable");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}