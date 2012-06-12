using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace kuaishuo2
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        private string _pinyin;
        public string Pinyin
        {
            get
            {
                return _pinyin;
            }
            set
            {
                if (value != _pinyin)
                {
                    _pinyin = value;
                    NotifyPropertyChanged("Pinyin");
                }
            }
        }

        private string _english;
        public string English
        {
            get
            {
                return _english;
            }
            set
            {
                if (value != _english)
                {
                    _english = value;
                    NotifyPropertyChanged("English");
                }
            }
        }

        private string _englishWithNewlines;
        public string EnglishWithNewlines
        {
            get
            {
                return _englishWithNewlines;
            }
            set
            {
                if (value != _englishWithNewlines)
                {
                    _englishWithNewlines = value;
                    NotifyPropertyChanged("English");
                }
            }
        }

        private string _chinese;
        public string Chinese
        {
            get
            {
                return _chinese;
            }
            set
            {
                if (value != _chinese)
                {
                    _chinese = value;
                    NotifyPropertyChanged("Chinese");
                }
            }
        }

        private int _index;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (value != _index)
                {
                    _index = value;
                    NotifyPropertyChanged("Index");
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