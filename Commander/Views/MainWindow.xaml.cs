using System.Windows;
using System.Windows.Input;

using Commander.Controls;

namespace Commander.Views;

public partial class MainWindow : Window
{
    #region Routed Commands

    public static RoutedUICommand SelectAllCommand { get; } = new("SelectAll", "SelectAll", typeof(MainWindow));
    public static RoutedUICommand SelectNoneCommand { get; } = new("SelectNone", "SelectNone", typeof(MainWindow));
    public static RoutedUICommand ToggleCurrentSelectionCommand { get; } = new("ToggleCurrentSelection", "ToggleCurrentSelection", typeof(MainWindow));
    public static RoutedUICommand SelectTillHereCommand { get; } = new("SelectTillHere", "SelectTillHere", typeof(MainWindow));
    public static RoutedUICommand SelectTillEndCommand { get; } = new("SelectTillEnd", "SSelectTillEnd", typeof(MainWindow));
    public static RoutedUICommand ShowHiddenCommand { get; } = new("ShowHidden", "ShowHidden", typeof(MainWindow));
    public static RoutedUICommand RefreshCommand { get; } = new("Refresh", "Refresh", typeof(MainWindow));
    public static RoutedUICommand CopyItemsCommand { get; } = new("CopyItems", "CopyItems", typeof(MainWindow));
    public static RoutedUICommand MoveItemsCommand { get; } = new("MoveItems", "MoveItems", typeof(MainWindow));
    public static RoutedUICommand DeleteItemsCommand { get; } = new("DeleteItems", "DeleteItems", typeof(MainWindow));

    #endregion

    #region Command Bindings

    void ShowHidden_Executed(object sender, ExecutedRoutedEventArgs e)
    //=> ShowHidden.IsChecked = !ShowHidden.IsChecked;

    {
        ShowHidden.IsChecked = !ShowHidden.IsChecked;
        MainWindowContext.Instance.ErrorText = ShowHidden.IsChecked ? "Das ist ein ganz besonders blöder Fäler" : null;
    }
    void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e) => GetActiveView().SelectAll();
    void SelectNone_Executed(object sender, ExecutedRoutedEventArgs e) => GetActiveView().SelectNone();
    void ToggleCurrentSelection_Executed(object sender, ExecutedRoutedEventArgs e) => GetActiveView().ToggleCurrentSelection();
    void SelectTillHere_Executed(object sender, ExecutedRoutedEventArgs e) => GetActiveView().SelectTillHere();
    void SelectTillEnd_Executed(object sender, ExecutedRoutedEventArgs e) => GetActiveView().SelectTillEnd();
    async void Refresh_Executed(object sender, ExecutedRoutedEventArgs e) => await GetActiveView().Refresh();
    void CopyItems_Executed(object sender, ExecutedRoutedEventArgs e) 
        => GetActiveView().CopyItems(GetInactiveView().Context.CurrentPath, GetInactiveView().Refresh, false);
    void MoveItems_Executed(object sender, ExecutedRoutedEventArgs e)
        => GetActiveView().CopyItems(GetInactiveView().Context.CurrentPath, GetInactiveView().Refresh, true);
    void DeleteItems_Executed(object sender, ExecutedRoutedEventArgs e) => GetActiveView().DeleteItems();

    #endregion

    public static MainWindow Instance { get; private set; } = null!;

    public MainWindow()
    {
        InitializeComponent();

        activeFolderView = LeftView;
        Instance = this;
        LeftView.ChangePath(Properties.Settings.Default.LeftPath ?? "root", true, true);
        RightView.ChangePath(Properties.Settings.Default.RightPath ?? "root", true, true);
    }

    public FolderView GetActiveView()
        => activeFolderView == LeftView
            ? LeftView
            : RightView;

    public FolderView GetInactiveView()
        => activeFolderView == LeftView
            ? RightView
            : LeftView;

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Restore bounds
        if (Properties.Settings.Default.WindowWidth > 0 &&
            Properties.Settings.Default.WindowHeight > 0)
        {
            Top = Properties.Settings.Default.WindowTop;
            Left = Properties.Settings.Default.WindowLeft;
            Width = Properties.Settings.Default.WindowWidth;
            Height = Properties.Settings.Default.WindowHeight;
        }

        // Restore window state after layout is applied
        Loaded += (s, ev) => WindowState = Properties.Settings.Default.WindowState;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        Properties.Settings.Default.WindowTop = Top;
        Properties.Settings.Default.WindowLeft = Left;
        Properties.Settings.Default.WindowWidth = Width;
        Properties.Settings.Default.WindowHeight = Height;
        Properties.Settings.Default.WindowState = WindowState;
        Properties.Settings.Default.LeftPath = LeftView.Context.CurrentPath ?? Properties.Settings.Default.LeftPath;
        Properties.Settings.Default.RightPath = RightView.Context.CurrentPath ?? Properties.Settings.Default.RightPath;

        Properties.Settings.Default.Save();
        base.OnClosing(e);
    }

    void Window_Loaded(object sender, RoutedEventArgs e) {}

    void LeftView_GotFocus(object sender, RoutedEventArgs e)
    {
        activeFolderView = LeftView;
        MainWindowContext.Instance.ActiveFolderContext = LeftView.Context;
    }

    void RightView_GotFocus(object sender, RoutedEventArgs e)
    {
        activeFolderView = RightView;
        MainWindowContext.Instance.ActiveFolderContext = RightView.Context;
    }

    void Hidden_Checked(object sender, RoutedEventArgs e)
        => MainWindowContext.Instance.ShowHidden = true;

    void Hidden_Unchecked(object sender, RoutedEventArgs e)
        => MainWindowContext.Instance.ShowHidden = false;

    FolderView activeFolderView;
}