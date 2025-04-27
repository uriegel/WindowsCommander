using System.ComponentModel;
using System.Windows;

namespace Commander.Controls.ColumnViewHeader;

public record HeaderItem(string Name, TextAlignment Alignment = TextAlignment.Left) : INotifyPropertyChanged
{
    public int Index { get; set; }

    public SortType SortType
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(SortType));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

};
