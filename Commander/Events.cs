using Commander.Controllers;
using Commander.ProgressAction;
using WebServerLight;

namespace Commander;

static class Events
{
    public static void WebSocket(IWebSocket webSocket)
        => Events.webSocket = webSocket;

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

    public static async void SendCopyProgress(CopyProgress copyProgress)
    {
        if (webSocket != null)
            await webSocket.SendJson(new WebSocketMsg("copyprogress", CopyProgress: copyProgress));
    }

    public static async void SendRaw(string msg)
    {
        if (webSocket != null)
            await webSocket.SendString(msg);
    }

    static IWebSocket? webSocket;
}

record WebSocketMsg(
    string Method,
    CmdMsg? CmdMsg = null,
    CmdToggleMsg? CmdToggleMsg = null,
    StatusMsg? StatusMsg = null,
    ExtendedInfo? ExtendedInfo = null,
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

