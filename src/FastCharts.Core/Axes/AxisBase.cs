using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes;

public abstract class AxisBase
{
    public FRange DataRange { get; set; }
    public FRange VisibleRange { get; set; }
    public string? LabelFormat { get; set; }
    public bool ShowMinorTicks { get; set; } = true;
    public bool ShowMinorGrid { get; set; } = true;
    public double MinorGridOpacity { get; set; } = 0.4;
    protected AxisBase()
    {
        DataRange = new FRange(0, 1);
        VisibleRange = new FRange(0, 1);
        LabelFormat = "G";
    }
    public abstract void UpdateScale(double pixelMin, double pixelMax);
}
