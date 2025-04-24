using System.Collections;
using System.Windows.Controls;

using Commander.Controls;

namespace Commander.Controllers
{
    interface IController 
    {
        void RemoveAll();
        Task<int> Fill(string? path, FolderView folderView);
        void OnSelectionChanged(IList selectedItems, SelectionChangedEventArgs e);
        void OnCurrentItemChanged(object? obj);
        string? GetCurrentPath();
    }
}
