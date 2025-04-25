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
                [new ParentItem() as INotifyPropertyChanged],
                directories,
                files);
        currentPath = directoryInfo.FullName;
        return 0.ToAsync();
    }

    public void OnSelectionChanged(IList selectedItems, SelectionChangedEventArgs e)
    {
        var selected = e.AddedItems.OfType<INotifyPropertyChanged>();
        var toRemove = selected.FirstOrDefault(n => n is ParentItem);
        if (toRemove != null)
            selectedItems.Remove(toRemove);
    }

    public void OnCurrentItemChanged(INotifyPropertyChanged? prop)
        => currentItem = prop;

    public string? GetCurrentPath()
    {
        if (currentItem is ParentItem && currentPath?.Length == 3)
            return "root";
        else 
            return currentPath?.AppendPath(currentItem is FileItem fi
                ? fi.Name
                : currentItem is DirectoryItem di
                ? di.Name
                : currentItem is ParentItem
                ? ".."
                : null);
    }

    #endregion

    public DirectoryController(FolderView folderView)
    {
        var ctx = new ColumnViewContext();
        folderView.DataContext = ctx;
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

    INotifyPropertyChanged? currentItem;
    string? currentPath;
}
