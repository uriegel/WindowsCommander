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
