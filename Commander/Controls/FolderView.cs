
using System.IO;
using System;

using Commander.Controllers;
using Commander.Controls.ColumnViewHeader;
using System.Windows.Controls;
using System.Windows;
using Commander.Controllers.Root;

namespace Commander;

public class FolderView : ColumnView
{
    public FolderView() : base() => controller = new RootController(this);

    public async void ChangePath(string path, bool saveHistory)
    {
        DetectController(path);
        try
        {
            controller.RemoveAll();
            var lastPos = await controller.Fill(path, this);
            //if (lastPos != -1)
            //    folderView.ScrollTo(lastPos);
            //folderView.Context.CurrentDirectories = Actions.Instance.ShowHidden ? controller.Directories + controller.HiddenDirectories : controller.Directories;
            //folderView.Context.CurrentFiles = Actions.Instance.ShowHidden ? controller.Files + controller.Files : controller.Files;
            //folderView.Context.CurrentPath = controller.CurrentPath;
            //folderView.OnPathChanged(saveHistory ? CurrentPath : null);
        }
        catch (UnauthorizedAccessException uae)
        {
            //OnError(uae);
            //MainContext.Instance.ErrorText = "Kein Zugriff";
        }
        catch (DirectoryNotFoundException dnfe)
        {
            //OnError(dnfe);
            //MainContext.Instance.ErrorText = "Pfad nicht gefunden";
        }
        //catch (RequestException re) when (re.CustomRequestError == CustomRequestError.ConnectionError)
        //{
        //    //OnError(re);
        //    //MainContext.Instance.ErrorText = "Die Verbindung zum Gerät konnte nicht aufgebaut werden";
        //}
        //catch (RequestException re) when (re.CustomRequestError == CustomRequestError.NameResolutionError)
        //{
        //    OnError(re);
        //    MainContext.Instance.ErrorText = "Der Netzwerkname des Gerätes konnte nicht ermittelt werden";
        //}
        catch (Exception e)
        {
            //OnError(e);
            //MainContext.Instance.ErrorText = "Ordner konnte nicht gewechselt werden";
        }
    }

    protected override void OnInitialize()
    {
    }

    bool DetectController(string path)
    {
        return path switch
        {
            //"fav" => SetController(() => new FavoritesController(folderView)),
            //"remotes" => SetController(() => new RemotesController(folderView)),
            "root" => SetController(() => new RootController(this)),
            //"" => SetController(() => new RootController(folderView)),
            //_ when path.StartsWith("remote") => SetController(() => new RemoteController(folderView)),
            //_ => SetController(() => new DirectoryController(folderView))
            _ => SetController(() => new RootController(this)),
        };

        bool SetController<T>(Func<T> controller)
            where T : IController
        {
            if (this.controller is not T)
            {
                //this.controller.Dispose();
                this.controller = controller();
                return true;
            }
            else
                return false;
        }
    }

    IController controller;
}
