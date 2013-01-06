using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace kuaishuo2
{
    public class ListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ListItemViewModel> Items { get; private set; }
        public ListViewModel()
        {
            this.Items = new ObservableCollection<ListItemViewModel>();
            LoadData();
        }

        public void LoadData()
        {
            this.Items.Clear();
            App app = (App)Application.Current;
            List<string> lists = new List<string>();
            foreach (string key in app.ListManager.Keys)
                lists.Add(key);
            lists.Sort();
            foreach (string name in lists)
            {
                DictionaryRecordList list = app.ListManager[name];
                list.Slurp();
                Items.Add(new ListItemViewModel {
                    LineOne = list.Name,
                    LineTwo = String.Format("{0} entr{1}", list.Count, (list.Count == 1 ? "y" : "ies"))
                });
            }
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
