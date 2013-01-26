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
                string lineTwo = list.Count.ToString();
                Items.Add(new ListItemViewModel { Name = list.Name, LineTwo = lineTwo, IsDeleted = list.IsDeleted });
            }
        }

        private bool _AddInProgress = false;
        public bool AddInProgress
        {
            get
            {
                return _AddInProgress;
            }
            set
            {
                if (value != _AddInProgress)
                {
                    _AddInProgress = value;
                    NotifyPropertyChanged("AddInProgress");
                    NotifyPropertyChanged("NotBusy");
                }
            }
        }

        private bool _EditInProgress = false;
        public bool EditInProgress
        {
            get
            {
                return _EditInProgress;
            }
            set
            {
                if (value != _EditInProgress)
                {
                    _EditInProgress = value;
                    NotifyPropertyChanged("EditInProgress");
                    NotifyPropertyChanged("NotBusy");
                }
            }
        }

        /// <summary>
        /// Inverse of Add/EditInProgress for XAML binding.
        /// </summary>
        public bool NotBusy
        {
            get
            {
                return (!AddInProgress && !EditInProgress);
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
