using System.Windows;
using System.Windows.Controls;
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
                };
                if (header.Alignment == TextAlignment.Right)
                    tb.TextAlignment = TextAlignment.Right;
                var border = new Border()
                {
                    BorderThickness = new Thickness(0, 0, 1, 0)
                };
                border.SetValue(Grid.ColumnProperty, idx++);
                border.SetResourceReference(Border.BorderBrushProperty, "MyBorderBrush");
                border.Child = tb;
                Grid.Children.Add(border);
            }
        }
    } = [];

    public ColumnViewHeader() => InitializeComponent();
}

public record ColumnViewHeaderItem(string Name, TextAlignment Alignment = TextAlignment.Left);
