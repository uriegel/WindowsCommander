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

    static async Task<BitmapSource?> ExtractIconAsync(string name)
    {
        if (!icons.TryGetValue(name, out var bitmapSource))
        {
            return await Task.Run(() =>
            {
                var info = new ShFileInfo();
                var ptr = Api.SHGetFileInfo(name, ClrWinApi.FileAttributes.Normal, ref info, Marshal.SizeOf(info),
                SHGetFileInfoConstants.TYPENAME |
                SHGetFileInfoConstants.SYSICONINDEX |
                SHGetFileInfoConstants.USEFILEATTRIBUTES);

                int index = info.Icon;
                int size = SHIL_SMALL; // Choose SHIL_EXTRALARGE for high DPI

                SHGetImageList(size, ref IID_IImageList, out var imageList);
                var res = imageList.GetIcon(index, 0x00000001, out var hIcon); // ILD_TRANSPARENT                    

                if (hIcon != 0)
                {
                    bitmapSource = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
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

    private const int SHIL_SMALL = 0x1; // or SHIL_SYSSMALL
    

    private static Guid IID_IImageList = new("46EB5926-582E-4017-9FDF-E8998DAA0950");

    [DllImport("shell32.dll")]
    private static extern int SHGetImageList(int iImageList, ref Guid riid, out IImageList ppv);

    [ComImport]
    [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IImageList
    {
        [PreserveSig] int Add(IntPtr hbmImage, IntPtr hbmMask, out int pi);
        [PreserveSig] int ReplaceIcon(int i, IntPtr hicon, out int pi);
        [PreserveSig] int SetOverlayImage(int iImage, int iOverlay);
        [PreserveSig] int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);
        [PreserveSig] int AddMasked(IntPtr hbmImage, uint crMask, out int pi);
        [PreserveSig] int Draw(IntPtr pimldp);
        [PreserveSig] int Remove(int i);
        [PreserveSig] int GetIcon(int i, int flags, out IntPtr phicon);
        // Others omitted
    }

    static readonly ConcurrentDictionary<string, BitmapSource> icons = [];
}

public record FileVersion(int Major, int Minor, int Patch, int Build)
{
    public override string ToString() => $"{Major}.{Minor}.{Patch}.{Build}";
}