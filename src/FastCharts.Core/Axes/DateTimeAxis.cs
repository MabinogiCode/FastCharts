using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Primitives;
using FastCharts.Core.Scales;

namespace FastCharts.Core.Axes;

/// <summary>
/// DateTime axis with OADate (double) backing for scale/ticks interoperability.
/// VisibleRange/DataRange are expressed in OADate doubles.
/// </summary>
public sealed class DateTimeAxis : AxisBase, IAxis<double>
{
    public DateTimeAxis()
    {
        Scale = new LinearScale(0, 1, 0, 1);
        Ticker = new NiceTicker(); // later: DateTicker with smart steps
        DataRange = new FRange(DateTime.Today.AddDays(-7).ToOADate(), DateTime.Today.ToOADate());
        VisibleRange = DataRange;
        LabelFormat = "yyyy-MM-dd";
    }

    public IScale<double> Scale { get; private set; }
    public ITicker<double> Ticker { get; }

    public override void UpdateScale(double pixelMin, double pixelMax)
    {
        Scale = new LinearScale(VisibleRange.Min, VisibleRange.Max, pixelMin, pixelMax);
    }

    public void SetVisibleRange(DateTime min, DateTime max)
    {
        if (min > max)
        {
            var t = min; min = max; max = t;
        }
        VisibleRange = new FRange(min.ToOADate(), max.ToOADate());
    }

    public void SetVisibleRange(double minOADate, double maxOADate)
    {
        if (minOADate > maxOADate)
        {
            var t = minOADate; minOADate = maxOADate; maxOADate = t;
        }
        VisibleRange = new FRange(minOADate, maxOADate);
    }
}
