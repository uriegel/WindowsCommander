using CsTools.Extensions;

using static CsTools.Extensions.Core;

namespace Commander.Controllers;

class RootController(string folderId) : Controller(folderId)
{
    public override string Id { get; } = "ROOT";

    public static void SaveShare(string pathWithShare)
    {
        string[] res = [.. shares, pathWithShare[..^3]];
        shares = [.. res.Distinct().Order()]; 
    }
     
    public override async Task<ChangePathResult> ChangePathAsync(string path, bool _)
    {
        var cancellation = Cancellations.ChangePathCancellation(FolderId);
        var rootItems = await GetRootItems();

        var fav = new RootItem(
            "fav",
            "Favoriten",
            -1,
            false);

        var items = ConcatEnumerables(rootItems, [fav], shares.Select(n => new RootItem(n, "", -1, true))).ToArray();
        return new RootResult(null, CheckInitial() ? Id : null, "root", items.Length, 0, items);
    }

    Task<RootItem[]> GetRootItems()
    {
        return DriveInfo
               .GetDrives()
               .Select(n => new RootItem(
                   n.Name,
                   n.IsReady ? n.VolumeLabel : "",
                   n.IsReady ? n.TotalSize : -1,
                   true))
                .ToArray()
                .ToAsync();
    }

    static string[] shares = [];
}

record RootResult(
    bool? Cancelled,
    string? Controller,
    string Path,
    int DirCount,
    int FileCount,
    RootItem[] Items
)
    : ChangePathResult(Cancelled, null, 0, Controller, Path, DirCount, FileCount);


record RootItem(
    string Name,
    string Description,
    long? Size,
    bool IsEjectable
)
    : ViewItem(Name, Size, null, null);