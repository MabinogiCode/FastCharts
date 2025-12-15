namespace FastCharts.Core.Tests;

/// <summary>
/// Helper class for testing dual-selector overload
/// </summary>
public class TestPoint
{
    public double Low { get; }
    public double High { get; }

    public TestPoint(double low, double high)
    {
        Low = low;
        High = high;
    }
}