using System.ComponentModel;
using System.IO;

namespace Commander.Controllers.Root;

public class RootItem : INotifyPropertyChanged
{
    public string Name
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(Name));
        }
    } = "";

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

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}
