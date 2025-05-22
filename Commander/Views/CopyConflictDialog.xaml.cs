using System.Windows;
using System.Windows.Data;

using Commander.Controllers;
using Commander.Controllers.Directory;
using Commander.Controls.ColumnViewHeader;

namespace Commander.Views;

public partial class CopyConflictDialog : Window
{
    public CopyConflictDialog()
    {
        InitializeComponent();

        Owner = Application.Current.MainWindow;

        Title = "Dateien kopieren";

        ColumnView.Headers.HeaderItems = [ new HeaderItem("Name"), new HeaderItem("Datum"), new HeaderItem("Größe", TextAlignment.Right)];

        Item[] items = 
            [
                new FileItem() { DateTime = DateTime.Now, Name = "Der Name", Size = 1243 },
                new FileItem() { DateTime = DateTime.Now - TimeSpan.FromHours(1), Name = "Noch einer", Size = 4321 }
            ];
        var oldView = CollectionViewSource.GetDefaultView(ColumnView.ListView.ItemsSource) as ListCollectionView;
        var view = new ListCollectionView(items.ToList())
        {
            CustomSort = oldView?.CustomSort,
        };
        ColumnView.ListView.ItemsSource = view;

        var ctx = new ColumnViewContext();
        ColumnView.DataContext = ctx;
        ColumnView.Headers.ColumnViewContext = ctx;
    }
}
