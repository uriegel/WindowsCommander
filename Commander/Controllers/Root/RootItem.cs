using System.IO;
using System.Windows.Media;

using Commander.Extensions;

namespace Commander.Controllers.Root;

public record RootItem : Item
{
    public ImageSource? Icon { get; set; }

    public string Description
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(Description));
        }
    } = "";

    public long? Size
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(Size));
        }
    }

    public bool IsMounted
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(IsMounted));
        }
    }

    public static RootItem Create(DriveInfo driveInfo)
        => new()
        {
            Icon = $"Resources/{GetIcon(driveInfo)}.ico".IconFromResource(),
            Name = driveInfo.Name ?? "",
            Description = driveInfo.IsReady ? driveInfo.VolumeLabel : "",
            Size = driveInfo.IsReady ? (long?)driveInfo.TotalSize : null,
            IsMounted = driveInfo.IsReady
        };

    static string GetIcon(DriveInfo driveInfo)
        => driveInfo.Name == @"C:\"
            ? "WindowsDrive"
            : driveInfo.DriveType == DriveType.Removable
            ? "RemovableDrive"
            : "Drive";
}
