using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CsTools;
using CsTools.Extensions;
using WebServerLight;
using Commander.Controllers;

using static Commander.Controllers.FolderController;
using static ClrWinApi.Api;
using static CsTools.Core;
using ClrWinApi;
using CsTools.Async;
using Commander.ProgressAction;
using Commander.Enums;

namespace Commander;

static class Requests
{
    public static async Task<bool> Process(IRequest request)
    {
        return request.SubPath switch
        {
            "changepath" => await ChangePath(request),
            "sendmenucmd" => await MenuCmd(request),
            "getextended" => await GetExtended(request),
            "preparecopy" => await PrepareCopy(request),
            "copy" => await Copy(request),
            "cancelcopy" => await CancelCopy(request),
            "onenter" => await OnEnter(request),
            "startuac" => await StartUac(request),
            "stopuac" => await StopUac(request),
            "setcontroller" => await SetController(request),
            "deleteitems" => await DeleteItems(request),
            "rename" => await Rename(request),
            "createfolder" => await CreateFolder(request),
            "connectshare" => await ConnectShare(request),
            _ => false
        };
    }

    public static async Task<bool> GetIconFromName(IRequest request)
    {
        var iconfile = Resources.Get(request.SubPath ?? "emtpy");
        var stream = new MemoryStream();
        iconfile!.CopyTo(stream);
        stream.Position = 0;
        request.AddResponseHeader("Expires", (DateTime.UtcNow + TimeSpan.FromHours(1)).ToString("r"));
        await request.SendAsync(stream, stream.Length, "image/png");
        return true;
    }

    public static async Task<bool> GetIconFromExtension(IRequest request)
    {
        var stream = await GetIconStream(
            request.SubPath?.Replace('/', '\\') ?? ".default");
        request.AddResponseHeader("Expires", (DateTime.UtcNow + TimeSpan.FromHours(1)).ToString("r"));
        await request.SendAsync(stream, stream.Length, "image/png");
        return true;
    }

    public static async Task<bool> GetFile(IRequest request)
    {
        if (request.SubPath == null)
            return false;
        using var pic = File.OpenRead(request.SubPath.Replace('/', '\\'));
        if (pic != null)
        {
            await request.SendAsync(pic, pic.Length, MimeType.Get(request.SubPath.GetFileExtension()) ?? MimeTypes.ImageJpeg);
            return true;
        }
        else
            return false;
    }

    public static async Task<bool> GetTrack(IRequest request)
    {
        if (request.SubPath == null)
            return false;
        var track = TrackInfo.Get("/" + request.SubPath);
        await request.SendJsonAsync(track);
        return true;
    }
    
    static Task<bool> ChangePath(IRequest request)
        => Request<ChangePathRequest, ChangePathResult>(request, n =>
        {
            DetectController(n.Id, n.Path);
            return GetController(n.Id).ChangePathAsync(n.Path, n.ShowHidden);
        });

    static Task<bool> MenuCmd(IRequest request)
        => Request<SendMenuCmdRequest, SendMenuCmdResponse>(request, n =>
        {
            if (n.Cmd == "SHOW_DEV_TOOLS")
                Program.Instance.WebView.ShowDevTools();
            return new SendMenuCmdResponse().ToAsync();
        });

    static Task<bool> GetExtended(IRequest request)
        => Request<GetExtendedRequest, GetExtendedResult>(request, n => GetController(n.FolderId).GetExtended(n.Id));

    static Task<bool> PrepareCopy(IRequest request)
        => Request<PrepareCopyRequest, PrepareCopyResult>(request, n => GetController(n.Id).PrepareCopy(n));

    static Task<bool> Copy(IRequest request)
        => Request<CopyRequest, CopyResult>(request, n => GetController(n.Id).Copy(n));

    static Task<bool> CancelCopy(IRequest request)
        => Request<NilRequest, NilResponse>(request, async _ =>
        {
            await ProgressContext.Cancel();
            return new NilResponse();
        });

    static Task<bool> StartUac(IRequest request)
        => Request<NilRequest, StartUacResponse>(request, async _ =>
        {
            var res = await UacServer.Start();
            return new StartUacResponse(res);
        });

    static Task<bool> StopUac(IRequest request)
        => Request<NilRequest, NilResponse>(request, _ =>
        {
            UacServer.Stop();
            return new NilResponse().ToAsync();
        });

    static Task<bool> SetController(IRequest request)
        => Request<SetControllerRequest, SetControllerResponse>(request, UacServer.SetController);
    
