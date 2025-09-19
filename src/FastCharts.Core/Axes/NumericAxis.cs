using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Scales;
using FastCharts.Core.Ticks;

namespace FastCharts.Core.Axes;

public sealed class NumericAxis : IAxis<double>
{
    public NumericAxis()
    {
        Scale = new LinearScale(0, 1, 0, 1);
        Ticker = new NumericTicker();
        DataRange = new FRange(0, 1);
        VisibleRange = new FRange(0, 1);
    }

    public IScale<double> Scale { get; private set; }
    public ITicker<double> Ticker { get; }
    public FRange DataRange { get; set; }
    public FRange VisibleRange { get; set; }
    public string? LabelFormat { get; set; } = "G";

    public void UpdateScale(double pixelMin, double pixelMax)
    {
        Scale = new LinearScale(VisibleRange.Min, VisibleRange.Max, pixelMin, pixelMax);
    }
    
    /// <summary>
    /// Updates the visible range. Guards against NaN, inverted or zero-length ranges.
    /// </summary>
    public void SetVisibleRange(double min, double max)
    {
        if (double.IsNaN(min) || double.IsNaN(max))
            return;

        if (min > max)
        {
            var t = min; min = max; max = t;
        }

        // Avoid zero-length range (bad for scales)
        if (min == max)
        {
            var eps = (min == 0d) ? 1e-6 : System.Math.Abs(min) * 1e-6;
            min -= eps;
            max += eps;
        }

        VisibleRange = new FRange(min, max);
    }
}
