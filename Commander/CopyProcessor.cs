using ClrWinApi;
using Commander.Controllers;
using CsTools.Extensions;
using WebWindowNetCore;

namespace Commander;

enum SelectedItemsType
{
    None,
    Folder,
    Folders,
    File,
    Files,
    Both
}

class CopyProcessor(string sourcePath, string targetPath, SelectedItemsType selectedItemsType, DirectoryItem[] selectedItems, bool move)
{
    public static CopyProcessor? Current { get; private set; }

    public PrepareCopyResult PrepareCopy()
    {
        Current = this;
        copyItems = MakeCopyItems(MakeSourceCopyItems(selectedItems, sourcePath), targetPath);
        var conflicts = copyItems.Where(n => n.Target != null).ToArray();
        var copySize = copyItems.Aggregate(0L, (s, n) => s + n.Source.Size);
        return new(selectedItemsType, copySize, conflicts);
    }

    public Task<CopyResult> Copy(CopyRequest data)
    {
        try
        {
            Program.Instance.Window?.BeginInvoke(() =>
            {
                copyItems = data.NotOverwrite ? [.. copyItems.Where(n => n.Target == null)] : copyItems;
                var op = new ShFileOPStruct()
                {
                    //Hwnd = Program.Instance.WindowHandle,
                    Flags = FileOpFlags.MULTIDESTFILES | FileOpFlags.ALLOWUNDO,
                    From = string.Join("\0", copyItems.Select(n => sourcePath.AppendPath(n.Source.Name))) + "\0",
                    To = string.Join("\0", copyItems.Select(n => targetPath.AppendPath(n.Source.Name))) + "\0",
                    Func = move ? FileFuncFlags.MOVE : FileFuncFlags.COPY,
                    ProgressTitle = move ? "Verschiebe Dateien" : "Kopiere Dateien"
                };
                // if (!data.NotOverwrite)
                    //op.Flags |= FileOpFlags.NOCONFIRMATION;
                var res = Api.SHFileOperation(op);
            });
            return new CopyResult().ToAsync();
        }
        catch (UnauthorizedAccessException)
        {
            return new CopyResult().ToAsync();
        }
        catch
        {
            return new CopyResult().ToAsync();
        }
        finally
        {
            Current = null;
        }
    }

    protected virtual CopyItem[] MakeCopyItems(IEnumerable<DirectoryItem> items, string targetPath)
        => [.. items.Select(n => CreateCopyItem(n, targetPath))];

    protected virtual CopyItem CreateCopyItem(DirectoryItem source, string targetPath)
    {
        var info = new FileInfo(targetPath.AppendPath(source.Name));
        var target = info.Exists ? DirectoryItem.CreateFileItem(info) : null;
        return new(source, target);
    }

    protected virtual IEnumerable<DirectoryItem> MakeSourceCopyItems(IEnumerable<DirectoryItem> items, string sourcePath)
    {
        var dirs = items
                        .Where(n => n.IsDirectory)
                        .SelectMany(n => Flatten(n.Name, sourcePath));
        var files = items
                        .Where(n => !n.IsDirectory)
                        .SelectFilterNull(n => ValidateFile(n.Name, sourcePath));
        return dirs.Concat(files);
    }

    static IEnumerable<DirectoryItem> Flatten(string item, string sourcePath)
    {
        var info = new DirectoryInfo(sourcePath.AppendPath(item));

        var dirs = info
            .GetDirectories()
            .OrderBy(n => n.Name)
            .Select(n => item.AppendPath(n.Name))
            .SelectMany(n => Flatten(n, sourcePath));

        var files = info
            .GetFiles()
            .OrderBy(n => n.Name)
            .Select(n => DirectoryItem.CreateCopyFileItem(item.AppendPath(n.Name), n));
        return dirs.Concat(files);
    }

    static DirectoryItem? ValidateFile(string subPath, string path)
    {
        var info = new FileInfo(path.AppendPath(subPath));
        if (!info.Exists)
            return null;
        return DirectoryItem.CreateFileItem(info);
    }

    CopyItem[] copyItems = [];
}

record CopyItem(DirectoryItem Source, DirectoryItem? Target);