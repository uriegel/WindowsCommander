using System.Windows;
using System.Windows.Data;

using Commander.Controllers;
using Commander.Controllers.Directory;
using Commander.Controls.ColumnViewHeader;

namespace Commander.Views;

public partial class MessageDialog : Window
{
    public MessageDialog()
    {
        InitializeComponent();

        Owner = Application.Current.MainWindow;

        Title = "Dateien kopieren";
    }

    void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
