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
                string lineTwo;
                if (list.IsDeleted)
                    lineTwo = "To be deleted on exit. Open to undo.";
                else
                    lineTwo = String.Format("{0} entr{1}", list.Count, (list.Count == 1 ? "y" : "ies"));
                Items.Add(new ListItemViewModel { Name = list.Name, LineTwo = lineTwo });
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
                    NotifyPropertyChanged("NotEditing");
                }
            }
        }

        /// <summary>
        /// Inverse of EditInProgress for XAML binding.
        /// </summary>
        public bool NotEditing
        {
            get
            {
                return !EditInProgress;
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
