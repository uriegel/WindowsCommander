using System.IO;

namespace Commander.Controllers.Directory;

public record FileItem : Item
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
            IsHidden = info.Attributes.HasFlag(FileAttributes.Hidden),
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