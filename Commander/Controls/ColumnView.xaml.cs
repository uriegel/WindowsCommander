using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Commander.Extensions;

namespace Commander;

// TODO ListView_SelectionChanged => Controller
// TODO DirectoryController
// TODO Eliminate TestControl when DirectoryController is done
// TODO Shift Tab: focus path textBox

public partial class ColumnView : UserControl
{
    public ColumnView() => InitializeComponent();

    void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Return:

                ////(DataContext as ItemViewContext).ChangePath.Execute((sender as TextBox).Text);
                //var items = Directory.GetItems((sender as TextBox).Text);
                //this.listView.ItemsSource = items;
                //e.Handled = true;
                //liste.FocusItem((DataContext as ItemViewContext).View.CurrentItem as Item, true);


                Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
                {
                    var ripple = new WaterRipple
                    {
                        Amplitude = 10,
                        RatioControl = 2,
                        Frequency = 35
                    };
                    ListView.Effect = ripple;
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
            ListView.Effect = null;
        }
    }

    void TextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() => (sender as TextBox)?.SelectAll());
        e.Handled = true;
    }

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

