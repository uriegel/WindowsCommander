using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using ClrWinApi;

using CsTools.Extensions;

namespace Commander.Controllers.Directory;

static class FileIcon
{
    public static async Task<BitmapSource?> Get(string? iconPath)
    {
        var extension = iconPath != null && iconPath.EndsWith("exe", StringComparison.InvariantCultureIgnoreCase)
            ? iconPath
            : iconPath?.GetFileExtension();
        //if (string.Compare(extension, ".7z", true) == 0)
        //    extension = ".zip";
        //else if (string.Compare(extension, ".rar", true) == 0)
        //    extension = ".zip";
        //else if (string.Compare(extension, ".gz", true) == 0)
        //    extension = ".zip";
        //else if (string.Compare(extension, ".tar", true) == 0)
        //    extension = ".zip";
        return await ExtractIconAsync(extension ?? ".");
    }

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
                if (imageList != null)
                {
                    var res = imageList.GetIcon(index, 0x00000001, out var hIcon); // ILD_TRANSPARENT                    

                    if (hIcon != 0)
                    {
                        bitmapSource = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        bitmapSource.Freeze();
                        icons[name] = bitmapSource;
                        return bitmapSource;
                    }
                }
                return null;
            });
        }
        else
            return bitmapSource;
    }

    const int SHIL_SMALL = 0x1; // or SHIL_SYSSMALL

    static Guid IID_IImageList = new("46EB5926-582E-4017-9FDF-E8998DAA0950");

    [DllImport("shell32.dll")]
    static extern int SHGetImageList(int iImageList, ref Guid riid, out IImageList? ppv);

    [ComImport]
    [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IImageList
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
