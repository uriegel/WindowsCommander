using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;

using Commander.Controls;

namespace Commander.Controllers
{
    interface IController 
    {
        void RemoveAll();
        Task<int> Fill(string? path, FolderView folderView);
        void OnSelectionChanged(IList selectedItems, SelectionChangedEventArgs e);
        void OnCurrentItemChanged(INotifyPropertyChanged? obj);
        string? GetCurrentPath();
    }
}
