using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;

namespace FastCharts.Core.Interaction.Behaviors;

/// <summary>
/// Projects data points into the plot rectangle (in surface pixels) for hit-testing,
/// honoring logarithmic axes. Points outside the visible ranges are rejected instead of
/// being clamped onto the plot border.
/// </summary>
internal readonly struct PlotProjection
{
    public PlotProjection(double left, double top, double width, double height, double cursorX, double cursorY)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
        CursorX = cursorX;
        CursorY = cursorY;
    }

    public double Left { get; }

    public double Top { get; }

    public double Width { get; }

    public double Height { get; }

    public double CursorX { get; }

    public double CursorY { get; }

    public bool TryProject(IAxis<double> xAxis, IAxis<double> yAxis, double x, double y, out double px, out double py)
    {
        var tX = AxisCoordinates.ToNormalized(xAxis, x);
        var tY = AxisCoordinates.ToNormalized(yAxis, y);
        px = Left + (tX * Width);
        py = Top + ((1 - tY) * Height);

        // NaN fails both comparisons, so non-finite values are rejected as well
        return tX >= 0 && tX <= 1 && tY >= 0 && tY <= 1;
    }

    public double DistanceSquared(IAxis<double> xAxis, IAxis<double> yAxis, double x, double y)
    {
        if (!TryProject(xAxis, yAxis, x, y, out var px, out var py))
        {
            return double.PositiveInfinity;
        }

        var dx = px - CursorX;
        var dy = py - CursorY;
        return (dx * dx) + (dy * dy);
    }
}
