using System.Globalization;
using System.Windows.Data;

namespace Commander.Views;

public class StatusBarConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        =>  values[1] is string str
                ? $"Einschränkung auf: {str}"
                : values[0];

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
