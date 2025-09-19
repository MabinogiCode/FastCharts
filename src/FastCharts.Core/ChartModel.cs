using System.Collections.Generic;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using System.Collections.ObjectModel;
using System.Linq;

using FastCharts.Core.Themes.BuiltIn;

namespace FastCharts.Core;

public sealed class ChartModel : IChartModel
{
    public ChartModel()
    {
        XAxis = new NumericAxis();
        YAxis = new NumericAxis();
        Viewport = new Interactivity.Viewport(new FRange(0, 1), new FRange(0, 1));

        Series = new ObservableCollection<object>();
        Axes = new ReadOnlyCollection<object>(new object[] { XAxis, YAxis });
    }

    public ITheme Theme { get; set; } = new LightTheme();
    public NumericAxis XAxis { get; }
    public NumericAxis YAxis { get; }
    public IViewport Viewport { get; }
    public ObservableCollection<object> Series { get; }
    public IReadOnlyList<object> Axes { get; }

    // IChartModel (kept loose for now)
    IReadOnlyList<object> IChartModel.Axes => Axes;
    IReadOnlyList<object> IChartModel.Series => Series;
    
    public Margins PlotMargins { get; set; } = new Margins(48, 16, 16, 36);

    public void AddSeries(object series)
    {
        Series.Add(series);
        AutoFitDataRange();
    }

    public void ClearSeries()
    {
        Series.Clear();
        XAxis.DataRange = new FRange(0, 1);
        YAxis.DataRange = new FRange(0, 1);
        Viewport.SetVisible(XAxis.DataRange, YAxis.DataRange);
    }

    public void UpdateScales(double widthPx, double heightPx)
    {
        XAxis.VisibleRange = Viewport.X;
        YAxis.VisibleRange = Viewport.Y;
        XAxis.UpdateScale(0, widthPx);
        // Y pixels typically grow downward; invert here for convenience
        YAxis.UpdateScale(heightPx, 0);
    }

    public void AutoFitDataRange()
    {
        var xs = new List<FRange>();
        var ys = new List<FRange>();

        foreach (var s in Series)
        {
            if (s is Series.LineSeries ls && !ls.IsEmpty)
            {
                xs.Add(ls.GetXRange());
                ys.Add(ls.GetYRange());
            }
        }

        if (xs.Count == 0 || ys.Count == 0) return;

        var xMin = xs.Min(r => r.Min);
        var xMax = xs.Max(r => r.Max);
        var yMin = ys.Min(r => r.Min);
        var yMax = ys.Max(r => r.Max);

        if (xMin == xMax) { xMin -= 0.5; xMax += 0.5; }
        if (yMin == yMax) { yMin -= 0.5; yMax += 0.5; }

        XAxis.DataRange = new FRange(xMin, xMax);
        YAxis.DataRange = new FRange(yMin, yMax);
        Viewport.SetVisible(XAxis.DataRange, YAxis.DataRange);
    }
    
    public void ZoomAt(double factorX, double factorY, double centerDataX, double centerDataY)
    {
        // factor < 1 => zoom in; factor > 1 => zoom out
        var xr = XAxis.VisibleRange;
        var yr = YAxis.VisibleRange;

        var newSizeX = xr.Size * factorX;
        var newSizeY = yr.Size * factorY;

        // keep the center point fixed
        var newMinX = centerDataX - (centerDataX - xr.Min) * factorX;
        var newMaxX = newMinX + newSizeX;

        var newMinY = centerDataY - (centerDataY - yr.Min) * factorY;
        var newMaxY = newMinY + newSizeY;

        XAxis.SetVisibleRange(newMinX, newMaxX);
        YAxis.SetVisibleRange(newMinY, newMaxY);
    }

    public void Pan(double deltaDataX, double deltaDataY)
    {
        var xr = XAxis.VisibleRange;
        var yr = YAxis.VisibleRange;

        XAxis.SetVisibleRange(xr.Min + deltaDataX, xr.Max + deltaDataX);
        YAxis.SetVisibleRange(yr.Min + deltaDataY, yr.Max + deltaDataY);
    }
}
