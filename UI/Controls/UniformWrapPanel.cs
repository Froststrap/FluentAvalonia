using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

public class UniformWrapPanel : Panel
{
    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<UniformWrapPanel, double>(nameof(MinItemWidth), 100.0);

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    private double _cachedCellHeight;

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = Children;
        if (children.Count == 0)
            return new Size(0, 0);

        int totalItems = children.Count;
        int columns;
        if (double.IsInfinity(availableSize.Width))
        {
            columns = totalItems;
        }
        else
        {
            int maxColumns = (int)(availableSize.Width / MinItemWidth);
            columns = Math.Max(1, Math.Min(totalItems, maxColumns));
        }

        double cellWidth = double.IsInfinity(availableSize.Width)
            ? MinItemWidth
            : availableSize.Width / columns;

        var cellConstraint = new Size(cellWidth, double.PositiveInfinity);
        double maxHeight = 0;
        foreach (var child in children)
        {
            child.Measure(cellConstraint);
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
        }

        _cachedCellHeight = maxHeight;

        var cellSize = new Size(cellWidth, _cachedCellHeight);
        foreach (var child in children)
        {
            child.Measure(cellSize);
        }

        int rowCount = (int)Math.Ceiling((double)totalItems / columns);
        double totalWidth = double.IsInfinity(availableSize.Width)
            ? columns * cellWidth
            : availableSize.Width;
        double totalHeight = rowCount * _cachedCellHeight;

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children;
        int totalItems = children.Count;
        if (totalItems == 0)
            return finalSize;

        int columns;
        if (double.IsInfinity(finalSize.Width))
        {
            columns = totalItems;
        }
        else
        {
            int maxColumns = (int)(finalSize.Width / MinItemWidth);
            columns = Math.Max(1, Math.Min(totalItems, maxColumns));
        }

        double cellWidth = double.IsInfinity(finalSize.Width)
            ? MinItemWidth
            : finalSize.Width / columns;

        double cellHeight = _cachedCellHeight > 0 ? _cachedCellHeight : 100;

        int rowCount = (int)Math.Ceiling((double)totalItems / columns);
        int index = 0;
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (index >= totalItems)
                    break;
                var child = children[index];
                double x = col * cellWidth;
                double y = row * cellHeight;
                child.Arrange(new Rect(x, y, cellWidth, cellHeight));
                index++;
            }
        }

        return finalSize;
    }
}
