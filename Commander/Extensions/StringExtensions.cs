using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Commander.Extensions;
static class StringExtensions
{
    public static ImageSource? IconFromResource(this string relativePath, int size = 16)
        => InitIcon(new CacheKey(relativePath, size));

    //static Func<CacheKey, ImageSource> IconFromResourceFunc { get; } 
    //    = Memoize(InitIcon);

    static ImageSource? InitIcon(CacheKey param)
    {
        var uri = new Uri($"pack://application:,,,/{param.Path}", UriKind.Absolute);
        var decoder = new IconBitmapDecoder(uri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

        return decoder.Frames
            .OrderBy(f => Math.Abs(f.PixelWidth - param.Size))
            .FirstOrDefault();
    }

    //static ImageSource LoadIconFromResource(string relativePath, int size = 16)
    //{
    //}

    record CacheKey(string Path, int Size);
}
