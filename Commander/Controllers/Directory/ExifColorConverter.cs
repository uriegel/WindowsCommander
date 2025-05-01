using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Commander.Controllers.Directory;

public class ExifColorConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is DateTime
            ? ThemeHelper.IsDarkTheme() ? Brushes.LightYellow : Brushes.DarkBlue
            : DependencyProperty.UnsetValue;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
