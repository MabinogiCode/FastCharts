using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Formatting;
using FastCharts.Core.Primitives;
using FastCharts.Core.Scales;

namespace FastCharts.Core.Axes;

public sealed class DateTimeAxis : AxisBase, IAxis<double>
{
    public DateTimeAxis()
    {
        Scale = new LinearScale(0, 1, 0, 1);
        Ticker = new DateTicker();
        DataRange = new FRange(DateTime.Today.AddDays(-7).ToOADate(), DateTime.Today.ToOADate());
        VisibleRange = DataRange;
        LabelFormat = "yyyy-MM-dd";
        DateTimeFormatter = new AdaptiveDateTimeFormatter();
    }
    public IScale<double> Scale { get; private set; }
    public ITicker<double> Ticker { get; }
    public IDateTimeFormatter? DateTimeFormatter { get; set; }
    public override void UpdateScale(double pixelMin, double pixelMax)
    {
        Scale = new LinearScale(VisibleRange.Min, VisibleRange.Max, pixelMin, pixelMax);
    }
    public void SetVisibleRange(DateTime min, DateTime max)
    {
        if (min > max)
        {
            (min, max) = (max, min);
        }
        VisibleRange = new FRange(min.ToOADate(), max.ToOADate());
    }
    public void SetVisibleRange(double minOADate, double maxOADate)
    {
        if (minOADate > maxOADate)
        {
            (minOADate, maxOADate) = (maxOADate, minOADate);
        }
        VisibleRange = new FRange(minOADate, maxOADate);
    }

    /// <summary>
    /// Converts an OLE Automation date (the X unit of date axes) to a <see cref="DateTime"/>,
    /// returning false instead of throwing for values outside the supported range — which a
    /// date axis reaches when zoomed out far beyond year 1 or 9999.
    /// </summary>
    public static bool TryFromOADate(double oaDate, out DateTime result)
    {
        if (double.IsNaN(oaDate) || oaDate <= -657435.0 || oaDate >= 2958466.0)
        {
            result = default;
            return false;
        }

        try
        {
            result = DateTime.FromOADate(oaDate);
            return true;
        }
        catch (ArgumentException)
        {
            result = default;
            return false;
        }
    }
}
