using System.ComponentModel;
using System.IO;

namespace Commander.Controllers.Directory;

public class FileItem : Item
{
    public string? IconPath { get; init; }

    public DateTime DateTime
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(DateTime));
        }
    }

    public long? Size
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
            IsHidden = info.Attributes.HasFlag(FileAttributes.Hidden),
            IconPath = info.Name?.EndsWith("exe", StringComparison.InvariantCultureIgnoreCase) == true
                        ? info.FullName 
                        : info.Name ?? "",
            DateTime = info.LastWriteTime,
            Size = info.Length
        };
}

