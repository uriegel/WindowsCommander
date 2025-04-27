using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Commander.Controls;
using Commander.Controls.ColumnViewHeader;

using CsTools.Extensions;

using static CsTools.Extensions.Core;

namespace Commander.Controllers.Directory;

class DirectoryController : IController
{
    #region IController

    public void RemoveAll() { }

    public Task<int> Fill(string? path, FolderView folderView)
    {
        var directoryInfo = new DirectoryInfo(path ?? @"c:\");

        var directories = directoryInfo.GetDirectories()
            .OrderBy(n => n.Name)
            .Select(DirectoryItem.Create)
            .ToArray();
        var files = directoryInfo.GetFiles()
            .OrderBy(n => n.Name)
            .Select(FileItem.Create)
            .ToArray();

        folderView.SetItemsSource(
            ConcatEnumerables(
                [new ParentItem() as Item],
                directories,
                files));
        var oldPos = folderView
                        .GetItems()
                        .Index()
                        .FirstOrDefault(n => n.Item.Name == (folderView.DataContext as FolderViewContext)?.CurrentPath?.SubstringAfterLast('\\'));
        if (folderView.DataContext is FolderViewContext fvc)
        {
            fvc.CurrentPath = directoryInfo.FullName;
            fvc.DirectoriesCount = directories.Length;
            fvc.FilesCount = files.Length;
        }
        return oldPos.Index.ToAsync();
    }

    public void OnSelectionChanged(IList selectedItems, SelectionChangedEventArgs e)
    {
        var selected = e.AddedItems.OfType<Item>();
        var toRemove = selected.FirstOrDefault(n => n is ParentItem);
        if (toRemove != null)
            selectedItems.Remove(toRemove);
    }

    public string? GetCurrentPath(string? parentPath, Item? item)
    {
        if (item is ParentItem && parentPath?.Length == 3)
            return "root";
        else 
            return parentPath.AppendPath(item is FileItem fi
                ? fi.Name
                : item is DirectoryItem di
                ? di.Name
                : item is ParentItem
                ? ".."
                : null);
    }

    #endregion

    public DirectoryController(FolderView folderView)
    {
        var ctx = new ColumnViewContext();
        folderView.SetColumnViewContext(ctx);
        folderView.SetHeaders(
        [
            new HeaderItem("Name") { SortType = SortType.Ascending },
            new HeaderItem("Datum"),
            new HeaderItem("Größe", TextAlignment.Right)
        ]);
    }

    public void Refresh()  { }
}
