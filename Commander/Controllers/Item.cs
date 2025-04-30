using System.ComponentModel;

namespace Commander.Controllers;

public record Item : INotifyPropertyChanged
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

    public bool IsHidden { get; init; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}
