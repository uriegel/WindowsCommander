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
        var stream = await GetIconStream(request.SubPath ?? ".default");
        request.AddResponseHeader("Expires", (DateTime.UtcNow + TimeSpan.FromHours(1)).ToString("r"));
        await request.SendAsync(stream, stream.Length, "image/png");
        return true;
    }

    public static async Task<bool> GetFile(IRequest request)
    {
        if (request.SubPath == null)
            return false;
        using var pic = File.OpenRead(request.SubPath);
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
    
    public static void WebSocket(IWebSocket webSocket)
        => Requests.webSocket = webSocket;

    public static async void SendMenuCommand(string id)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("cmd", new(id)));    
    }

    public static async void SendMenuCheck(string id, bool check)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("cmdtoggle", null, new(id, check))); 
    }

    public static async void SendStatusBarInfo(string id, int requestId, string? text)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("status", StatusMsg: new(id, requestId, text))); 
    }

    public static async void SendExtendedInfo(string id, int requestId, DirectoryItem[] items)
    {
        try
        {
            if (webSocket != null)
                await webSocket.SendJson(new WebSocketMsg("extendedinfo", ExtendedInfo: new(id, requestId, items)));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sending extended info: {ex}");
        }
    }

    public static async void SendProgressRevealed(bool revealed)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("progressrevealed", ProgressRevealed: revealed)); 
    }

    public static async void SendCopyProgress(CopyProgress copyProgress)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("copyprogress", CopyProgress: copyProgress)); 
    }

    public static async void SendProgressRunning()
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("progressrunning", ProgressRunning: true)); 
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
        => Request<CancelCopyRequest, CancelCopyResult>(request, _ =>
        {
            ProgressContext.Cancel();
            return new CancelCopyResult().ToAsync();
        });

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

    static IWebSocket? webSocket;
}
record ChangePathRequest(
    string Id,
    string Path,
    bool Mount,
    bool ShowHidden

);
record ChangePathResult(
    bool? Cancelled,
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
record CopyResult(bool Cancelled);

record CancelCopyRequest();
record CancelCopyResult();

record GetExtendedRequest(int Id, string FolderId);
record GetExtendedResult();

record ViewItem(
    string Name,
    long? Size,
    bool? IsParent,
    bool? IsDirectory
);

record WebSocketMsg(
    string Method,
    CmdMsg? CmdMsg = null,
    CmdToggleMsg? CmdToggleMsg = null,
    StatusMsg? StatusMsg = null,
    ExtendedInfo? ExtendedInfo = null,
    bool? ProgressRevealed = null,
    CopyProgress? CopyProgress = null,
    bool? ProgressRunning = null
);

record CmdMsg(string Cmd);
record CmdToggleMsg(string Cmd, bool Checked);
record StatusMsg(
    string FolderId,
    int RequestId,
    string? Text);
record ExtendedInfo(
    string FolderId,
    int RequestId,
    DirectoryItem[] Items);

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
