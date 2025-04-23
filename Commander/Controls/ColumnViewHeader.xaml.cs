using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Commander.Extensions;

namespace Commander;

public partial class ColumnViewHeader : UserControl
{
    public ColumnViewHeaderItem[] HeaderItems 
    { 
        get => field;
        set
        {
            field = value;

            var idx = 0;
            foreach (var item in field)
                item.Index = idx++;

            Grid.ItemsSource = field;
            Grid.MouseMove += OnMouseMove;
            // border.SetResourceReference(Border.BorderBrushProperty, "MyBorderBrush");
        }
    } = [];

    public ColumnViewHeader() => InitializeComponent();

    void OnMouseMove(object? sender, MouseEventArgs e)
    {
        var dO = e.OriginalSource as DependencyObject;
        var border = dO?.FindAncestorOrSelf<Border>();
        if (border != null)
        {
            if (!dragging)
            {
                var pos = e.GetPosition(border);
                if (pos.X < 10 && (border.DataContext as ColumnViewHeaderItem)?.Index != 0)
                {
                    border.Cursor = Cursors.ScrollWE;
                    if (e.LeftButton == MouseButtonState.Pressed && !dragging)
                    {
                        border.CaptureMouse();
                        border.MouseLeftButtonUp += OnMouseUp;
                        dragging = true;
                    }
                }
                else if (pos.X > border.ActualWidth - 10 && (border.DataContext as ColumnViewHeaderItem)?.Index != 2)
                {
                    border.Cursor = Cursors.ScrollWE;
                    if (e.LeftButton == MouseButtonState.Pressed && !dragging)
                    {
                        border.CaptureMouse();
                        border.MouseLeftButtonUp += OnMouseUp;
                        dragging = true;
                    }
                }
                else
                    border.Cursor = Cursors.Arrow;
            }
            else
            {
                border.Cursor = Cursors.ScrollWE;
                var pos = e.GetPosition(border);
            }
        }

        void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.MouseLeftButtonUp -= OnMouseUp;
                border.ReleaseMouseCapture();
                dragging = false;
                border.Cursor = Cursors.Arrow;
            }
        }
    }

    bool dragging;
}

public record ColumnViewHeaderItem(string Name, TextAlignment Alignment = TextAlignment.Left)
{
    public int Index { get; set; }
};
