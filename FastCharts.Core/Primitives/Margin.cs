namespace FastCharts.Core.Primitives;

public readonly struct Margin(double left, double top, double right, double bottom)
{
    public double Left { get; } = left;
    public double Top { get; } = top;
    public double Right { get; } = right;
    public double Bottom { get; } = bottom;
}
