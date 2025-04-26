using System.ComponentModel;
using System.IO;
using System.Windows.Media;

using Commander.Extensions;

namespace Commander.Controllers.Directory;

public class DirectoryItem : Item
{
    public ImageSource? Icon { get; set; }

    public DateTime DateTime
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(DateTime));
        }
    }

    public static DirectoryItem Create(DirectoryInfo info)
        => new()
        {
            Icon = "Resources/Folder.ico".IconFromResource(),
            IsHidden = info.Attributes.HasFlag(FileAttributes.Hidden),
            Name = info.Name ?? "",
            DateTime = info.LastWriteTime
        };
}

