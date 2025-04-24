using System.ComponentModel;
using System.IO;

namespace Commander.Controllers.Directory;

public class DirectoryItem : INotifyPropertyChanged
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

    public static DirectoryItem Create(FileInfo info)
        => new()
        {
            Name = info.FullName ?? "",
            DateTime = info.LastWriteTime,
            Size = info.Length
        };

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}

