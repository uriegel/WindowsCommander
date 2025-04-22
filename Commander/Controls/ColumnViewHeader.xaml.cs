using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Win32;

namespace Commander;
/// <summary>
/// Interaktionslogik für ColumnViewHader.xaml
/// </summary>
public partial class ColumnViewHeader : UserControl
{
    public ColumnViewHeaderItem[] HeaderItems 
    { 
        get => field;
        set
        {
            field = value;

            var idx = 0;
            foreach (var header in field)
            {
                Grid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
                var tb = new TextBlock()
                {
                    Text = header.Name,
                    Margin = new Thickness(5, 0, 5, 0),
                };
                if (header.Alignment == TextAlignment.Right)
                    tb.TextAlignment = TextAlignment.Right;
                var border = new Border()
                {
                    BorderThickness = idx < field.Length - 1 ? new Thickness(0, 0, 1, 0) : new Thickness(0),
                    Background = Brushes.Transparent,
                    Child = tb
                };
                border.MouseMove += OnMouseMove;
                border.SetValue(Grid.ColumnProperty, idx++);
                border.SetResourceReference(Border.BorderBrushProperty, "MyBorderBrush");
                Grid.Children.Add(border);
                firstBorder = Grid.Children[0] as Border;
                lastBorder = Grid.Children.Cast<Border>().LastOrDefault();
            }
        }
    } = [];

    public ColumnViewHeader() => InitializeComponent();

    void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            if (!dragging)
            {
                var pos = e.GetPosition(border);
                if (pos.X < 10 && border != firstBorder)
                {
                    border.Cursor = Cursors.ScrollWE;
                    if (e.LeftButton == MouseButtonState.Pressed && !dragging)
                    {
                        border.CaptureMouse();
                        border.MouseLeftButtonUp += OnMouseUp;
                        dragging = true;
                    }
                }
                else if (pos.X > border.ActualWidth - 10 && border != lastBorder)
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
    Border? firstBorder;
    Border? lastBorder;
}

public record ColumnViewHeaderItem(string Name, TextAlignment Alignment = TextAlignment.Left);
