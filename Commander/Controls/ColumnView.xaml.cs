using System.Windows.Controls;

namespace Commander;

// TODO Eliminate TestControl
// TODO Shift Tab: focus path textBox

public partial class ColumnView : UserControl
{
    public ColumnView()
    {
        InitializeComponent();
        OnInitialize();
    }

    protected virtual void OnInitialize() { }

    void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {

    }

    void TextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
    {

    }
}

