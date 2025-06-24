using WebWindowNetCore;

WebView
    .Create()
    .AppId("de.uriegel.test")
    .Title("Web Window Net Core 👍")
    .InitialBounds(600, 800)
    .SaveBounds()
    .DevTools()
    //.DefaultContextMenuDisabled()
#if Windows
    .ResourceIcon("icon")
#endif
    .DebugUrl("http://localhost:5173")
    .Url("https://github.com")
    .CanClose(() => true)
    .Run();