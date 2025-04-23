using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Commander.Controls.ColumnViewHeader;

public class HeaderGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => new GridLength((double)value, GridUnitType.Star);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}