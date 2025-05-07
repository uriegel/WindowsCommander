using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ClrWinApi;

using CsTools.Extensions;
using System.Collections.Concurrent;

namespace Commander.Controllers.Directory;

public record FileItem : Item
{
    public string? IconPath 
    { 
        get => field; 
        set
        {
            OnChanged(nameof(Icon));
            field = value;
        }
    }

    public ImageBrush? Icon
    {
        get
        {
            if (field == null)
            {
                var extension = IconPath != null && IconPath.EndsWith("exe", StringComparison.InvariantCultureIgnoreCase)
                    ? IconPath
                    : IconPath?.GetFileExtension();
                //if (string.Compare(extension, ".7z", true) == 0)
                //    extension = ".zip";
                //else if (string.Compare(extension, ".rar", true) == 0)
                //    extension = ".zip";
                //else if (string.Compare(extension, ".gz", true) == 0)
                //    extension = ".zip";
                //else if (string.Compare(extension, ".tar", true) == 0)
                //    extension = ".zip";
                GetIcon();
                async void GetIcon()
                {
                    var bitmapSource = await ExtractIconAsync(extension ?? ".");
                    if (bitmapSource != null)
                    {
                        field = new ImageBrush(bitmapSource)
                        {
                            Stretch = Stretch.None
                        };
                    }
                    OnChanged(nameof(Icon));
                }
            }
            return field;
        }
        set
        {
            OnChanged(nameof(Icon));
            field = value;
        }
    }

    public DateTime DateTime
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(DateTime));
        }
    }

    public DateTime? ExifTime
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(ExifTime));
        }
    }

    public FileVersion? FileVersion
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(FileVersion));
        }
    }

    public long Size
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(Size));
        }
    }

    public static FileItem Create(FileInfo info)
        => new()
        {
            Name = info.Name ?? "",
            IsHidden = info.Attributes.HasFlag(System.IO.FileAttributes.Hidden),
            IconPath = info.Name?.EndsWith("exe", StringComparison.InvariantCultureIgnoreCase) == true
                        ? info.FullName 
                        : info.Name ?? "",
            DateTime = info.LastWriteTime,
            Size = info.Length
        };

    static async Task<BitmapSource?> ExtractIconAsync(string name)
    {
        if (!icons.TryGetValue(name, out var bitmapSource))
        {
            return await Task.Run(() =>
            {
                var info = new ShFileInfo();
                var ptr = Api.SHGetFileInfo(name, ClrWinApi.FileAttributes.Normal, ref info, Marshal.SizeOf(info),
                SHGetFileInfoConstants.ICON |
                SHGetFileInfoConstants.SMALLICON |
                SHGetFileInfoConstants.USEFILEATTRIBUTES |
                SHGetFileInfoConstants.TYPENAME);

                if (info.IconHandle != 0)
                {
                    bitmapSource = Imaging.CreateBitmapSourceFromHIcon(info.IconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    icons[name] = bitmapSource;
                    return bitmapSource;
                }
                else
                    return null;
            });
        }
        else
            return bitmapSource;
    }

    static readonly ConcurrentDictionary<string, BitmapSource> icons = [];
}

public record FileVersion(int Major, int Minor, int Patch, int Build)
{
    public override string ToString() => $"{Major}.{Minor}.{Patch}.{Build}";
}