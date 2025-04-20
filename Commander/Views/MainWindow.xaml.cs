using System.Windows;

namespace Commander;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Restore bounds
        if (Properties.Settings.Default.WindowWidth > 0 &&
            Properties.Settings.Default.WindowHeight > 0)
        {
            Top = Properties.Settings.Default.WindowTop;
            Left = Properties.Settings.Default.WindowLeft;
            Width = Properties.Settings.Default.WindowWidth;
            Height = Properties.Settings.Default.WindowHeight;
        }

        // Restore window state after layout is applied
        this.Loaded += (s, ev) => WindowState = Properties.Settings.Default.WindowState;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        Properties.Settings.Default.WindowTop = Top;
        Properties.Settings.Default.WindowLeft = Left;
        Properties.Settings.Default.WindowWidth = Width;
        Properties.Settings.Default.WindowHeight = Height;
        Properties.Settings.Default.WindowState = WindowState;

        Properties.Settings.Default.Save();
        base.OnClosing(e);
    }
}