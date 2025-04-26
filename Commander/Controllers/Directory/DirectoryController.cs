using System.Collections;
using System.ComponentModel;
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
            .Select(DirectoryItem.Create);
        var files = directoryInfo.GetFiles()
            .OrderBy(n => n.Name)
            .Select(FileItem.Create);

        folderView.ColumnView.ListView.ItemsSource =
            ConcatEnumerables(
                [new ParentItem() as Item],
                directories,
                files);
        var oldPos = folderView.ColumnView
                        .ListView
                        .Items
                        .Cast<Item>()
                        .Index()
                        .FirstOrDefault(n => n.Item.Name == (folderView.DataContext as FolderViewContext)?.CurrentPath?.SubstringAfterLast('\\'));
        if (folderView.DataContext is FolderViewContext fvc)
            fvc.CurrentPath = directoryInfo.FullName;
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
        folderView.ColumnView.DataContext = ctx;
        folderView.ColumnView.Headers.ColumnViewContext = ctx;
        folderView.ColumnView.Headers.HeaderItems =
        [
            new HeaderItem("Name"),
            new HeaderItem("Datum"),
            new HeaderItem("Größe", TextAlignment.Right)
        ];
    }

    public void Execute(string command)
    {
        //throw new NotImplementedException();
    }

    public void Refresh()
    {
        //throw new NotImplementedException();
    }

    public void Dispose()
    {
        //throw new NotImplementedException();
    }
}
