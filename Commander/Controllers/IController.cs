using System.Collections;
using System.Collections.Immutable;
using System.Windows.Controls;

using Commander.Controls;

namespace Commander.Controllers
{
    interface IController 
    {
        void RemoveAll() { }
        Task<int> Fill(string? path, FolderView folderView);
        void OnSelectionChanged(IList selectedItems, SelectionChangedEventArgs e);
        string? GetCurrentPath(string? parentPath, Item? item);
        void StartResolvingExtendedInfos(Item[] items, FolderViewContext folderViewContext, CancellationToken cancellation) { }
        void SelectAll(Item[] items, Action<IEnumerable<Item>> setSelectedItems) { }
        void CopyItems(FolderView folderView, FolderView targetFolderView, bool move) { }
        void DeleteItems(FolderView folderView) { }
    }
}
