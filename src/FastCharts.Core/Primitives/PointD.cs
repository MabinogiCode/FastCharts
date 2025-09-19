namespace FastCharts.Core.Primitives;

public readonly struct PointD(double x, double y)
{
    public double X { get; } = x;
    public double Y { get; } = y;
}
