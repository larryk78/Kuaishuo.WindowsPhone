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
            foreach (DictionaryRecord r in items)
                this.Items.Add(new ItemViewModel()
                {
                    Pinyin = r.Chinese.Pinyin,
                    English = String.Join("; ", r.English),
                    EnglishWithNewlines = String.Join("\n", r.English),
                    SimplifiedChinese = r.Chinese.Simplified,
                    Index = r.Index
                });
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