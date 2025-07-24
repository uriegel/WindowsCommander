using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Commander.Enums;
using Commander.ProgressAction;
using CsTools.Extensions;

using static System.Console;
using static ClrWinApi.Api;

namespace Commander.Controllers;

class DirectoryController(string folderId) : Controller(folderId)
{
    public override string Id { get; } = "DIRECTORY";

    public override async Task<ChangePathResult> ChangePathAsync(string path, bool showHidden)
    {
        var changePathId = Interlocked.Increment(ref ChangePathSeed);
        try
        {
            var cancellation = Cancellations.ChangePathCancellation(FolderId);
            var result = await Task.Run(() => GetFiles(path, showHidden, changePathId, cancellation));
            GetExtendedInfo(changePathId, result.Items, path, cancellation);
            return result;
        }
        catch (UnauthorizedAccessException uae)
        {
            OnError(uae);
            return new ChangePathResult(false, true, changePathId, null, "", 0, 0);
        }
        catch (DirectoryNotFoundException dnfe)
        {
            OnError(dnfe);
            // TODO: Show error in status bar
            return new ChangePathResult(true, null, changePathId, null, "", 0, 0);
        }
        // catch (RequestException re) when (re.CustomRequestError == CustomRequestError.ConnectionError)
        // {
        //     OnError(re);
        //     MainContext.Instance.ErrorText = "Die Verbindung zum Gerät konnte nicht aufgebaut werden";
        //     return new ChangePathResult(true, changePathId, null, "", 0, 0);
        // }
        // catch (RequestException re) when (re.CustomRequestError == CustomRequestError.NameResolutionError)
        // {
        //     OnError(re);
        //     MainContext.Instance.ErrorText = "Der Netzwerkname des Gerätes konnte nicht ermittelt werden";
        //     return new ChangePathResult(true, changePathId, null, "", 0, 0);
        // }
        catch (OperationCanceledException)
        {
            return new ChangePathResult(true, null, changePathId, null, "", 0, 0);
        }
        catch (Exception e)
        {
            OnError(e);
            // TODO: Show error in status bar
            return new ChangePathResult(true, null, changePathId, null, "", 0, 0);
        }

        static void OnError(Exception e) => Error.WriteLine($"Konnte Pfad nicht ändern: {e}");
    }

    public override Task<PrepareCopyResult> PrepareCopy(PrepareCopyRequest data)
    {
        //if ((data.TargetPath.StartsWith('/') != true && data.TargetPath?.StartsWith("remote/") != true)
        if (data.TargetPath.Length < 1
        || (data.TargetPath[1] != ':' && !data.TargetPath.StartsWith('\\'))
        || string.Compare(data.Path, data.TargetPath, StringComparison.CurrentCultureIgnoreCase) == 0
        || data.Items.Length == 0)
            return new PrepareCopyResult(SelectedItemsType.None, 0, []).ToAsync();
        var copyProcessor = new CopyProcessor(data.Path, data.TargetPath, GetSelectedItemsType(data.Items), data.Items, data.Move);
        return Task.Run(copyProcessor.PrepareCopy);
    }

    public override Task<CopyResult> Copy(CopyRequest copyRequest) => CopyProcessor.Current?.Copy(copyRequest) ?? new CopyResult(true).ToAsync();

    public override Task<DeleteItemsResult> DeleteItems(DeleteItemsRequest request)
    {
        var res = SHFileOperation(new ClrWinApi.ShFileOPStruct
        {
            Func = ClrWinApi.FileFuncFlags.DELETE,
            From = string.Join("\U00000000", request.Items.Select(request.Path.AppendPath))
                            + "\U00000000\U00000000",
            Flags = ClrWinApi.FileOpFlags.NOCONFIRMMKDIR
                    | ClrWinApi.FileOpFlags.NOERRORUI
                    | ClrWinApi.FileOpFlags.ALLOWUNDO
        });
        return new DeleteItemsResult(res != 0 ? true : null, res == 0x78 ? true : null).ToAsync();
    }

