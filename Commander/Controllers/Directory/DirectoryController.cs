using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

using ClrWinApi;

using Commander.Controls;
using Commander.Controls.ColumnViewHeader;
using Commander.Extensions;
using Commander.Views;

using CsTools;
using CsTools.Extensions;

using static CsTools.Extensions.Core;

namespace Commander.Controllers.Directory;

class DirectoryController : IController
{
    #region IController

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

    public void SelectAll(Item[] items, Action<IEnumerable<Item>> setSelectedItems)
        => setSelectedItems(items.Where(n => n is not ParentItem));

    #endregion

    public async void StartResolvingExtendedInfos(Item[] items, FolderViewContext folderViewContext, CancellationToken cancellation)
    {
        folderViewContext.BackgroundAction = "Erweiterte Dateiinformationen werden ermittelt...";
        var path = folderViewContext.CurrentPath;
        var exifItems = await Task.Run<ExtendedItem[]>(() =>
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
                {
                    if (exifItem.Exif.HasValue)
                        originalItem.ExifTime = exifItem.Exif.Value;
                    if (exifItem.FileVersion != null)
                        originalItem.FileVersion = exifItem.FileVersion;
                }
            }
        folderViewContext.BackgroundAction = null;
    }

    public DirectoryController(FolderView folderView)
    {
        var ctx = new ColumnViewContext();
        folderView.SetColumnViewContext(ctx);
        folderView.SetHeaders(
        [
            new HeaderItem("Name") { SortType = SortType.Ascending },
            new HeaderItem("Datum"),
            new HeaderItem("Größe", TextAlignment.Right),
            new HeaderItem("Version")
        ]);
    }

    public async void CopyItems(FolderView folderView, string targetPath, Func<Task> refresh, bool move)
    {
        var selectedItems = GetSelectedItems(folderView);
        if (selectedItems.FileCount == 0 && selectedItems.DirCount == 0)
            return;

        var copyItems = selectedItems.Items.SelectFilterNull(CreateCopyItem).ToArray();
        if (!copyItems.Any(n => n.Conflict != null))
        {
            var action = move ? "verschieben" : "kopieren";
            var message = selectedItems.FileCount == 0 && selectedItems.DirCount == 1
                    ? $"Möchtest du das Verzeichnis {action}?"
                    : selectedItems.FileCount == 1 && selectedItems.DirCount == 0
                    ? $"Möchtest du die Datei {action}?"
                    : selectedItems.FileCount > 1 && selectedItems.DirCount == 0
                    ? $"Möchtest du die Dateien {action}?"
                    : selectedItems.FileCount == 0 && selectedItems.DirCount > 1
                    ? $"Möchtest du die Verzeichnisse {action}?"
                    : $"Möchtest du die Elemente {action}?";
            var md = new MessageDialog($"Elemente {action}", message);
            if (md.ShowDialog() != true)
                return;
        }
        else
        {
            var ccd = new CopyConflictDialog(move, [.. copyItems.Where(n => n.Conflict != null)]);
            if (ccd.ShowDialog() != true)
                return;
        }

        var op = new ShFileOPStruct()
        {
            Hwnd = new WindowInteropHelper(Window.GetWindow(folderView)).Handle,
            Flags = FileOpFlags.NOCONFIRMATION | FileOpFlags.MULTIDESTFILES | FileOpFlags.ALLOWUNDO,
            From = string.Join("\0", copyItems.Select(n => folderView.Context.CurrentPath.AppendPath(n.Name))) + "\0",
            To = string.Join("\0", copyItems.Select(n => targetPath.AppendPath(n.Name))) + "\0",
            Func = move ? FileFuncFlags.MOVE : FileFuncFlags.COPY,
            ProgressTitle = move ? "Verschiebe Dateien" : "Kopiere Dateien"
        };
        var res = Api.SHFileOperation(op);
        if (move)
            await folderView.Refresh();
        await refresh();

        CopyItem? CreateCopyItem(Item item)
        {
            if (item is DirectoryItem dirItem)
            {
                var info = new DirectoryInfo(folderView.Context.CurrentPath.AppendPath(dirItem.Name));
                if (info.Exists && new DirectoryInfo(targetPath.AppendPath(dirItem.Name)).Exists)
                    return new CopyItem(dirItem.Name);
                else
                    return null;
            }
            else if (item is FileItem fileItem)
            {
                var info = new FileInfo(folderView.Context.CurrentPath.AppendPath(fileItem.Name));
                if (info.Exists)
                    return new CopyItem(fileItem.Name, info.FullName, info.Length, info.LastWriteTime, fileItem.FileVersion, CreateConflict(fileItem));
            }
            return null;
        }

        Conflict? CreateConflict(FileItem fileItem)
        {
            var info = new FileInfo(targetPath.AppendPath(fileItem.Name));
            if (info.Exists)
                return new Conflict(info.Length, info.LastWriteTime, fileItem.FileVersion);
            return null;
        }
    }

