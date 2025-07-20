using System.Diagnostics;
using Commander.Controllers;
using CsTools.Extensions;
using URiegel.WebSocketClient;
using WebServerLight;
using WebServerLight.Routing;

namespace Commander;

static class UacServer
{
    public static bool IsRunning { get; private set; }

    public static async Task<bool> Start()
    {
        try
        {
            process = new Process()
            {
                StartInfo = new(Process.GetCurrentProcess()?.MainModule?.FileName ?? "")
                {
#if DEBUG
                    Arguments = $"{Environment.CurrentDirectory.AppendPath(@"Commander\bin\Debug\net9.0-windows\commander.dll")} -adminMode {Environment.ProcessId}",
#else
                    Arguments = $"-adminMode {Environment.ProcessId}",
#endif
                    Verb = "runas",
                    UseShellExecute = true
                }

            };
            if (process.Start())
                IsRunning = true;
            else
                return false;
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => IsRunning = false;

            // TODO check websocket client finalizing
            var client = new WsClient("ws://localhost:21000/events", msg =>
            {
                Events.SendRaw(msg);
                return 0.ToAsync();
            }, () => Console.WriteLine("Closed"));

            await client.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void Stop() => processRunning?.SetResult();

    public static async Task Run(int commanderId)
    {
        processRunning = new();

        server =
            WebServer
                .New()
                .Http(21000)
                .WebsiteFromResource()
                .Route(MethodRoute
                    .New(Method.Post)
                    .Add(PathRoute
                        .New("/request")
                        .Request(req => Requests.Process(req))))
                .AddAllowedOrigin("http://localhost:5173")
                .AddAllowedOrigin("http://localhost:20000")
                .AccessControlMaxAge(TimeSpan.FromMinutes(5))
                .WebSocket(Events.WebSocket)
                .Build();
        server.Start();

        await Task.WhenAny([
            Process.GetProcessById(commanderId).WaitForExitAsync(),
            processRunning.Task
        ]);

    }

    public static Task<SetControllerResponse> SetController(SetControllerRequest setController)
    {
        FolderController.DetectController(setController.Id, setController.Path ?? "");
        return new SetControllerResponse().ToAsync();
    }

    static TaskCompletionSource? processRunning;
    static IServer? server;

    static Process? process;
}

record WebSocketRunningMsg(
    string Method,
    bool? ProgressRunning = null
);
