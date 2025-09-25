using System.Collections.Generic;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using System.Collections.ObjectModel;
using System.Linq;

using FastCharts.Core.Interaction;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Legend; // added
using FastCharts.Core.Series;

namespace FastCharts.Core;

public sealed class ChartModel : IChartModel
{
    private readonly List<AxisBase> _axesList;
    private readonly ReadOnlyCollection<AxisBase> _axesView;

    public ChartModel()
    {
        var x = new NumericAxis();
        var y = new NumericAxis();
        XAxis = x;
        YAxis = y;
        Viewport = new Interactivity.Viewport(new FRange(0, 1), new FRange(0, 1));

        Series = new ObservableCollection<SeriesBase>();
        _axesList = new List<AxisBase> { (AxisBase)XAxis, (AxisBase)YAxis };
        _axesView = new ReadOnlyCollection<AxisBase>(_axesList);
        Legend = new LegendModel();
    }

    public ITheme Theme { get; set; } = new LightTheme();
    public IAxis<double> XAxis { get; private set; }
    public IAxis<double> YAxis { get; private set; }
    public IViewport Viewport { get; }
    public ObservableCollection<SeriesBase> Series { get; }
    public IReadOnlyList<AxisBase> Axes => _axesView;

    // IChartModel (kept loose for now)
    IReadOnlyList<AxisBase> IChartModel.Axes => Axes;
    IReadOnlyList<SeriesBase> IChartModel.Series => Series;
    
    public Margins PlotMargins { get; set; } = new Margins(48, 16, 16, 36);
    
    /// <summary>Legend model describing series entries.</summary>
    public LegendModel Legend { get; }
    
    /// <summary>Ordered list of behaviors attached to this model.</summary>
    public IList<IBehavior> Behaviors { get; } = new List<IBehavior>();

    /// <summary>Shared interaction state that renderers may read to draw overlays.</summary>
    public InteractionState? InteractionState { get; set; }

    public void ReplaceXAxis(IAxis<double> newAxis)
    {
        if (newAxis == null)
        {
            return;
        }
        var old = XAxis;
        // Preserve data range; visible will be set from viewport during UpdateScales
        newAxis.DataRange = old.DataRange;
        XAxis = newAxis;
        if (_axesList.Count > 0)
        {
            _axesList[0] = (AxisBase)newAxis;
        }
    }

    public void ReplaceYAxis(IAxis<double> newAxis)
    {
        if (newAxis == null)
        {
            return;
        }
        var old = YAxis;
        newAxis.DataRange = old.DataRange;
        YAxis = newAxis;
        if (_axesList.Count > 1)
        {
            _axesList[1] = (AxisBase)newAxis;
        }
    }

    public void AddSeries(SeriesBase series)
    {
        Series.Add(series);
        Legend.SyncFromSeries(Series); // keep legend updated
        AutoFitDataRange();
    }

    public void ClearSeries()
    {
        Series.Clear();
        Legend.SyncFromSeries(Series);
        XAxis.DataRange = new FRange(0, 1);
        YAxis.DataRange = new FRange(0, 1);
        Viewport.SetVisible(XAxis.DataRange, YAxis.DataRange);
    }

    public void UpdateScales(double widthPx, double heightPx)
    {
        XAxis.VisibleRange = Viewport.X;
        YAxis.VisibleRange = Viewport.Y;
        ((AxisBase)XAxis).UpdateScale(0, widthPx);
        // Y pixels typically grow downward; invert here for convenience
        ((AxisBase)YAxis).UpdateScale(heightPx, 0);
    }

    public void AutoFitDataRange()
    {
        var xs = new List<FRange>();
        var ys = new List<FRange>();

        foreach (var s in Series)
        {
            if (s is LineSeries ls && !ls.IsEmpty)
            {
                xs.Add(ls.GetXRange());
                ys.Add(ls.GetYRange());
            }
            else if (s is ScatterSeries sc && !sc.IsEmpty)
            {
                xs.Add(sc.GetXRange());
                ys.Add(sc.GetYRange());
            }
            else if (s is BandSeries bs && !bs.IsEmpty)
            {
                // Band series X range from its points, Y from min(low) to max(high)
                var minX = bs.Data.Min(p => p.X);
                var maxX = bs.Data.Max(p => p.X);
                var minY = bs.Data.Min(p => p.YLow);
                var maxY = bs.Data.Max(p => p.YHigh);
                xs.Add(new FRange(minX, maxX));
                ys.Add(new FRange(minY, maxY));
            }
        }

        if (xs.Count == 0 || ys.Count == 0) { return; }

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

        // apply with guard against zero-length
        if (newMinX == newMaxX) { newMinX -= 1e-6; newMaxX += 1e-6; }
        if (newMinY == newMaxY) { newMinY -= 1e-6; newMaxY += 1e-6; }

        XAxis.VisibleRange = new FRange(newMinX, newMaxX);
        YAxis.VisibleRange = new FRange(newMinY, newMaxY);
    }

    public void Pan(double deltaDataX, double deltaDataY)
    {
        var xr = XAxis.VisibleRange;
        var yr = YAxis.VisibleRange;

        XAxis.VisibleRange = new FRange(xr.Min + deltaDataX, xr.Max + deltaDataX);
        YAxis.VisibleRange = new FRange(yr.Min + deltaDataY, yr.Max + deltaDataY);
    }
}
