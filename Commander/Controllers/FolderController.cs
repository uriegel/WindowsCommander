using System.Collections.Concurrent;
using CsTools.Extensions;

namespace Commander.Controllers;

static class FolderController
{
    public static Controller DetectController(string id, string path)
        => controllers.AddOrUpdate(
            id,
            _ => CreateController(id, path),
            (_, controller) => SetController(controller, path, () => CreateController(id, path)));

    public static Controller GetController(string id) => controllers[id];

    static Controller SetController<T>(Controller current, string path, Func<T> factory)
        where T : Controller
    {
        if (current.GetType() != GetControllerType(path))
            current = factory();
        return current;
    }

    static Type GetControllerType(string path)
        => path switch
        {
            "root" => typeof(RootController),
            "/.." => typeof(RootController),
            "" => typeof(RootController),
            "fav" => typeof(FavoritesController),
            _ => IsShareParent(path)
                ? typeof(RootController) 
                : typeof(DirectoryController)
        };

    static bool IsShareParent(string path)
        => path.StartsWith(@"\\") && path.Replace('/', '\\').Pipe(p => p.EndsWith(@"\..") && p.Count(n => n == '\\') == 4);

    static Controller CreateController(string folderId, string path)
        => path switch
        {
            "root" => new RootController(folderId),
            "/.." => new RootController(folderId),
            "" => new RootController(folderId),
            "fav" => new FavoritesController(folderId),
            _ => IsShareParent(path)
                ? new RootController(folderId).SideEffect(_ => RootController.SaveShare(path))
                : new DirectoryController(folderId)
        };

    static readonly ConcurrentDictionary<string, Controller> controllers = [];
}