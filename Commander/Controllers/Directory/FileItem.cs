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

    public BitmapSource? Icon
    {
        get
        {
            if (field == null)
            {
                GetIcon();
                async void GetIcon()
                {
                    var bitmapSource = await FileIcon.Get(IconPath);
                    if (bitmapSource != null)
                    {
                        field = bitmapSource;
                        OnChanged(nameof(Icon));
                    }
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
}

public record FileVersion(int Major, int Minor, int Patch, int Build)
{
    public override string ToString() => $"{Major}.{Minor}.{Patch}.{Build}";
}