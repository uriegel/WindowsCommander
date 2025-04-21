using System.Windows.Controls;

namespace Commander;

public partial class ColumnView : UserControl
{
    public ColumnView()
    {
        InitializeComponent();
    }
}

// TODO Menu with Actions and Shortcuts
// TODO In TestView controlling Selections via Keybord: Ins, Ctrl+A, Shift End, Shift Home
// TODO Don't select parent item
// TODO HeaderControl with a variable number of Headers
// TODO HeaderControl to TestView
// TODO Resize HeaderColumns
// TODO ColumnView with HeaderControl and Controller
// TODO Controller with DataContext, HeaderControl