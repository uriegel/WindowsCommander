using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Commander.Controls;
using Commander.Controls.ColumnViewHeader;

using CsTools.Extensions;

namespace Commander.Controllers.Directory;

class DirectoryController : IController
{
    #region IController

    public void RemoveAll() { }

    public Task<int> Fill(string? path, FolderView folderView)
    {
        var directoryInfo = new DirectoryInfo(path ?? @"c:\");

        var directories = directoryInfo.GetFiles()
            .OrderBy(n => n.Name)
            .Select(DirectoryItem.Create);

        folderView.ColumnView.ListView.ItemsSource = directories;
        return 0.ToAsync();
    }

    public void OnSelectionChanged(IList selectedItems, SelectionChangedEventArgs e)
    {
        //var selected = e.AddedItems.OfType<Item>();
        //var toRemove = selected.Where(n => n.Name == "@AdvancedKeySettingsNotification.png");
        //ColumnView.ListView.SelectedItems.Remove(toRemove.FirstOrDefault());
    }

    public void OnCurrentItemChanged(object? obj)
        => currentItem = obj as DirectoryItem;

    public string? GetCurrentPath() => currentItem?.Name;

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

    DirectoryItem? currentItem;
}
