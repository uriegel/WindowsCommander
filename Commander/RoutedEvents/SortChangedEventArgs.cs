using System.Windows;

namespace Commander.RoutedEvents;

public class SortChangedEventArgs : RoutedEventArgs
{
    internal SortChangedEventArgs(int index, bool descending)
    {
        Index = index;
        Descending = descending;
    } 
    
    public int Index {  get; }
    public bool Descending { get; }
}
