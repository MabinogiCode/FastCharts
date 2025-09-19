namespace FastCharts.Core.Primitives;

public readonly struct RectD(double x, double y, double width, double height)
{
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Width { get; } = width;
    public double Height { get; } = height;
}
