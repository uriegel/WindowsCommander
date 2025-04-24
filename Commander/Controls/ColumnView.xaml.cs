using System.Windows;
using System.Windows.Controls;

namespace Commander.Controls;

public partial class ColumnView : UserControl
{
    #region Routed Events

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler),
            typeof(ColumnView));

    public event SelectionChangedEventHandler SelectionChanged
    {
        add { AddHandler(SelectionChangedEvent, value); }
        remove { RemoveHandler(SelectionChangedEvent, value); }
    }

    #endregion

    public ColumnView() => InitializeComponent();

    void ListView_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // var lbi = (e.OriginalSource as DependencyObject)?.FindAncestorOrSelf<ListBoxItem>();
        //if (lbi != null)
        //{
        //    ListView.UpdateLayout(); // Ensure layout is up to date
        //    ListView.ScrollIntoView(lbi); // Ensure the item is visible
        //    //ListViewItem container = (ListViewItem)PeopleListView.ItemContainerGenerator.ContainerFromItem(lbi);
        //    Keyboard.Focus(lbi);

        //    e.Handled = true;
        //}
    }

    void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, e.AddedItems, e.RemovedItems)
        {
            RoutedEvent = SelectionChangedEvent,
            Source = this
        });
}

