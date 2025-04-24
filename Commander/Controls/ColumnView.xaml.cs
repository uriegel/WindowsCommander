using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Commander.Extensions;

namespace Commander.Controls;

// TODO ListView_SelectionChanged => Controller
// TODO DirectoryController
// TODO Eliminate TestControl when DirectoryController is done
// TODO Shift Tab: focus path textBox

public partial class ColumnView : UserControl
{
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
    {
        //var selected = e.AddedItems.OfType<Item>();
        //var toRemove = selected.Where(n => n.Name == "@AdvancedKeySettingsNotification.png");
        //ListView.SelectedItems.Remove(toRemove.FirstOrDefault());
        // TODO to Controller
        ListView.SelectedItems.Clear();
    }
}

