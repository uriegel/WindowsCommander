using System.Windows;
using System.Windows.Controls;

namespace Commander.RoutedEvents;

public class CurrentItemChangedEventArgs : RoutedEventArgs
{
    internal CurrentItemChangedEventArgs(ListBoxItem? currentItem)
        => CurrentItem = currentItem;

    public ListBoxItem? CurrentItem {  get; }
}
