using System.ComponentModel;

using Commander.Controls;

namespace Commander.Views;

public class MainWindowContext : INotifyPropertyChanged
{
    public static MainWindowContext Instance { get; private set; } = new();
    
    public FolderViewContext? ActiveFolderContext
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(ActiveFolderContext));
            }
        }
    }

    public bool ShowHidden
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(ShowHidden));
            }
        }
    }

    public MainWindowContext() => Instance = this;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}
