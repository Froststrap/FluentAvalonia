using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace FluentAvalonia.Converters;

public class CornerRadiusToClipGeometryConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count != 3)
            return null;

        if (!(values[0] is double width) || !(values[1] is double height) || !(values[2] is CornerRadius cr))
            return null;

        if (width <= 0 || height <= 0 || double.IsNaN(width) || double.IsNaN(height))
            return null;

        double maxRadius = Math.Min(width, height) / 2;
        var tl = Math.Min(cr.TopLeft, maxRadius);
        var tr = Math.Min(cr.TopRight, maxRadius);
        var br = Math.Min(cr.BottomRight, maxRadius);
        var bl = Math.Min(cr.BottomLeft, maxRadius);

        var geometry = new PathGeometry();
        var figure = new PathFigure
        {
            StartPoint = new Point(tl, 0),
            IsClosed = true
        };

        figure.Segments.Add(new LineSegment { Point = new Point(width - tr, 0) });
        figure.Segments.Add(new ArcSegment
        {
            Point = new Point(width, tr),
            Size = new Size(tr, tr),
            RotationAngle = 0,
            IsLargeArc = false,
            SweepDirection = SweepDirection.Clockwise
        });
        figure.Segments.Add(new LineSegment { Point = new Point(width, height - br) });
        figure.Segments.Add(new ArcSegment
        {
            Point = new Point(width - br, height),
            Size = new Size(br, br),
            RotationAngle = 0,
            IsLargeArc = false,
            SweepDirection = SweepDirection.Clockwise
        });
        figure.Segments.Add(new LineSegment { Point = new Point(bl, height) });
        figure.Segments.Add(new ArcSegment
        {
            Point = new Point(0, height - bl),
            Size = new Size(bl, bl),
            RotationAngle = 0,
            IsLargeArc = false,
            SweepDirection = SweepDirection.Clockwise
        });
        figure.Segments.Add(new LineSegment { Point = new Point(0, tl) });
        figure.Segments.Add(new ArcSegment
        {
            Point = new Point(tl, 0),
            Size = new Size(tl, tl),
            RotationAngle = 0,
            IsLargeArc = false,
            SweepDirection = SweepDirection.Clockwise
        });

        geometry.Figures.Add(figure);
        return geometry;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
