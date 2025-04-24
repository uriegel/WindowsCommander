namespace Commander.Controllers
{
    interface IController 
    {
        void RemoveAll();
        Task<int> Fill(string path, FolderView22 folderView);
    }
}
