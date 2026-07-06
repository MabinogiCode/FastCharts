using System;

using FastCharts.Core.Primitives;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Axes;

public abstract class AxisBase
{
    private FRange _visibleRange;

    public FRange DataRange { get; set; }

    /// <summary>
    /// Currently visible range. Raises <see cref="VisibleRangeChanged"/> when it actually
    /// changes, which enables cross-chart axis linking (see ChartLinkGroup).
    /// </summary>
    public FRange VisibleRange
    {
        get => _visibleRange;
        set
        {
            if (DoubleUtils.AreEqual(value.Min, _visibleRange.Min) && DoubleUtils.AreEqual(value.Max, _visibleRange.Max))
            {
                return;
            }

            _visibleRange = value;
            VisibleRangeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Raised after <see cref="VisibleRange"/> changes (zoom, pan, auto-fit, linked axes).
    /// </summary>
    public event EventHandler? VisibleRangeChanged;

    public string? LabelFormat { get; set; }

    public bool ShowMinorTicks { get; set; } = true;

    public bool ShowMinorGrid { get; set; } = true;

    public double MinorGridOpacity { get; set; } = 0.4;

    protected AxisBase()
    {
        DataRange = new FRange(0, 1);
        _visibleRange = new FRange(0, 1);
        LabelFormat = "G";
    }

    public abstract void UpdateScale(double pixelMin, double pixelMax);
}
