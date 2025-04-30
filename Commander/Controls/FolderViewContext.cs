using System.ComponentModel;

using Commander.Controllers;

namespace Commander.Controls;

public class FolderViewContext : INotifyPropertyChanged
{
    public string CurrentPath
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(CurrentPath));
        }
    } = "";

    public string CurrentItemPath
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(CurrentItemPath));
        }
    } = "";

    public string? Restriction
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(Restriction));
        }
    }

    public string? BackgroundAction
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(BackgroundAction));
        }
    }

    public int DirectoriesCount
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(DirectoriesCount));
        }
    }

    public int FilesCount
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(FilesCount));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ColumnViewContext ColumnViewContext { get; set; } = new();

    internal IController? Controller
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(CurrentPath));
        }
    }

    internal void OnChanged() => OnChanged(nameof(CurrentPath));

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}
