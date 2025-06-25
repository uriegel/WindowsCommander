using System.Text;
using CsTools;
using CsTools.Extensions;
using WebServerLight;
//using Commander.Settings;

using static CsTools.Functional.Memoization;
using static CsTools.ProcessCmd;
using static Commander.Controllers.FolderController;
using Commander.Controllers;
using Commander.Enums;

namespace Commander;

static class Requests
{
    public static async Task<bool> Process(IRequest request)
    {
        return request.SubPath switch
        {
            "changepath" => await ChangePath(request),
            "preparecopy" => await PrepareCopy(request),
            "copy" => await Copy(request),
            _ => false
        };
    }

    public static async Task<bool> GetIconFromName(IRequest request)
    {
        return true;
    }

    public static async Task<bool> GetIconFromExtension(IRequest request)
    {
        return true;
    }

    public static async Task<bool> GetFile(IRequest request)
    {
        if (request.SubPath == null)
            return false;
        using var pic = File.OpenRead("/" + request.SubPath);
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
            await webSocket.SendJson(new WebSocketMsg("cmd", new(id), null, null, null));    
    }

    public static async void SendMenuCheck(string id, bool check)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("cmdtoggle", null, new(id, check), null, null)); 
    }

    public static async void SendStatusBarInfo(string id, int requestId, string? text)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("status", null, null, new(id, requestId, text), null)); 
    }

    public static async void SendExifInfo(string id, int requestId, DirectoryItem[] items)
    {
        try
        {
            if (webSocket != null)
                await webSocket.SendJson(new WebSocketMsg("exifinfo", null, null, null, new(id, requestId, items)));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sending exif info: {ex}");
        }
    }

    static async Task<bool> ChangePath(IRequest request)
    {
        var data = await request.DeserializeAsync<ChangePathRequest>();
        if (data != null)
        {
            DetectController(data.Id, data.Path);
            var response = await GetController(data.Id).ChangePathAsync(data.Path, data.ShowHidden);
            await request.SendJsonAsync(response, response.GetType());
        }
        return true;
    }

    static async Task<bool> PrepareCopy(IRequest request)
    {
        var data = await request.DeserializeAsync<PrepareCopyRequest>();
        if (data != null)
        {
            var response = await GetController(data.Id).PrepareCopy(data);
            await request.SendJsonAsync(response, response.GetType());
        }
        return true;
    }
    
    static async Task<bool> Copy(IRequest request)
    {
        var data = await request.DeserializeAsync<CopyRequest>();
        if (data != null)
        {
            var response = await GetController(data.Id).Copy();
        //     await request.SendJsonAsync(response, response.GetType());
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
    long TotalSize
);

record CopyRequest(string Id);
record CopyResult();

record ViewItem(
    string Name,
    long? Size,
    bool? IsParent,
    bool? IsDirectory
);

record WebSocketMsg(
    string Method,
    CmdMsg? CmdMsg,
    CmdToggleMsg? CmdToggleMsg,
    StatusMsg? StatusMsg,
    ExifMsg? ExifMsg);

record CmdMsg(string Cmd);
record CmdToggleMsg(string Cmd, bool Checked);
record StatusMsg(
    string FolderId,
    int RequestId,
    string? Text);
record ExifMsg(
    string FolderId,
    int RequestId,
    DirectoryItem[] Items);

