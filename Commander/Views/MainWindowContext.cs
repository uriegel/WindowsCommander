using System.ComponentModel;

using Commander.Controls;

namespace Commander.Views;

public class MainWindowContext : INotifyPropertyChanged
{
    public FolderViewContext? ActiveFolderView
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnChanged(nameof(ActiveFolderView));
            }
        }
    } 

    public event PropertyChangedEventHandler? PropertyChanged;
    
    void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}
