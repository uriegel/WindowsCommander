using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Commander.Controls;
using Commander.Controls.ColumnViewHeader;

using CsTools;
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
                        .FirstOrDefault(n => n.Item.Name == folderView.Context.CurrentPath.SubstringAfterLast('\\'));
        folderView.Context.CurrentPath = directoryInfo.FullName;
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

    public async void StartResolvingExtendedInfos(Item[] items, FolderViewContext folderViewContext, CancellationToken cancellation)
    {
        folderViewContext.BackgroundAction = "Erweiterte Dateiinformationen werden ermittelt...";
        var path = folderViewContext.CurrentPath;
        var exifItems = await Task.Run<ExifItem[]>(() =>
        {
            try
            {
                return [.. items.SelectFilterNull((n, i) => GetExifItem(n, i, path, cancellation))];
            }
            catch { return []; }
        });
        if (!cancellation.IsCancellationRequested)
            foreach (var exifItem in exifItems)
            {
                if (items[exifItem.Index] is FileItem originalItem)
                    originalItem.ExifTime = exifItem.Exif;
            }
        folderViewContext.BackgroundAction = null;
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

    static ExifItem? GetExifItem(Item item, int idx, string path, CancellationToken token)
    {
        if (item is FileItem fileItem
            && !token.IsCancellationRequested
            && (fileItem.Name.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                || fileItem.Name.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                || fileItem.Name.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)))
        {
            var exif = ExifReader.GetExifData(path.AppendPath(fileItem.Name))?.DateTime;
            return exif.HasValue ? new(idx, exif.Value) : null;
        }
        else
            return null;
    }
}

record ExifItem(int Index, DateTime Exif);