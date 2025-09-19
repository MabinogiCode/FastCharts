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
}
