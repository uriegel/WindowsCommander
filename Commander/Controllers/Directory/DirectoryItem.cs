using System.ComponentModel;
using System.IO;
using System.Windows.Media;

using Commander.Extensions;

namespace Commander.Controllers.Directory;

public class DirectoryItem : INotifyPropertyChanged
{
    public ImageSource? Icon { get; set; }

    public string Name
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(Name));
        }
    } = "";

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
            Name = info.Name ?? "",
            DateTime = info.LastWriteTime
        };

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}

