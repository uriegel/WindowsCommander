using System.ComponentModel;
using System.Windows;

namespace Commander.Controllers
{
    public class ColumnViewContext : INotifyPropertyChanged
    {
        public GridLength[] ColumnWidths
        {
            get => field;
            set
            {
                field = value;
                OnChanged(nameof(ColumnWidths));
            }
        } = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        void OnChanged(string name) => PropertyChanged?.Invoke(this, new(name));
    }
}
