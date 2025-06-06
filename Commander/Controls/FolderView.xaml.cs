﻿using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using ClrWinApi;

using Commander.Controllers;
using Commander.Controllers.Directory;
using Commander.Controllers.Root;
using Commander.Controls.ColumnViewHeader;
using Commander.Views;

namespace Commander.Controls;

// TODO F9 adapt
// TODO CreateFolder
// TODO Rename with ShellExecute and SH-UI (UAC)

// TODO ShowProperties
// TODO OpenFile
// TODO OpenWith

// TODO Banner info
// TODO Sorting by file extension

// TODO %systemroot%\system32\imageres.dll
// TODO Copy (move): create test copy from network to folder with no access


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
            Filter = i => FilterHidden(i) && FilterRestriction(i),
        };
        ColumnView.ListView.ItemsSource = view;
    }

    public IEnumerable<Item> GetItems()
        => ColumnView.ListView.ItemsSource is ListCollectionView view
            ? view.Cast<Item>()
            : [];

    public async void ChangePath(string? path, bool saveHistory, bool dontFocus = false) 
        => await ChangePathAsync(path, saveHistory, dontFocus);
    
    public void SelectAll() => ColumnView.ListView.SelectAll();

    public void SelectNone() => ColumnView.ListView.SelectedItems.Clear();

    public void SelectTillHere()
    {
        ColumnView.ListView.SelectedItems.Clear();
        var currentItem = ColumnView.CurrentItem as Item;
        if (currentItem != null)
        {
            var index = GetItems().Index().FirstOrDefault(n => n.Item == currentItem).Index;
            foreach (var item in GetItems().Take(index + 1))
                ColumnView.ListView.SelectedItems.Add(item);
        }
    }
    public void SelectTillEnd()
    {
        ColumnView.ListView.SelectedItems.Clear();
        var currentItem = ColumnView.CurrentItem as Item;
        if (currentItem != null)
        {
            var index = GetItems().Index().FirstOrDefault(n => n.Item == currentItem).Index;
            foreach (var item in GetItems().Skip(index))
               ColumnView.ListView.SelectedItems.Add(item);
        }
    }

    public void ToggleCurrentSelection()
    {
        var currentItem = ColumnView.CurrentItem as Item;
        if (currentItem != null)
        {
            if (ColumnView.ListView.SelectedItems.Contains(currentItem))
                ColumnView.ListView.SelectedItems.Remove(currentItem);
            else
                ColumnView.ListView.SelectedItems.Add(currentItem);
            var index = GetItems().Index().FirstOrDefault(n => n.Item == currentItem).Index;
            var listViewItem = ColumnView.ListView.ItemContainerGenerator.ContainerFromIndex(index + 1) as ListViewItem;
            ColumnView.ListView.UpdateLayout();
            listViewItem?.Focus();
        }
    }

    public async Task Refresh(bool focus = true)
    {
        var currentItem = ColumnView.CurrentItem as Item;
        var selectedItemsNames = ColumnView.ListView.SelectedItems.Cast<Item>().Select(n => n.Name).ToArray();
        ColumnView.ListView.SelectedItems.Clear();
        await ChangePathAsync(Context.CurrentPath, false, !focus);
        if (currentItem != null)
        {
            var index = GetItems().Index().FirstOrDefault(n => n.Item.Name == currentItem.Name).Index;
            var listViewItem = ColumnView.ListView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
            ColumnView.ListView.UpdateLayout();
            if (focus)
                listViewItem?.Focus();
        }
        if (selectedItemsNames.Length != 0)
        {
            var items = GetItems();
            var newSelectedItems = items.Where(n => selectedItemsNames.Contains(n.Name));
            foreach (var selectedItem in newSelectedItems)
                ColumnView.ListView.SelectedItems.Add(selectedItem);
        }
    }

    public void CopyItems(FolderView targetFolderView, bool move) => Controller.CopyItems(this, targetFolderView, move);

    public void DeleteItems() => Controller.DeleteItems(this);

    public (Item? currentItem, IEnumerable<Item> selectedItems) GetSelectedItems()
    {
        var currentItem = ColumnView.CurrentItem as Item;
        var selectedItems = ColumnView
                        .ListView
                        .SelectedItems
                        .OfType<Item>();
        return (currentItem, selectedItems);
    }

    async Task ChangePathAsync(string? path, bool saveHistory, bool dontFocus = false)
    {
        Context.Restriction = null;
        cancellation.Cancel();
        cancellation = new();
        DetectController(path);
        try
        {
            Controller.RemoveAll();
            ColumnView.ListView.UpdateLayout();
            var lastPos = await Controller.Fill(path, this);
            var view = CollectionViewSource.GetDefaultView(ColumnView.ListView.ItemsSource) as ListCollectionView;
            var items = view?.Cast<Item>();
            if (items != null)
            {
                Controller.StartResolvingExtendedInfos(items.ToArray(), Context, cancellation.Token);
                if (saveHistory && Context.CurrentPath != null)
                    history.Set(Context.CurrentPath);
                if (!dontFocus)
                    await Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
                    {
                        ColumnView.ListView.ScrollIntoView(items.Skip(lastPos).FirstOrDefault());
                        var listViewItem = ColumnView.ListView.ItemContainerGenerator.ContainerFromIndex(lastPos) as ListViewItem;
                        ColumnView.ListView.UpdateLayout();
                        listViewItem?.Focus();
                    });
                Context.DirectoriesCount =
                    items.Where(n => n is DirectoryItem di && (MainWindowContext.Instance.ShowHidden || !di.IsHidden))?.Count() ?? 0;
                Context.FilesCount =
                    items.Where(n => n is FileItem di && (MainWindowContext.Instance.ShowHidden || !di.IsHidden))?.Count() ?? 0;
            }
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
            MainWindow.Instance.GetActiveView().ColumnView.FocusCurrentItem();
            active.ColumnView.FocusCurrentItem();
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

    void ColumnView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space && Context.Restriction != null)
        {
            Context.Restriction += " ";
            RefreshRestrictionView();
            e.Handled = true;
        }
        else if (e.Key == Key.End && Keyboard.Modifiers == ModifierKeys.Shift)
        {
            SelectTillEnd();
            e.Handled = true;
        }
        else if (e.Key == Key.Home && Keyboard.Modifiers == ModifierKeys.Shift)
        {
            SelectTillHere();
            e.Handled = true;
        }
    }

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
                    if (Context.Restriction == null)
                    {
                        var path = history?.Get(Keyboard.Modifiers == ModifierKeys.Shift);
                        if (path != null)
                            ChangePath(path, false);
                    }
                    else
                    {
                        Context.Restriction = Context.Restriction[..^1];
                        if (Context.Restriction.Length == 0)
                            Context.Restriction = null;
                        RefreshRestrictionView();
                    }
                    e.Handled = true;
                }
                break;
            case Key.Escape:
                if (Context.Restriction != null)
                {
                    Context.Restriction = null;
                    RefreshRestrictionView();
                    e.Handled = true;
                }
                break;
            default:
                var chr = GetCharFromKey(e.Key);
                if (chr.HasValue)
                {
                    Context.Restriction += chr.ToString();
                    if (ColumnView.ListView.Items.Count == 0)
                        Context.Restriction = Context.Restriction[..^1];
                    RefreshRestrictionView();
                }
                break;
        }
    }

    static char? GetCharFromKey(Key key)
    {
        if (Keyboard.Modifiers != ModifierKeys.None || key == Key.Tab || key == Key.Enter || key == Key.Back)
            return null;    
        var virtualKey = (uint)KeyInterop.VirtualKeyFromKey(key);
        var keyboardState = new byte[256];
        Api.GetKeyboardState(keyboardState);
        var scanCode = Api.MapVirtualKey(virtualKey, MapType.Vsc);
        var stringBuilder = new StringBuilder(2);

        return Api.ToUnicode(virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0) switch
        {
            1 => stringBuilder[0],
            _ => null,
        };
    }

    static bool FilterHidden(object item)
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

    bool FilterRestriction(object item)
        => Context.Restriction == null 
            || item is Item i && i.Name.StartsWith(Context.Restriction, StringComparison.CurrentCultureIgnoreCase);

    void RefreshRestrictionView()
    {
        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(ColumnView.ListView.ItemsSource);
        view.Refresh();
        ColumnView.FocusCurrentItem();
        if (ColumnView.ListView.Items.Count == 0 && Context.Restriction?.Length > 0)
        {
            Context.Restriction = Context.Restriction[..^1];
            if (Context.Restriction.Length == 0)
                Context.Restriction = null;
            view.Refresh();
            ColumnView.FocusCurrentItem();
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
    CancellationTokenSource cancellation = new();
}

