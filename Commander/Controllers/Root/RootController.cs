
using System.IO;
using System.Windows;

using Commander.Controls;
using Commander.Controls.ColumnViewHeader;

using CsTools.Extensions;

namespace Commander.Controllers.Root;

class RootController : IController
{
    #region IController

    public void RemoveAll() { }
    
    public Task<int> Fill(string path, FolderView folderView)
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
}

