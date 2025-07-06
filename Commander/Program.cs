using Commander;
using WebServerLight;
using WebServerLight.Routing;
using WebWindowNetCore;

class Program
{
    public static Program Instance { get; } = new Program();

    public WebView WebView { get; }

    public static void Main() => Instance.WebView.Run();

    public Program()
    {
        server =
            WebServer
                .New()
                .Http(20000)
                .WebsiteFromResource()
                .Route(MethodRoute
                    .New(Method.Post)
                    .Add(PathRoute
                        .New("/request")
                        .Request(Requests.Process)))
                .Route(MethodRoute
                    .New(Method.Get)
                    .Add(PathRoute
                        .New("/iconfromname")
                        .Request(Requests.GetIconFromName))
                    .Add(PathRoute
                        .New("/iconfromextension")
                        .Request(Requests.GetIconFromExtension))
                    .Add(PathRoute
                        .New("/getfile")
                        .Request(Requests.GetFile))
                    .Add(PathRoute
                        .New("/gettrack")
                        .Request(Requests.GetTrack)))
                .AddAllowedOrigin("http://localhost:5173")
                .AccessControlMaxAge(TimeSpan.FromMinutes(5))
                .WebSocket(Requests.WebSocket)
                .UseRange()
                .Build();
        server.Start();

        WebView = WebView
            .Create()
            .AppId("de.uriegel.test")
            .Title("Commander")
            .InitialBounds(600, 800)
            .SaveBounds()
            .DevTools()
            .DefaultContextMenuDisabled()
            .ResourceIcon("icon")
            .WithoutNativeTitlebar()
            .DebugUrl("http://localhost:5173")
            .Url("http://localhost:20000")
            .CanClose(() => true);
    }

    readonly IServer server;
}


