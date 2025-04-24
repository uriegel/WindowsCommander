using System.ComponentModel;
using System.IO;

namespace Commander.Controllers.Directory;

public class ParentItem : INotifyPropertyChanged
{
    public string Name { get; } = "..";

    public event PropertyChangedEventHandler? PropertyChanged;
}

