﻿using System.Globalization;
using System.Windows.Data;

namespace Commander.ValueConverters;

public class NotNullValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value != null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
