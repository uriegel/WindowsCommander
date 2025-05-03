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

    public Task<int> Fill(string? _, FolderView folderView)
    {
        var drives = 
            DriveInfo
                .GetDrives()
                .Select(RootItem.Create)
                .OrderByDescending(n => n.IsMounted)
                .ThenBy(n => n.Name);
        folderView.SetItemsSource(drives);
        folderView.Context.CurrentPath = "root";

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
        folderView.SetColumnViewContext(ctx);
        folderView.SetHeaders(
        [
            new HeaderItem("Name") { SortType = SortType.Disabled },
            new HeaderItem("Beschreibung") { SortType = SortType.Disabled },
            new HeaderItem("Größe", TextAlignment.Right) { SortType = SortType.Disabled }
        ]);
    }

    public void Refresh() { }
}

