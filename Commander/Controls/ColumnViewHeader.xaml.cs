using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Commander.Extensions;

namespace Commander;

// TODO: adapt stars in ColumnViewContext
// TODO: multiple files
public partial class ColumnViewHeader : UserControl
{
    #region Attaches Properties

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached(
        "Columns", typeof(ColumnViewHeaderContext), typeof(ColumnViewHeader), new PropertyMetadata(null, ColumnsChanged));

    public static ColumnViewHeaderContext GetColumns(DependencyObject obj) => (ColumnViewHeaderContext)obj.GetValue(ColumnsProperty);

    public static void SetColumns(DependencyObject obj, ColumnViewHeaderContext value) => obj.SetValue(ColumnsProperty, value);

    public static void ColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not Grid)
            return;

        var ctx = (ColumnViewHeaderContext)e.NewValue;
        var grid = (Grid)obj;
        grid.ColumnDefinitions.Clear();

        var idx = 0;
        foreach (var col in ctx.StarLength.Cast<double>())
        {
            var coldef = new ColumnDefinition();
            var binding = new Binding
            {
                Source = e.NewValue,
                Path = new PropertyPath($"StarLength[{idx++}]"),
                Converter = new GridLengthConverter()
            };
            BindingOperations.SetBinding(coldef, ColumnDefinition.WidthProperty, binding);
            grid.ColumnDefinitions.Add(coldef);
        }
    }

    #endregion 

    public ColumnViewHeaderItem[] HeaderItems 
    { 
        get => field;
        set
        {
            field = value;

            var idx = 0;
            foreach (var item in field)
                item.Index = idx++;

            Headers.ItemsSource = field;
            Headers.MouseMove += OnMouseMove;
            // border.SetResourceReference(Border.BorderBrushProperty, "MyBorderBrush");
        }
    } = [];

    public ColumnViewHeader()
    {
        DataContext = new ColumnViewHeaderContext { StarLength = [1, 1, 1] };
        InitializeComponent();
    }

    double start = 0;
    int startIndex = 0;
    double[] stars = [];
    bool first;

    void OnMouseMove(object? sender, MouseEventArgs e)
    {
        var dO = e.OriginalSource as DependencyObject;
        var border = dO?.FindAncestorOrSelf<Border>();
        if (border != null 
                && border.DataContext is ColumnViewHeaderItem item 
                && DataContext is ColumnViewHeaderContext ctx)
        {
            if (!dragging)
            {
                var pos = e.GetPosition(border);
                if (pos.X < 10 && item.Index != 0)
                {
                    border.Cursor = Cursors.ScrollWE;
                    if (e.LeftButton == MouseButtonState.Pressed && !dragging)
                    {
                        InitializeDrag();
                        first = false;
                        startIndex = item.Index - 1;
                        start = pos.X;
                    }
                }
                else if (pos.X > border.ActualWidth - 10 && item.Index < ctx.StarLength.Length - 1)
                {
                    border.Cursor = Cursors.ScrollWE;
                    if (e.LeftButton == MouseButtonState.Pressed && !dragging)
                    {
                        InitializeDrag();
                        first = true;
                        startIndex = item.Index;
                        start = pos.X;
                    }
                }
                else
                    border.Cursor = Cursors.Arrow;
            }
            else 
            {
                border.Cursor = Cursors.ScrollWE;
                var pos = e.GetPosition(border);
                var newStars = new double[ctx.StarLength.Length];
                if (first)
                    stars.CopyTo(newStars);
                else
                    ctx.StarLength.CopyTo(newStars);
                var factor = (pos.X - start);
                newStars[startIndex] += factor;
                newStars[startIndex + 1] -= factor;
                if (newStars[startIndex] > 10 && newStars[startIndex + 1] > 10)
                    ctx.StarLength = newStars;

            }

            void InitializeDrag()
            {
                var grid = dO?.FindAncestorOrSelf<Grid>();
                var cols = grid?.Children.Cast<ContentPresenter>()?.ToArray();
                border.CaptureMouse();
                border.MouseLeftButtonUp += OnMouseUp;
                stars = new double[ctx.StarLength.Length];
                ctx.StarLength.CopyTo(stars);
                for (var i = 0; i < stars.Length; i++)
                    stars[i] = cols?[i].ActualWidth ?? 0;
                ctx.StarLength = stars;
                dragging = true;
            }

            void OnMouseUp(object? sender, MouseEventArgs e)
            {
                if (sender is Border border && DataContext is ColumnViewHeaderContext ctx)
                {
                    border.MouseLeftButtonUp -= OnMouseUp;
                    border.ReleaseMouseCapture();
                    dragging = false;
                    border.Cursor = Cursors.Arrow;
                }
            }
        }
    }

    bool dragging;
}

public class ColumnViewHeaderContext : INotifyPropertyChanged
{
    public double[] StarLength
    {
        get => field;
        set
        {
            field = value;
            OnChanged(nameof(StarLength));
        }
    } = [];


    public event PropertyChangedEventHandler? PropertyChanged;

    void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public record ColumnViewHeaderItem(string Name, TextAlignment Alignment = TextAlignment.Left)
{
    public int Index { get; set; }
};

public class GridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        => new GridLength((double)value, GridUnitType.Star);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}