using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Commander.Controllers.Directory;
using Commander.Extensions;
using Commander.RoutedEvents;

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

    public static readonly RoutedEvent CurrentItemChangedEvent =
        EventManager.RegisterRoutedEvent("CurrentItemChanged", RoutingStrategy.Bubble, typeof(CurrentItemChangedEventHandler),
            typeof(ColumnView));

    public event CurrentItemChangedEventHandler CurrentItemChanged
    {
        add { AddHandler(CurrentItemChangedEvent, value); }
        remove { RemoveHandler(CurrentItemChangedEvent, value); }
    }

    public static readonly RoutedEvent OnEnterEvent =
        EventManager.RegisterRoutedEvent("OnEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
            typeof(ColumnView));

    public event RoutedEventHandler OnEnter
    {
        add { AddHandler(OnEnterEvent, value); }
        remove { RemoveHandler(OnEnterEvent, value); }
    }

    #endregion

    public ColumnView() => InitializeComponent();

    public void FocusCurrentItem()
    {
        if (currentItem != null)
        {
            ListView.ScrollIntoView(currentItem);
            UpdateLayout();
            var listViewItem = (ListViewItem)ListView.ItemContainerGenerator.ContainerFromItem(currentItem);
            listViewItem?.Focus();
        }
    }

    void ListView_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control)
        {
            var lbi = (e.OriginalSource as DependencyObject)?.FindAncestorOrSelf<ListBoxItem>();
            if (lbi != null)
            {
                ListView.UpdateLayout();
                ListView.ScrollIntoView(lbi);
                Keyboard.Focus(lbi);
                if (e.ClickCount == 1)
                    e.Handled = true;
                else if (e.ClickCount == 2)
                {
                    RaiseEvent(new RoutedEventArgs(OnEnterEvent)
                    {
                        Source = this
                    });
                    e.Handled = true;
                }
            }
        }
    }

    void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems)
        {
            RoutedEvent = SelectionChangedEvent,
            Source = this
        });

    void ListView_GotFocus(object sender, RoutedEventArgs e)
    {
        var lbi = e.OriginalSource as ListBoxItem;
        if (lbi != null)
            currentItem = lbi.DataContext;
        RaiseEvent(new CurrentItemChangedEventArgs(lbi)
        {
            RoutedEvent = CurrentItemChangedEvent,
            Source = this
        });
    }

    void ListView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case System.Windows.Input.Key.Enter:
                RaiseEvent(new RoutedEventArgs(OnEnterEvent)
                {
                    Source = this
                });
                break;
        }
    }

    void Headers_SortChanged(object sender, SortChangedEventArgs e)
    {
        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(ListView.ItemsSource);
        view.CustomSort = new DirectoryComparer(e.Index, e.Descending);
        view.Refresh();
        FocusCurrentItem();
    }

    object? currentItem;
}

