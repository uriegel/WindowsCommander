using System.Windows;
using System.Windows.Input;

using Commander.Controls;

namespace Commander.Views;

public partial class MainWindow : Window
{
    #region Routed Commands

    public static RoutedUICommand SelectCurrentCommand { get; } = new("SelectCurrent", "SelectCurrent", typeof(MainWindow));

    #endregion

    #region Command Bindings
    
    void SelectCurrent_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        //var element = FocusManager.GetFocusedElement(this);
        //if (element is ListViewItem lvi)
        //{
        //    if (LeftView.PeopleListView.SelectedItems.Contains(lvi.DataContext))
        //    {
        //        LeftView.PeopleListView.SelectedItems.Remove(lvi.DataContext);
        //        lvi.IsSelected = false;
        //    }
        //    else
        //    {
        //        LeftView.PeopleListView.SelectedItems.Add(lvi.DataContext);
        //        lvi.IsSelected = true;
        //    }


        //    int index = LeftView.PeopleListView.ItemContainerGenerator.IndexFromContainer(lvi);
        //    int nextIndex = index + 1;

        //    if (nextIndex < LeftView.PeopleListView.ItemsSource.Cast<Item>().Count())
        //    {
        //        var nextItem = LeftView.PeopleListView.Items[nextIndex];

        //        LeftView.PeopleListView.ScrollIntoView(nextItem); // Optional: ensure it's visible
        //        LeftView.PeopleListView.UpdateLayout(); // Ensure container is generated

        //        var nextContainer = (ListViewItem)LeftView.PeopleListView.ItemContainerGenerator.ContainerFromItem(nextItem);
        //        if (nextContainer != null)
        //            Keyboard.Focus(nextContainer); // Focus with visible focus rectangle
        //    }
        //}

    }

    #endregion

    public MainWindow()
    {
        InitializeComponent();
        LeftView.ChangePath(Properties.Settings.Default.LeftPath ?? "root", true, true);
        RightView.ChangePath(Properties.Settings.Default.RightPath ?? "root", true, true);
    }

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
        Properties.Settings.Default.LeftPath = (LeftView.DataContext as FolderViewContext)?.CurrentPath ?? Properties.Settings.Default.LeftPath;
        Properties.Settings.Default.RightPath = (RightView.DataContext as FolderViewContext)?.CurrentPath ?? Properties.Settings.Default.RightPath;

        Properties.Settings.Default.Save();
        base.OnClosing(e);
    }

    void Window_Loaded(object sender, RoutedEventArgs e) {}

    void LeftView_GotFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowContext mwc && LeftView.DataContext is FolderViewContext fvc)
            mwc.ActiveFolderView = fvc;
    }

    void RightView_GotFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowContext mwc && RightView.DataContext is FolderViewContext fvc)
            mwc.ActiveFolderView = fvc;
    }
}