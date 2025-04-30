using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Commander.Views;

public class InfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string
            ? ThemeHelper.IsDarkTheme() ? Brushes.DarkBlue : Brushes.LightBlue
            : Brushes.Transparent;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
