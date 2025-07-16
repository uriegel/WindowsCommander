using System.Runtime.InteropServices;
using ClrWinApi;
using Commander.Controllers;
using CsTools.Extensions;

using static ClrWinApi.Api;

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
        copySize = copyItems.Aggregate(0L, (s, n) => s + n.Source.Size);
        return new(selectedItemsType, copySize, conflicts);
    }

    public async Task<CopyResult> Copy(CopyRequest data)
    {
        try
        {
            if (data.Cancelled)
                return new CopyResult(true);

            ProgressContext.Instance.SetRunning();
            var index = 0;
            copyItems = data.NotOverwrite ? [.. copyItems.Where(n => n.Target == null)] : copyItems;
            copySize = data.NotOverwrite ? copyItems.Sum(n => n.Source.Size) : copySize;
            var cancellation = ProgressContext.Instance.Start(data.Id, move ? "Verschieben" : "Kopieren", copySize, copyItems.Length);
            foreach (var item in copyItems)
            {
                if (cancellation.IsCancellationRequested)
                    throw new TaskCanceledException();
                ProgressContext.Instance.SetNewFileProgress(item.Source.Name, item.Source.Size, ++index);
                await CopyItem(item, move, cancellation);
            }

            if (move)
            {
                var dirs = move ? selectedItems.Where(n => n.IsDirectory).Select(n => n.Name) : [];
                dirs.DeleteEmptyDirectories(sourcePath);
            }
            return new CopyResult(false);
        }
        catch (UnauthorizedAccessException)
        {
            //MainContext.Instance.ErrorText = "Zugriff verweigert";
            return new CopyResult(false);
        }
        catch
        {
            return new CopyResult(false);
        }
        finally
        {
            ProgressContext.Instance.Stop();
            ProgressContext.Instance.SetRunning(false);
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

    protected virtual Task CopyItem(CopyItem item, bool move, CancellationToken cancellation)
    {
        var newFileName = targetPath.AppendPath(item.Source.Name).RemoveWriteProtection();

        var cancel = 0;
        cancellation.Register(() => cancel = -1);
        var res = move
            ? MoveFileWithProgress(sourcePath.AppendPath(item.Source.Name), newFileName, (total, current, c, d, e, f, g, h, i) =>
                {
                    ProgressContext.Instance.SetProgress(total, current);
                    return CopyProgressResult.Continue;
                }, IntPtr.Zero, MoveFileFlags.CopyAllowed | MoveFileFlags.ReplaceExisting)
            : CopyFileEx(sourcePath.AppendPath(item.Source.Name), newFileName, (total, current, c, d, e, f, g, h, i) =>
                {
                    ProgressContext.Instance.SetProgress(total, current);
                    return CopyProgressResult.Continue;
                }, IntPtr.Zero, ref cancel, (CopyFileFlags)0);
        if (!res)
        {
            var error = Marshal.GetLastWin32Error();
            //            if (error == 5)
            //                return Error<Nothing, RequestError>(IOErrorType.AccessDenied.ToError());
            //            else if (error != 0)
            //                return Error<Nothing, RequestError>(IOErrorType.Exn.ToError());
        }

        return 0.ToAsync();
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
    long copySize;
}

record CopyItem(DirectoryItem Source, DirectoryItem? Target);

static partial class Extensions
{
    public static void DeleteEmptyDirectories(this IEnumerable<string> dirs, string path)
    {
        foreach (var dir in dirs)
        {
            var dirToCheck = path.AppendPath(dir);
            if (dirToCheck.IsDirectoryEmpty())
            {
                try
                {
                    System.IO.Directory.Delete(dirToCheck, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Konnte Ausgangsverzeichnis nicht löschen: {e}");
                }
            }
        }
    }

    public static string RemoveWriteProtection(this string file)
        => file.SideEffect(n => {
            var fi = new FileInfo(n);
            if (fi.Exists)
                fi.IsReadOnly = false;
        });    

    static bool IsDirectoryEmpty(this string dir)
    {
        var info = new DirectoryInfo(dir);
        if (info.GetFiles().Length != 0)
            return false;

        var dirs = info
            .GetDirectories()
            .Select(n => n.FullName);
        foreach (var subDir in dirs)
        {
            if (!subDir.IsDirectoryEmpty())
                return false;
        }
        return true;
    }
}