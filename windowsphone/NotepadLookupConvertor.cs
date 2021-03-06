﻿using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace kuaishuo2
{
    public class NotepadLookupConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (DesignerProperties.IsInDesignTool)
                return true;
            App app = (App)Application.Current;
            bool availableList = false;
            foreach (DictionaryRecordList list in app.ListManager.Values)
                if (!list.ReadOnly && !list.IsDeleted)
                    availableList = true;
            return availableList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
