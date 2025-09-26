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
    /// <summary>Optional secondary Y axis (index 1) when present. Lazily created.</summary>
    public IAxis<double>? YAxisSecondary { get; private set; }
    public IViewport Viewport { get; }
    public ObservableCollection<SeriesBase> Series { get; }
    public IReadOnlyList<AxisBase> Axes => _axesView;
    IReadOnlyList<AxisBase> IChartModel.Axes => Axes;
    IReadOnlyList<SeriesBase> IChartModel.Series => Series;
    public Margins PlotMargins { get; set; } = new Margins(48, 16, 16, 36);
    public LegendModel Legend { get; }
    public IList<IBehavior> Behaviors { get; } = new List<IBehavior>();
    public InteractionState? InteractionState { get; set; }
    public void EnsureSecondaryYAxis()
    {
        if (YAxisSecondary != null) return;
        YAxisSecondary = new NumericAxis();
        _axesList.Add((AxisBase)YAxisSecondary);
    }
    public void ReplaceXAxis(IAxis<double> newAxis)
    {
        if (newAxis == null) return;
        var old = XAxis;
        newAxis.DataRange = old.DataRange;
        XAxis = newAxis;
        if (_axesList.Count > 0) _axesList[0] = (AxisBase)newAxis;
    }
    public void ReplaceYAxis(IAxis<double> newAxis)
    {
        if (newAxis == null) return;
        var old = YAxis;
        newAxis.DataRange = old.DataRange;
        YAxis = newAxis;
        if (_axesList.Count > 1) _axesList[1] = (AxisBase)newAxis;
    }
    public void ReplaceSecondaryYAxis(IAxis<double> newAxis)
    {
        EnsureSecondaryYAxis();
        if (newAxis == null || YAxisSecondary == null) return;
        newAxis.DataRange = YAxisSecondary.DataRange;
        YAxisSecondary = newAxis;
        _axesList[2] = (AxisBase)newAxis;
    }
    public void AddSeries(SeriesBase series)
    {
        Series.Add(series);
        Legend.SyncFromSeries(Series);
        AutoFitDataRange();
    }
    public void ClearSeries()
    {
        Series.Clear();
        Legend.SyncFromSeries(Series);
        XAxis.DataRange = new FRange(0, 1);
        YAxis.DataRange = new FRange(0, 1);
        if (YAxisSecondary != null) YAxisSecondary.DataRange = new FRange(0, 1);
        Viewport.SetVisible(XAxis.DataRange, YAxis.DataRange);
    }
    public void UpdateScales(double widthPx, double heightPx)
    {
        XAxis.VisibleRange = Viewport.X;
        YAxis.VisibleRange = Viewport.Y;
        ((AxisBase)XAxis).UpdateScale(0, widthPx);
        ((AxisBase)YAxis).UpdateScale(heightPx, 0);
        if (YAxisSecondary != null)
        {
            // For now share primary viewport Y range; future: independent viewport or mapping
            YAxisSecondary.VisibleRange = YAxis.VisibleRange;
            ((AxisBase)YAxisSecondary).UpdateScale(heightPx, 0);
        }
    }
    public void AutoFitDataRange()
    {
        var xs = new List<FRange>();
        var ysPrimary = new List<FRange>();
        var ysSecondary = new List<FRange>();
        bool anySecondary = false;
        foreach (var s in Series)
        {
            if (s.IsEmpty || !s.IsVisible) continue;
            FRange xr; FRange yr;
            switch (s)
            {
                case LineSeries ls:
                    xr = ls.GetXRange(); yr = ls.GetYRange(); break;
                case ScatterSeries sc:
                    xr = sc.GetXRange(); yr = sc.GetYRange(); break;
                case BandSeries band:
                    var minX = band.Data.Min(p => p.X); var maxX = band.Data.Max(p => p.X);
                    var minY = band.Data.Min(p => p.YLow); var maxY = band.Data.Max(p => p.YHigh);
                    xr = new FRange(minX, maxX); yr = new FRange(minY, maxY); break;
                case BarSeries bar:
                    xr = bar.GetXRange(); yr = bar.GetYRange(); break;
                case StackedBarSeries sbar:
                    xr = sbar.GetXRange(); yr = sbar.GetYRange(); break;
                case OhlcSeries ohlc:
                    xr = ohlc.GetXRange(); yr = ohlc.GetYRange(); break;
                case ErrorBarSeries err:
                    xr = err.GetXRange(); yr = err.GetYRange(); break;
                default:
                    continue;
            }
            xs.Add(xr);
            if (s.YAxisIndex == 1)
            {
                anySecondary = true; ysSecondary.Add(yr);
            }
            else
            {
                ysPrimary.Add(yr);
            }
        }
        if (xs.Count == 0) return;
        var xMin = xs.Min(r => r.Min); var xMax = xs.Max(r => r.Max);
        if (xMin == xMax) { xMin -= 0.5; xMax += 0.5; }
        XAxis.DataRange = new FRange(xMin, xMax);
        if (ysPrimary.Count > 0)
        {
            var yMin = ysPrimary.Min(r => r.Min); var yMax = ysPrimary.Max(r => r.Max);
            if (yMin == yMax) { yMin -= 0.5; yMax += 0.5; }
            YAxis.DataRange = new FRange(yMin, yMax);
        }
        if (anySecondary)
        {
            EnsureSecondaryYAxis();
            if (ysSecondary.Count > 0)
            {
                var y2Min = ysSecondary.Min(r => r.Min); var y2Max = ysSecondary.Max(r => r.Max);
                if (y2Min == y2Max) { y2Min -= 0.5; y2Max += 0.5; }
                YAxisSecondary!.DataRange = new FRange(y2Min, y2Max);
            }
        }
        Viewport.SetVisible(XAxis.DataRange, YAxis.DataRange);
    }
    public void ZoomAt(double factorX, double factorY, double centerDataX, double centerDataY)
    {
        var xr = XAxis.VisibleRange; var yr = YAxis.VisibleRange;
        var newSizeX = xr.Size * factorX; var newSizeY = yr.Size * factorY;
        var newMinX = centerDataX - (centerDataX - xr.Min) * factorX; var newMaxX = newMinX + newSizeX;
        var newMinY = centerDataY - (centerDataY - yr.Min) * factorY; var newMaxY = newMinY + newSizeY;
        if (newMinX == newMaxX) { newMinX -= 1e-6; newMaxX += 1e-6; }
        if (newMinY == newMaxY) { newMinY -= 1e-6; newMaxY += 1e-6; }
        XAxis.VisibleRange = new FRange(newMinX, newMaxX); YAxis.VisibleRange = new FRange(newMinY, newMaxY);
        if (YAxisSecondary != null) YAxisSecondary.VisibleRange = YAxis.VisibleRange;
    }
    public void Pan(double deltaDataX, double deltaDataY)
    {
        var xr = XAxis.VisibleRange; var yr = YAxis.VisibleRange;
        XAxis.VisibleRange = new FRange(xr.Min + deltaDataX, xr.Max + deltaDataX);
        YAxis.VisibleRange = new FRange(yr.Min + deltaDataY, yr.Max + deltaDataY);
        if (YAxisSecondary != null) YAxisSecondary.VisibleRange = YAxis.VisibleRange;
    }
}
