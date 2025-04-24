using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Commander.Controls;
using Commander.Controls.ColumnViewHeader;

using CsTools.Extensions;

namespace Commander.Controllers.Root;

class RootController : IController
{
    #region IController

    public void RemoveAll() { }
    
    public Task<int> Fill(string? _, FolderView folderView)
    {
        var drives = 
            DriveInfo
                .GetDrives()
                .Select(RootItem.Create)
                .OrderByDescending(n => n.IsMounted)
                .ThenBy(n => n.Name);
        folderView.ColumnView.ListView.ItemsSource = drives;
        return 0.ToAsync();
    }

    public void OnSelectionChanged(IList selectedItems, SelectionChangedEventArgs e)
    {
        //var selected = e.AddedItems.OfType<Item>();
        //var toRemove = selected.Where(n => n.Name == "@AdvancedKeySettingsNotification.png");
        //ColumnView.ListView.SelectedItems.Remove(toRemove.FirstOrDefault());
        selectedItems.Clear();
    }

    public void OnCurrentItemChanged(INotifyPropertyChanged? prop)
        => currentItem = prop as RootItem;

    public string? GetCurrentPath() => currentItem?.Name;

    #endregion

    public RootController(FolderView folderView)
    {
        var ctx = new ColumnViewContext();
        folderView.DataContext = ctx;
        folderView.ColumnView.Headers.ColumnViewContext = ctx;
        folderView.ColumnView.Headers.HeaderItems =
        [
            new HeaderItem("Name"),
            new HeaderItem("Beschreibung"),
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

    RootItem? currentItem;
}