    public override Task<OnEnterResult> OnEnter(OnEnterRequest data)
    {
        var path = data.Path.AppendPath(data.Name);
        if (data.Alt || data.Ctrl)
        {
            var info = new ClrWinApi.ShellExecuteInfo();
            info.Size = Marshal.SizeOf(info);
            info.Verb = data.Alt == true ? "properties" : "openas";
            info.File = path;
            info.Show = ClrWinApi.ShowWindowFlag.Show;
            info.Mask = ClrWinApi.ShellExecuteFlag.InvokeIDList;
            ClrWinApi.Api.ShellExecuteEx(ref info);
        }
        else
        {
            using var proc = new Process()
            {
                StartInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = true,
                },
            };

            proc.Start();
        }
        return new OnEnterResult(true).ToAsync();
    }

    public override Task<GetExtendedResult> GetExtended(int id)
    {
        if (extendedTasks.TryGetValue(id, out var tcs))
            tcs.TrySetResult();
        return new GetExtendedResult().ToAsync();
    }

    public static SelectedItemsType GetSelectedItemsType(DirectoryItem[] items)
    {
        var dirs = items.Count(n => n.IsDirectory);
        var files = items.Count(n => !n.IsDirectory);
        return dirs > 1 && files == 0
            ? SelectedItemsType.Folders
            : dirs == 0 && files > 1
            ? SelectedItemsType.Files
            : dirs == 1 && files == 0
            ? SelectedItemsType.Folder
            : dirs == 0 && files == 1
            ? SelectedItemsType.File
            : dirs + files > 0
            ? SelectedItemsType.Both
            : SelectedItemsType.None;
    }

    GetFilesResult GetFiles(string path, bool showHidden, int changePathId, CancellationToken cancellation)
    {
        var info = new DirectoryInfo(path);
        cancellation.ThrowIfCancellationRequested();
        return MakeFilesResult(new DirFileInfo(
                    [.. info
                        .GetDirectories()
                        .Where(n => showHidden || (n.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        .OrderBy(n => n.Name)
                        .Select(DirectoryItem.CreateDirItem)],
                    [.. info
                        .GetFiles()
                        .Where(n => showHidden || (n.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        .OrderBy(n => n.Name)
                        .Select(DirectoryItem.CreateFileItem)]));

        GetFilesResult MakeFilesResult(DirFileInfo dirFileInfo)
            => new(null, changePathId, CheckInitial() ? Id : null, info.FullName, dirFileInfo.Directories.Length, dirFileInfo.Files.Length, [
                DirectoryItem.CreateParentItem(),
                .. dirFileInfo.Directories,
                .. dirFileInfo.Files]);
    }

    async void GetExtendedInfo(int changePathId, DirectoryItem[] items, string path, CancellationToken cancellation)
    {
        try
        {
            var extendedReady = WaitForExtendedDataRequest(changePathId);
            bool changed = false;
            await Task.Run(async () =>
            {
                Events.SendStatusBarInfo(FolderId, changePathId, "Ermittle erweiterte Informationen...");
                foreach (var item in items
                                        .Where(item => !cancellation.IsCancellationRequested
                                                && (item.Name.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                                                    || item.Name.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                                                    || item.Name.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))))
                {
                    cancellation.ThrowIfCancellationRequested();
                    item.ExifData = ExifReader.GetExifData(path.AppendPath(item.Name));
                    if (item.ExifData != null)
                        changed = true;
                }
                foreach (var item in items
                                        .Where(item => !cancellation.IsCancellationRequested
                                                && (item.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase)
                                                    || item.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))))
                {
                    cancellation.ThrowIfCancellationRequested();
                    item.FileVersion = GetVersion(path.AppendPath(item.Name));
                    if (item.FileVersion != null)
                        changed = true;
                }
                if (changed)
                {
                    await extendedReady;

                    Events.SendExtendedInfo(FolderId, changePathId, items);
                }
            }, cancellation);
        }
        catch { }
        finally
        {
            Events.SendStatusBarInfo(FolderId, changePathId, null);
        }
    }

    static Task WaitForExtendedDataRequest(int requestId)
    {
        var tcs = new TaskCompletionSource();
        var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        cancellation.Token.Register(() => tcs.TrySetCanceled());
        extendedTasks.AddOrUpdate(requestId, tcs, (id, t) => tcs);
        return tcs.Task;
    }

    static FileVersion? GetVersion(string file)
        => file.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)
            ? FileVersionInfo
                    .GetVersionInfo(file)
                    .MapVersion()
            : null;

    public static int ChangePathSeed = 0;

    static readonly ConcurrentDictionary<int, TaskCompletionSource> extendedTasks = [];
}

record DirectoryItem(
    string Name,
    string? Icon,
    long Size,
    bool IsDirectory,
    bool IsParent,
    bool IsHidden,
    DateTime? Time
)
{
    public ExifData? ExifData { get; set; }
    public FileVersion? FileVersion { get; set; }

    public static DirectoryItem CreateParentItem()
        => new(
            "..",
            null,
            -1,
            true,
            true,
            false,
            null);

    public static DirectoryItem CreateDirItem(DirectoryInfo info)
        => new(
            info.Name,
            null,
            -1,
            true,
            false,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);

    public static DirectoryItem CreateFileItem(FileInfo info)
        => new(
            info.Name,
            GetIcon(info),
            info.Length,
            false,
            false,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);

    public static DirectoryItem CreateCopyFileItem(string name, FileInfo info)
        => new(
            name,
            GetIcon(info),
            info.Length,
            false,
            false,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);

    static string? GetIcon(FileInfo info) =>
        info.Name.EndsWith("exe", StringComparison.InvariantCultureIgnoreCase)
        ? info.FullName
        : info.Name.GetFileExtension();
};

public record FileVersion(int Major, int Minor, int Build, int Patch);

record DirFileInfo(
    DirectoryItem[] Directories,
    DirectoryItem[] Files
);

record GetFilesResult(
    bool? Cancelled,
    int Id,
    string? Controller,
    string Path,
    int DirCount,
    int FileCount,
    DirectoryItem[] Items
)
    : ChangePathResult(Cancelled, null, Id, Controller, Path, DirCount, FileCount);

static class FileVersionInfoExtensions
{
    public static FileVersion? MapVersion(this FileVersionInfo? info)
           => info != null
               ? new(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart)
               : null;
}
