using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Commander.Controllers;
using Commander.Controllers.Directory;
using Commander.Controllers.Root;

namespace Commander.Controls;

// TODO StatusBar: ActiveFolderContext, CurrentItemPath
// TODO StatusBar: files, dirs
// TODO Drive icons, removable drive icons
// TODO save last pathes
// TODO History
// TODO Sorting
// TODO Filter hidden
// TODO Restriction
// TODO Banner
// TODO Selection

public partial class FolderView : UserControl
{
    public FolderView()
    {
        InitializeComponent();
        Controller = new RootController(this);
    }

    public async void ChangePath(string? path, bool saveHistory, bool dontFocus = false)
    {
        DetectController(path);
        try
        {
            Controller.RemoveAll();
            ColumnView.ListView.UpdateLayout();
            var lastPos = await Controller.Fill(path, this);
            if (!dontFocus)
                await Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
                {
                    ColumnView.ListView.ScrollIntoView(ColumnView.ListView.Items[lastPos]);
                    var listViewItem = (ListViewItem)ColumnView.ListView.ItemContainerGenerator.ContainerFromIndex(lastPos);
                    ColumnView.ListView.UpdateLayout();
                    listViewItem?.Focus();
                });
            //folderView.Context.CurrentDirectories = Actions.Instance.ShowHidden ? controller.Directories + controller.HiddenDirectories : controller.Directories;
            //folderView.Context.CurrentFiles = Actions.Instance.ShowHidden ? controller.Files + controller.Files : controller.Files;
            //folderView.Context.CurrentPath = controller.CurrentPath;
            //folderView.OnPathChanged(saveHistory ? CurrentPath : null);
        }
        catch (UnauthorizedAccessException)
        {
            //OnError(uae);
            //MainContext.Instance.ErrorText = "Kein Zugriff";
        }
        catch (DirectoryNotFoundException)
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
        catch (Exception)
        {
            //OnError(e);
            //MainContext.Instance.ErrorText = "Ordner konnte nicht gewechselt werden";
        }
    }

    bool DetectController(string? path)
    {
        return path switch
        {
            //"fav" => SetController(() => new FavoritesController(folderView)),
            //"remotes" => SetController(() => new RemotesController(folderView)),
            "root" => SetController(() => new RootController(this)),
            "" => SetController(() => new RootController(this)),
            //_ when path.StartsWith("remote") => SetController(() => new RemoteController(folderView)),
            _ => SetController(() => new DirectoryController(this))
        };

        bool SetController<T>(Func<T> controller)
            where T : IController
        {
            if (this.Controller is not T)
            {
                //this.controller.Dispose();
                this.Controller = controller();
                return true;
            }
            else
                return false;
        }
    }

    void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Return:

                ChangePath(PathTextBox.Text, true);

                Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
                {
                    var ripple = new WaterRipple
                    {
                        Amplitude = 10,
                        RatioControl = 2,
                        Frequency = 35
                    };
                    ColumnView.Effect = ripple;
                    var story = (Storyboard)FindResource("WaterRipples");
                    story.Completed += story_Completed;
                    story.Begin();
                });

                break;
        }

        void story_Completed(object? sender, EventArgs e)
        {
            var story = (Storyboard)FindResource("WaterRipples");
            story.Completed -= story_Completed;
            ColumnView.Effect = null;
        }
    }

    void TextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() => (sender as TextBox)?.SelectAll());
        e.Handled = true;
    }

    void ColumnView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => Controller.OnSelectionChanged(ColumnView.ListView.SelectedItems, e);

    void ColumnView_CurrentItemChanged(object sender, RoutedEvents.CurrentItemChangedEventArgs e)
    {
        if (DataContext is FolderViewContext fvc)
            fvc.CurrentItemPath = Controller.GetCurrentPath(fvc.CurrentPath, e.CurrentItem?.DataContext as Item) ?? "";   
    }

    void ColumnView_OnEnter(object sender, System.Windows.RoutedEventArgs e)
        =>  ChangePath((DataContext as FolderViewContext)?.CurrentItemPath, true);

    void ColumnView_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case System.Windows.Input.Key.Tab when (Keyboard.Modifiers == ModifierKeys.Shift):
                PathTextBox.Focus();
                e.Handled = true;
                break;
        }
    }

    IController Controller 
    {
        get => field; 
        set
        {
            field = value;
            if (DataContext is FolderViewContext fvc)
                fvc.OnChanged();
        }
    }
}
