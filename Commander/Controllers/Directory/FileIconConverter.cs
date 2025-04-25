using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ClrWinApi;

using CsTools.Extensions;

namespace Commander.Controllers.Directory;

public class FileIconConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            ImageBrush? imageBrush;
            var extension = path.GetFileExtension();
            //if (string.Compare(extension, ".7z", true) == 0)
            //    extension = ".zip";
            //else if (string.Compare(extension, ".rar", true) == 0)
            //    extension = ".zip";
            //else if (string.Compare(extension, ".gz", true) == 0)
            //    extension = ".zip";
            //else if (string.Compare(extension, ".tar", true) == 0)
            //    extension = ".zip";

            //if (extension == ".exe")
            //    imageBrush = ImageBrushCreator.ExtractIcon(path);
            //else if (!icons.TryGetValue(extension, out imageBrush))
            //{
                imageBrush = ExtractIcon(extension ?? ".");
            //    icons[extension] = imageBrush;
            //}
            return imageBrush;
        }
        else
            return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    static ImageBrush? ExtractIcon(string name)
    {
        var info = new ShFileInfo();
        IntPtr ptr = Api.SHGetFileInfo(name, FileAttributes.Normal, ref info, Marshal.SizeOf(info),
            SHGetFileInfoConstants.ICON |
            SHGetFileInfoConstants.SMALLICON |
            SHGetFileInfoConstants.USEFILEATTRIBUTES |
            SHGetFileInfoConstants.TYPENAME);

        if (info.IconHandle == 0)
            return null;
        var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(info.IconHandle,Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        var brush = new ImageBrush(bitmapSource)
        {
            Stretch = Stretch.None
        };
        return brush;
    }

}