    static Task<bool> DeleteItems(IRequest request)
        => Request<DeleteItemsRequest, DeleteItemsResponse>(request, n => GetController(n.Id).DeleteItems(n));

    static Task<bool> Rename(IRequest request)
        => Request<RenameRequest, RenameResponse>(request, n => GetController(n.Id).Rename(n));

    static Task<bool> CreateFolder(IRequest request)
        => Request<CreateFolderRequest, CreateFolderResponse>(request, n => GetController(n.Id).CreateFolder(n));
    
    static Task<bool> ConnectShare(IRequest request)
        => Request<ConnectShareRequest, ConnectShareResponse>(request, DirectoryController.ConnectShare);
    
    static Task<bool> OnEnter(IRequest request)
                => Request<OnEnterRequest, OnEnterResult>(request, n => GetController(n.Id).OnEnter(n));

    static Task<Stream> GetIconStream(string iconHint)
        => Try(() => iconHint.Contains('\\')
            ? (Icon.ExtractAssociatedIcon(iconHint)?.Handle ?? 0).ToAsync()
            : RepeatOnException(() =>
                {
                    var shinfo = new ShFileInfo();
                    var handle = SHGetFileInfo(iconHint, ClrWinApi.FileAttributes.Normal, ref shinfo, Marshal.SizeOf(shinfo),
                        SHGetFileInfoConstants.ICON | SHGetFileInfoConstants.SMALLICON | SHGetFileInfoConstants.USEFILEATTRIBUTES | SHGetFileInfoConstants.TYPENAME);
                    return shinfo.IconHandle != IntPtr.Zero
                        ? shinfo.IconHandle.ToAsync()
                        : throw new Exception("Not found");
                }, 3, TimeSpan.FromMilliseconds(40)), _ => Icon.ExtractAssociatedIcon(@"C:\Windows\system32\SHELL32.dll")!.Handle)
            ?.Select(handle =>
                {
                    using var icon = Icon.FromHandle(handle);
                    using var bitmap = icon.ToBitmap();
                    var ms = new MemoryStream();
                    bitmap.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    DestroyIcon(handle);
                    return ms as Stream;
                })
            ?? (new MemoryStream() as Stream).ToAsync();
    
    static async Task<bool> Request<TRequest, TResponse>(IRequest request, Func<TRequest, Task<TResponse>> onRequest)
        where TResponse: class
    {
        var data = await request.DeserializeAsync<TRequest>();
        if (data != null)
        {
            var response = await onRequest(data);
            await request.SendJsonAsync(response, response.GetType());
        }
        return true;
    }
}
record ChangePathRequest(
    string Id,
    string Path,
    bool Mount,
    bool ShowHidden

);
record ChangePathResult(
    bool? Cancelled,
    bool? AccessDenied,
    int Id,
    string? Controller,
    string Path,
    int DirCount,
    int FileCount
);

record PrepareCopyRequest(
    string Id,
    string Path,
    string TargetPath,
    bool Move,
    DirectoryItem[] Items
);

record PrepareCopyResult(
    SelectedItemsType SelectedItemsType,
    long TotalSize,
    CopyItem[] Conflicts,
    bool? AlreadyRunning = null
);

record CopyRequest(
    string Id,
    bool Cancelled,
    bool NotOverwrite

);
record CopyResult(bool Cancelled, bool? AccessDenied = null);

record DeleteItemsRequest(string Id, string Path, string[] Items);
record DeleteItemsResponse(bool? Error = null, bool? AccessDenied = null);

record ConnectShareRequest(string Name, string Password, string Share);
record ConnectShareResponse(bool Success);

record RenameRequest(
    string Id,
    string Path,
    string Item,
    string NewName,
    bool? AsCopy
);

record RenameResponse(bool? Error);

record CreateFolderRequest(
    string Id,
    string Path,
    string NewName
);

record CreateFolderResponse(bool? Error);

record NilRequest();
record NilResponse();

record StartUacResponse(bool Success);

record SetControllerRequest(
    string Id,
    string? Path
);

record SetControllerResponse(bool? Cancelled = null);

record GetExtendedRequest(int Id, string FolderId);
record GetExtendedResult();

record ViewItem(
    string Name,
    long? Size,
    bool? IsParent,
    bool? IsDirectory
);

record SendMenuCmdRequest(string Cmd);
record SendMenuCmdResponse();

record OnEnterRequest(
    string Id,
    string Path,
    string Name,
    bool Ctrl,
    bool Alt
);

record OnEnterResult(bool Success);
