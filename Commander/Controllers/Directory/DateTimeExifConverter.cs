using System.Globalization;
using System.Windows.Data;

namespace Commander.Controllers.Directory;

public class DateTimeExifConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values[0] is DateTime dateTime 
            ? values[1] is DateTime exifTime 
            ? exifTime.ToString() 
            : dateTime.ToString()
            : null;
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
