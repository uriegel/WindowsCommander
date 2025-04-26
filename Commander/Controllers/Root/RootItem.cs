using System.IO;

namespace Commander.Controllers.Root;

public class RootItem : Item
{
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
            Name = driveInfo.Name ?? "",
            Description = driveInfo.IsReady ? driveInfo.VolumeLabel : "",
            Size = driveInfo.IsReady ? (long?)driveInfo.TotalSize : null,
            IsMounted = driveInfo.IsReady
        };
}
