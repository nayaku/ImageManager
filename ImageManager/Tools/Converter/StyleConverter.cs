﻿using System.Windows;
using System.Windows.Data;

namespace ImageManager.Tools.Converter
{
    public class StyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return null;

            if (value is string str)
            {
                return Application.Current.FindResource(str);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
