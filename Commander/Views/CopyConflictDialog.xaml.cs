using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Commander.Controllers;
using Commander.Controllers.Directory;
using Commander.Controls;
using Commander.Controls.ColumnViewHeader;

namespace Commander.Views;

public partial class CopyConflictDialog : Window
{
    public CopyConflictDialog(bool move, CopyItem[] copyItems)
    {
        InitializeComponent();

        Owner = Application.Current.MainWindow;

        Title = move ? "Dateien verschieben" : "Dateien kopieren";

        ColumnView.Headers.HeaderItems = 
        [ 
            new HeaderItem("Name"), 
            new HeaderItem("Datum"), 
            new HeaderItem("Größe", TextAlignment.Right), 
            new HeaderItem("Version")
        ];

        var oldView = CollectionViewSource.GetDefaultView(ColumnView.ListView.ItemsSource) as ListCollectionView;
        var view = new ListCollectionView(copyItems.ToList())
        {
            CustomSort = oldView?.CustomSort,
        };
        ColumnView.ListView.ItemsSource = view;
        ColumnView.ListView.Focus();
        if (ColumnView.ListView.Items.Count > 0)
        {
            Focus();

            async void Focus()
            {
                await Task.Delay(500);
                var currentItem = ColumnView.ListView.Items[0];
                ColumnView.ListView.ScrollIntoView(currentItem);
                UpdateLayout();
                var listViewItem = ColumnView.ListView.ItemContainerGenerator.ContainerFromItem(currentItem) as ListViewItem;
                listViewItem?.Focus();
            }
        }
        var ctx = new ColumnViewContext();
        ColumnView.DataContext = ctx;
        ColumnView.Headers.ColumnViewContext = ctx;

        NoButton.IsDefault = true;
    }

    void YesButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    void NoButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    void DockPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }
}
