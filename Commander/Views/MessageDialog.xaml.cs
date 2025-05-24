using System.Windows;
using System.Windows.Data;

using Commander.Controllers;
using Commander.Controllers.Directory;
using Commander.Controls.ColumnViewHeader;

namespace Commander.Views;

public partial class MessageDialog : Window
{
    public MessageDialog(string title, string message)
    {
        InitializeComponent();

        Owner = Application.Current.MainWindow;

        Title = title;
        Message.Content = message;
        OkButton.Focus();
    }

    void Button_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}