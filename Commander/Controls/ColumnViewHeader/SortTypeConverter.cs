using System.Globalization;
using System.Windows.Data;

namespace Commander.Controls.ColumnViewHeader;

class SortTypeConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            SortType.Ascending => "🢕",
            SortType.Descending => "🢗",
            _ => null
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
