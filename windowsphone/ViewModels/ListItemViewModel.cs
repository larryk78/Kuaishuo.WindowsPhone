using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace kuaishuo2
{
    public class ListItemViewModel : INotifyPropertyChanged
    {
        private string _line1;
        public string Name
        {
            get
            {
                return _line1;
            }
            set
            {
                if (value != _line1)
                {
                    _line1 = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _line2;
        public string LineTwo
        {
            get
            {
                return _line2;
            }
            set
            {
                if (value != _line2)
                {
                    _line2 = value;
                    NotifyPropertyChanged("LineTwo");
                }
            }
        }

        private string _line3;
        public string LineThree
        {
            get
            {
                return _line3;
            }
            set
            {
                if (value != _line3)
                {
                    _line3 = value;
                    NotifyPropertyChanged("LineThree");
                }
            }
        }

        private bool _isEditable = false;
        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }
            set
            {
                if (value != _isEditable)
                {
                    _isEditable = value;
                    NotifyPropertyChanged("IsEditable");
                }
            }
        }

        private bool _isDeleted = false;
        public bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                if (value != _isDeleted)
                {
                    _isDeleted = value;
                    NotifyPropertyChanged("IsDeleted");
                }
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