    public async void DeleteItems(FolderView folderView)
    {
        var selectedItems = GetSelectedItems(folderView);
        if (selectedItems.FileCount == 0 && selectedItems.DirCount == 0)
            return;
        var message = selectedItems.FileCount == 0 && selectedItems.DirCount == 1
                ? "Möchtest du das Verzeichnis löschen?"
                : selectedItems.FileCount == 1 && selectedItems.DirCount == 0
                ? "Möchtest du die Datei löschen?"
                : selectedItems.FileCount > 1 && selectedItems.DirCount == 0
                ? "Möchtest du die Dateien löschen?"
                : selectedItems.FileCount == 0 && selectedItems.DirCount > 1
                ? "Möchtest du die Verzeichnisse löschen?"
                : "Möchtest du die Elemente löschen?";
        var md = new MessageDialog("Elemente löschen", message);
        if (md.ShowDialog() == true)
        {
            var files = selectedItems.Items.Select(n => n.Name).ToArray();
            var op = new ShFileOPStruct()
            {
                Hwnd = new WindowInteropHelper(Window.GetWindow(folderView)).Handle,
                Flags = FileOpFlags.NOCONFIRMATION | FileOpFlags.ALLOWUNDO,
                From = string.Join("\0", files.Select(folderView.Context.CurrentPath.AppendPath)) + "\0",
                Func = FileFuncFlags.DELETE,
                ProgressTitle = "Elemente löschen"
            };
            var res = Api.SHFileOperation(op);
            await folderView.Refresh();
        }
    }

    SelectedItems GetSelectedItems(FolderView folderView)
    {
        (var currentItem, var selectedItems) = folderView.GetSelectedItems();
        var files = selectedItems
                        .OfType<Item>()
                        .Where(n => n is not ParentItem)
                        .ToArray();
        if (files.Length == 0 && currentItem != null)
            files = [currentItem];
        var filesCount = files.Count(n => n is FileItem);
        var dirCount = files.Count(n => n is DirectoryItem);
        return new SelectedItems(files, dirCount, filesCount);
    }

    static ExtendedItem? GetExifItem(Item item, int idx, string path, CancellationToken token)
    {
        if (item is FileItem fileItem && !token.IsCancellationRequested)
        {
            var exif = fileItem.Name.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                        || fileItem.Name.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                        || fileItem.Name.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                ? ExifReader.GetExifData(path.AppendPath(fileItem.Name))?.DateTime
                : null;
            var version = fileItem.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)
                        || fileItem.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase)
                ? GetVersion(path.AppendPath(fileItem.Name))
                : null;
            return exif.HasValue || version != null 
                ? new(idx, exif, version) 
                : null;
        }
        else
            return null;
    }

    static FileVersion? GetVersion(string file)
        => file.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)
            ? FileVersionInfo
                    .GetVersionInfo(file)
                    .MapVersion()
            : null;
}

record ExtendedItem(int Index, DateTime? Exif, FileVersion? FileVersion);
public record SelectedItems(Item[] Items, int DirCount, int FileCount);
public record CopyItem(long? Size, DateTime? Date, FileVersion? Version, Conflict? Conflict) : Item
{
    public CopyItem(string name)
        : this(null, null, null, new Conflict(null, null, null))
    {
        Name = name;
        Icon = "Resources/Folder.ico".IconFromResource();
    }

    public CopyItem(string name, string iconPath, long Size, DateTime Date, FileVersion? Version, Conflict? Conflict)
        : this(Size, Date, Version, Conflict)
    {
        Name = name;
        IconPath = iconPath;
    }
    public string? IconPath
    {
        get => field;
        set
        {
            OnChanged(nameof(Icon));
            field = value;
        }
    }

    public ImageSource? Icon
    {
        get
        {
            if (field == null)
            {
                GetIcon();
                async void GetIcon()
                {
                    var bitmapSource = await FileIcon.Get(IconPath);
                    if (bitmapSource != null)
                    {
                        field = bitmapSource;
                        OnChanged(nameof(Icon));
                    }
                }
            }
            return field;
        }
        set
        {
            OnChanged(nameof(Icon));
            field = value;
        }
    }

    public bool IsNewer { get => Conflict != null && Date > Conflict.Date; }
    public bool IsOlder { get => Conflict != null && Date < Conflict.Date; }
    public bool IsEqualSize { get => Conflict != null && Size == Conflict.Size; }
}
public record Conflict(long? Size, DateTime? Date, FileVersion? Version);
