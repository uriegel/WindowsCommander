using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Commander.Controllers;
using Commander.Controllers.Directory;
using Commander.Controllers.Root;
using Commander.Controls.ColumnViewHeader;
using Commander.Views;

namespace Commander.Controls;

// TODO Restriction
// TODO Version, Exif with Statusbar hint (lightblue background
// TODO Banner
// TODO Selection
// TODO Copy, Move, Delete, Rename with ShellExecute and SH-UI (UAC)

public partial class FolderView : UserControl
{
    public FolderViewContext Context { get => (DataContext as FolderViewContext)!; }

    public FolderView()
    {
        InitializeComponent();
        Controller = new RootController(this);
        MainWindowContext.Instance.PropertyChanged += Instance_PropertyChanged;
    }

    public void SetColumnViewContext(ColumnViewContext ctx)
    {
        ColumnView.DataContext = ctx;
        ColumnView.Headers.ColumnViewContext = ctx;
    }

    public void SetHeaders(HeaderItem[] headers)
        => ColumnView.Headers.HeaderItems = headers;

    public void SetItemsSource(IEnumerable<Item> items)
    {
        var oldView = CollectionViewSource.GetDefaultView(ColumnView.ListView.ItemsSource) as ListCollectionView;
        var view = new ListCollectionView(items.ToList())
        {
            CustomSort = oldView?.CustomSort,
            Filter = item =>
            {
                if (item is Item i)
                {
                    if (i is ParentItem)
                        return true;
                    else if (i is DirectoryItem di)
                        return MainWindowContext.Instance.ShowHidden || !di.IsHidden;
                    else if (i is FileItem fi)
                        return MainWindowContext.Instance.ShowHidden || !fi.IsHidden;
                    else
                        return true;
                }
                else
                    return false;
            }
        };
        ColumnView.ListView.ItemsSource = view;
    }

    public IEnumerable<Item> GetItems()
        => ColumnView.ListView.ItemsSource is ListCollectionView view
            ? view.Cast<Item>()
            : [];

    public async void ChangePath(string? path, bool saveHistory, bool dontFocus = false)
    {
        DetectController(path);
        try
        {
            Controller.RemoveAll();
            ColumnView.ListView.UpdateLayout();
            var lastPos = await Controller.Fill(path, this);
            if (saveHistory && Context.CurrentPath != null)
                history.Set(Context.CurrentPath);
            var view = CollectionViewSource.GetDefaultView(ColumnView.ListView.ItemsSource) as ListCollectionView;
            var items = view?.Cast<Item>();
            if (!dontFocus)
                await Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
                {
                    ColumnView.ListView.ScrollIntoView(items?.Skip(lastPos).FirstOrDefault());
                    var listViewItem = (ListViewItem)ColumnView.ListView.ItemContainerGenerator.ContainerFromIndex(lastPos);
                    ColumnView.ListView.UpdateLayout();
                    listViewItem?.Focus();
                });
            Context.DirectoriesCount = 
                items
                    ?.Where(n => n is DirectoryItem di && (MainWindowContext.Instance.ShowHidden || !di.IsHidden))?.Count() ?? 0;
            Context.FilesCount = 
                items
                    ?.Where(n => n is FileItem di && (MainWindowContext.Instance.ShowHidden || !di.IsHidden))?.Count() ?? 0; 
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

    void Instance_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowContext.ShowHidden))
        {
            var view = CollectionViewSource.GetDefaultView(ColumnView.ListView.ItemsSource) as ListCollectionView;
            view?.Refresh();

            var items = view?.Cast<Item>();
            if (items != null)
            {
                Context.DirectoriesCount = items.Where(n => n is DirectoryItem di && (MainWindowContext.Instance.ShowHidden || !di.IsHidden))?.Count() ?? 0;
                Context.FilesCount = items?.Where(n => n is FileItem di && (MainWindowContext.Instance.ShowHidden || !di.IsHidden))?.Count() ?? 0;
            }
            var active = MainWindow.Instance.GetActiveView();
            MainWindow.Instance.GetInactiveView().ColumnView.FocusCurrentItem();
            active.ColumnView.FocusCurrentItem();
        }
    }

    void TextBox_KeyDown(object sender, KeyEventArgs e)
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

    void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() => (sender as TextBox)?.SelectAll());
        e.Handled = true;
    }

    void ColumnView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => Controller.OnSelectionChanged(ColumnView.ListView.SelectedItems, e);

    void ColumnView_CurrentItemChanged(object sender, RoutedEvents.CurrentItemChangedEventArgs e)
        => Context.CurrentItemPath = Controller.GetCurrentPath(Context.CurrentPath, e.CurrentItem?.DataContext as Item) ?? "";   

    void ColumnView_OnEnter(object sender, System.Windows.RoutedEventArgs e)
        =>  ChangePath(Context.CurrentItemPath, true);

    void ColumnView_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Tab when (Keyboard.Modifiers == ModifierKeys.Shift):
                PathTextBox.Focus();
                e.Handled = true;
                break;
            case Key.Back:
                {
                    var path = history?.Get(Keyboard.Modifiers == ModifierKeys.Shift);
                    if (path != null)
                        ChangePath(path, false);
                    e.Handled = true;
                }
                break;
        }
    }

    IController Controller 
    {
        get => field; 
        set
        {
            field = value;
            Context.OnChanged();
        }
    }

    readonly History history = new();
}
