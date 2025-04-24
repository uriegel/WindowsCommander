
namespace Commander.Controllers;

class RootController(FolderView folderView) : IController
{
    #region IController

    public void RemoveAll() { }
    public async Task<int> Fill(string path, FolderView folderView)
    {
        return 0;
    }

    #endregion

    public void Execute(string command)
    {
        //throw new NotImplementedException();
    }

    public void Refresh()
    {
        //throw new NotImplementedException();
    }

    public void Dispose()
    {
        //throw new NotImplementedException();
    }
}
