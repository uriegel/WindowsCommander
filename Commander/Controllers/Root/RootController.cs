using System.Collections;
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
        if (folderView.DataContext is FolderViewContext fvc)
        {
            fvc.CurrentPath = "root";
            fvc.DirectoriesCount = 0;
            fvc.FilesCount = 0;
        }

        return 0.ToAsync();
    }

    public void OnSelectionChanged(IList selectedItems, SelectionChangedEventArgs e)
        => selectedItems.Clear();

    public string? GetCurrentPath(string? parentPath, Item? item)
        => item?.Name;

    #endregion

    public RootController(FolderView folderView)
    {
        var ctx = new ColumnViewContext();
        folderView.ColumnView.DataContext = ctx;
        folderView.ColumnView.Headers.ColumnViewContext = ctx;
        folderView.ColumnView.Headers.HeaderItems =
        [
            new HeaderItem("Name"),
            new HeaderItem("Beschreibung"),
            new HeaderItem("Größe", TextAlignment.Right)
        ];
    }

    public void Refresh() { }
}

