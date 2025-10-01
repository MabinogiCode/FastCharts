using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using DynamicData;
using DynamicData.Binding;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Interaction;
using FastCharts.Core.Legend;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Utilities;

using ReactiveUI;

namespace FastCharts.Core;

/// <summary>
/// Enhanced ChartModel with DynamicData reactive collections for superior MVVM performance
/// </summary>
public sealed class ChartModelEnhanced : ReactiveObject, IChartModel, IDisposable
{
    private readonly ReadOnlyCollection<AxisBase> _axesView;
    private readonly CompositeDisposable _disposables = new();
    
    // DynamicData reactive collections
    private readonly SourceList<SeriesBase> _seriesSource = new();
    private readonly SourceList<IBehavior> _behaviorsSource = new();
    
    // Backing fields for reactive properties
    private ITheme _theme = new LightTheme();
    private IAxis<double> _xAxis;
    private IAxis<double> _yAxis;
    private IAxis<double>? _yAxisSecondary;
    private Margins _plotMargins = new Margins(48, 16, 16, 36);
    private InteractionState? _interactionState;
    private int _seriesCount;
    private int _visibleSeriesCount;
    private long _totalDataPoints;
    
    // Disposed state tracking
    private bool _disposed;

    public ChartModelEnhanced()
    {
        var x = new NumericAxis();
        var y = new NumericAxis();
        _xAxis = x;
        _yAxis = y;
        Viewport = new Interactivity.Viewport(new FRange(0, 1), new FRange(0, 1));
        
        var axesList = new List<AxisBase> { (AxisBase)XAxis, (AxisBase)YAxis };
        _axesView = new ReadOnlyCollection<AxisBase>(axesList);
        Legend = new LegendModel();
        
        // Initialize the properties with empty collections first
        Series = new ReadOnlyObservableCollection<SeriesBase>(new ObservableCollection<SeriesBase>());
        Behaviors = new ReadOnlyObservableCollection<IBehavior>(new ObservableCollection<IBehavior>());
        
        SetupReactiveCollections();
    }

    private void SetupReactiveCollections()
    {
        // ? Series collection with DynamicData
        _seriesSource.Connect()
            .Bind(out var seriesBinding)
            .DisposeMany() // Auto-dispose series if they implement IDisposable
            .Subscribe(_ => { /* Collection changed */ })
            .DisposeWith(_disposables);
        
        Series = seriesBinding;

        // ? Auto-sync legend when series change
        _seriesSource.Connect()
            .Subscribe(_ => Legend.SyncFromSeries(Series))
            .DisposeWith(_disposables);

        // ? Series count tracking
        _seriesSource.Connect()
            .Count()
            .Subscribe(count => SeriesCount = count)
            .DisposeWith(_disposables);

        // ? Visible series count tracking
        _seriesSource.Connect()
            .Filter(s => s.IsVisible && !s.IsEmpty)
            .Count()
            .Subscribe(count => VisibleSeriesCount = count)
            .DisposeWith(_disposables);

        // ? Total data points aggregation - simplified without WhenPropertyChanged
        _seriesSource.Connect()
            .Throttle(TimeSpan.FromMilliseconds(50)) // Debounce rapid changes
            .Subscribe(_ => UpdateTotalDataPoints())
            .DisposeWith(_disposables);

        // ? Auto-fit when visible series change
        _seriesSource.Connect()
            .Filter(s => s.IsVisible && !s.IsEmpty)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => AutoFitDataRange())
            .DisposeWith(_disposables);

        // ? Behaviors collection with DynamicData
        _behaviorsSource.Connect()
            .Bind(out var behaviorsBinding)
            .DisposeMany() // Auto-dispose behaviors
            .Subscribe()
            .DisposeWith(_disposables);
        
        Behaviors = behaviorsBinding;
    }

    /// <summary>
    /// Chart visual theme - supports reactive binding and change notifications
    /// </summary>
    public ITheme Theme
    {
        get => _theme;
        set => this.RaiseAndSetIfChanged(ref _theme, value);
    }

    /// <summary>
    /// Primary X axis - supports reactive binding
    /// </summary>
    public IAxis<double> XAxis
    {
        get => _xAxis;
        private set => this.RaiseAndSetIfChanged(ref _xAxis, value);
    }

    /// <summary>
    /// Primary Y axis - supports reactive binding
    /// </summary>
    public IAxis<double> YAxis
    {
        get => _yAxis;
        private set => this.RaiseAndSetIfChanged(ref _yAxis, value);
    }

    /// <summary>
    /// Optional secondary Y axis (index 1) when present. Lazily created.
    /// </summary>
    public IAxis<double>? YAxisSecondary
    {
        get => _yAxisSecondary;
        private set => this.RaiseAndSetIfChanged(ref _yAxisSecondary, value);
    }

    public IViewport Viewport { get; }

    /// <summary>
    /// Reactive observable collection of series with DynamicData enhancements
    /// </summary>
    public ReadOnlyObservableCollection<SeriesBase> Series { get; private set; }

    /// <summary>
    /// Reactive observable collection of behaviors with DynamicData enhancements
    /// </summary>
    public ReadOnlyObservableCollection<IBehavior> Behaviors { get; private set; }

    /// <summary>
    /// Total number of series (reactive property)
    /// </summary>
    public int SeriesCount
    {
        get => _seriesCount;
        private set => this.RaiseAndSetIfChanged(ref _seriesCount, value);
    }

    /// <summary>
    /// Number of visible series (reactive property)
    /// </summary>
    public int VisibleSeriesCount
    {
        get => _visibleSeriesCount;
        private set => this.RaiseAndSetIfChanged(ref _visibleSeriesCount, value);
    }

    /// <summary>
    /// Total data points across all series (reactive property)
    /// </summary>
    public long TotalDataPoints
    {
        get => _totalDataPoints;
        private set => this.RaiseAndSetIfChanged(ref _totalDataPoints, value);
    }

    public IReadOnlyList<AxisBase> Axes => _axesView;
    IReadOnlyList<AxisBase> IChartModel.Axes => Axes;
    IReadOnlyList<SeriesBase> IChartModel.Series => Series;

    /// <summary>
    /// Plot margins - supports reactive binding
    /// </summary>
    public Margins PlotMargins
    {
        get => _plotMargins;
        set => this.RaiseAndSetIfChanged(ref _plotMargins, value);
    }

    public LegendModel Legend { get; }

    /// <summary>
    /// Current interaction state - supports reactive binding
    /// </summary>
    public InteractionState? InteractionState
    {
        get => _interactionState;
        set => this.RaiseAndSetIfChanged(ref _interactionState, value);
    }

    /// <summary>
    /// Add a series using DynamicData
    /// </summary>
    public void AddSeries(SeriesBase series)
    {
        _seriesSource.Add(series);
        // Auto-fit and legend sync happen automatically via reactive streams
    }

    /// <summary>
    /// Add multiple series efficiently in batch
    /// </summary>
    public void AddSeries(IEnumerable<SeriesBase> series)
    {
        _seriesSource.AddRange(series);
    }

    /// <summary>
    /// Remove a series using DynamicData
    /// </summary>
    public void RemoveSeries(SeriesBase series)
    {
        _seriesSource.Remove(series);
    }

    /// <summary>
    /// Clear all series using DynamicData
    /// </summary>
    public void ClearSeries()
    {
        _seriesSource.Clear();
        XAxis.DataRange = new FRange(0, 1);
        YAxis.DataRange = new FRange(0, 1);
        if (YAxisSecondary != null)
        {
            YAxisSecondary.DataRange = new FRange(0, 1);
        }
        Viewport.SetVisible(XAxis.DataRange, YAxis.DataRange);
    }

    /// <summary>
    /// Add a behavior using DynamicData
    /// </summary>
    public void AddBehavior(IBehavior behavior)
    {
        _behaviorsSource.Add(behavior);
    }

    /// <summary>
    /// Remove a behavior using DynamicData
    /// </summary>
    public void RemoveBehavior(IBehavior behavior)
    {
        _behaviorsSource.Remove(behavior);
    }

    private void UpdateTotalDataPoints()
    {
        var total = Series.Where(s => s.IsVisible && !s.IsEmpty)
                          .Sum(s => s switch
                          {
                              LineSeries ls => ls.Data.Count,
                              ScatterSeries sc => sc.Data.Count,
                              BarSeries bar => bar.Data.Count,
                              _ => 0
                          });
        TotalDataPoints = total;
    }

    public void EnsureSecondaryYAxis()
    {
        if (YAxisSecondary != null)
        {
            return;
        }
        YAxisSecondary = new NumericAxis();
        // Note: For simplicity, we're not updating the _axesView here in this implementation
        // This could be enhanced to maintain the axes collection properly
    }

    public void ReplaceXAxis(IAxis<double> newAxis)
    {
        if (newAxis == null)
        {
            return;
        }
        var old = XAxis;
        newAxis.DataRange = old.DataRange;
        XAxis = newAxis;
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
    }

    public void ReplaceSecondaryYAxis(IAxis<double> newAxis)
    {
        EnsureSecondaryYAxis();
        if (newAxis == null || YAxisSecondary == null)
        {
            return;
        }
        newAxis.DataRange = YAxisSecondary.DataRange;
        YAxisSecondary = newAxis;
    }

    public void UpdateScales(double widthPx, double heightPx)
    {
        XAxis.VisibleRange = Viewport.X;
        YAxis.VisibleRange = Viewport.Y;
        ((AxisBase)XAxis).UpdateScale(0, widthPx);
        ((AxisBase)YAxis).UpdateScale(heightPx, 0);
        if (YAxisSecondary != null)
        {
            YAxisSecondary.VisibleRange = YAxis.VisibleRange;
            ((AxisBase)YAxisSecondary).UpdateScale(heightPx, 0);
        }
    }

    public void AutoFitDataRange()
    {
        var xs = new List<FRange>();
        var ysPrimary = new List<FRange>();
        var ysSecondary = new List<FRange>();
        var anySecondary = false;
        foreach (var s in Series)
        {
            if (s.IsEmpty || !s.IsVisible)
            {
                continue;
            }
            FRange xr;
            FRange yr;
            switch (s)
            {
                case LineSeries ls:
                {
                    xr = ls.GetXRange();
                    yr = ls.GetYRange();
                    break;
                }
                case ScatterSeries sc:
                {
                    xr = sc.GetXRange();
                    yr = sc.GetYRange();
                    break;
                }
                case BandSeries band:
                {
                    var minX = band.Data.Min(p => p.X);
                    var maxX = band.Data.Max(p => p.X);
                    var minY = band.Data.Min(p => p.YLow);
                    var maxY = band.Data.Max(p => p.YHigh);
                    xr = new FRange(minX, maxX);
                    yr = new FRange(minY, maxY);
                    break;
                }
                case BarSeries bar:
                {
                    xr = bar.GetXRange();
                    yr = bar.GetYRange();
                    break;
                }
                case StackedBarSeries sbar:
                {
                    xr = sbar.GetXRange();
                    yr = sbar.GetYRange();
                    break;
                }
                case OhlcSeries ohlc:
                {
                    xr = ohlc.GetXRange();
                    yr = ohlc.GetYRange();
                    break;
                }
                case ErrorBarSeries err:
                {
                    xr = err.GetXRange();
                    yr = err.GetYRange();
                    break;
                }
                default:
                {
                    continue;
                }
            }
            xs.Add(xr);
            if (s.YAxisIndex == 1)
            {
                anySecondary = true;
                ysSecondary.Add(yr);
            }
            else
            {
                ysPrimary.Add(yr);
            }
        }
        if (xs.Count == 0)
        {
            return;
        }
        var xMin = xs.Min(r => r.Min);
        var xMax = xs.Max(r => r.Max);
        if (DoubleUtils.AreEqual(xMin, xMax))
        {
            xMin -= 0.5;
            xMax += 0.5;
        }
        XAxis.DataRange = new FRange(xMin, xMax);
        if (ysPrimary.Count > 0)
        {
            var yMin = ysPrimary.Min(r => r.Min);
            var yMax = ysPrimary.Max(r => r.Max);
            if (DoubleUtils.AreEqual(yMin, yMax))
            {
                yMin -= 0.5;
                yMax += 0.5;
            }
            YAxis.DataRange = new FRange(yMin, yMax);
        }
        if (anySecondary)
        {
            EnsureSecondaryYAxis();
            if (ysSecondary.Count > 0)
            {
                var y2Min = ysSecondary.Min(r => r.Min);
                var y2Max = ysSecondary.Max(r => r.Max);
                if (DoubleUtils.AreEqual(y2Min, y2Max))
                {
                    y2Min -= 0.5;
                    y2Max += 0.5;
                }
                YAxisSecondary!.DataRange = new FRange(y2Min, y2Max);
            }
        }
        Viewport.SetVisible(XAxis.DataRange, YAxis.DataRange);
    }

    public void ZoomAt(double factorX, double factorY, double centerDataX, double centerDataY)
    {
        var xr = XAxis.VisibleRange;
        var yr = YAxis.VisibleRange;
        var newSizeX = xr.Size * factorX;
        var newSizeY = yr.Size * factorY;
        var newMinX = centerDataX - (centerDataX - xr.Min) * factorX;
        var newMaxX = newMinX + newSizeX;
        var newMinY = centerDataY - (centerDataY - yr.Min) * factorY;
        var newMaxY = newMinY + newSizeY;
        if (DoubleUtils.AreEqual(newMinX, newMaxX))
        {
            newMinX -= 1e-6;
            newMaxX += 1e-6;
        }
        if (DoubleUtils.AreEqual(newMinY, newMaxY))
        {
            newMinY -= 1e-6;
            newMaxY += 1e-6;
        }
        XAxis.VisibleRange = new FRange(newMinX, newMaxX);
        YAxis.VisibleRange = new FRange(newMinY, newMaxY);
        if (YAxisSecondary != null)
        {
            YAxisSecondary.VisibleRange = YAxis.VisibleRange;
        }
    }

    public void Pan(double deltaDataX, double deltaDataY)
    {
        var xr = XAxis.VisibleRange;
        var yr = YAxis.VisibleRange;
        XAxis.VisibleRange = new FRange(xr.Min + deltaDataX, xr.Max + deltaDataX);
        YAxis.VisibleRange = new FRange(yr.Min + deltaDataY, yr.Max + deltaDataY);
        if (YAxisSecondary != null)
        {
            YAxisSecondary.VisibleRange = YAxis.VisibleRange;
        }
    }

    /// <summary>
    /// Enhanced disposal with DynamicData cleanup
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // Dispose all reactive subscriptions
        _disposables.Dispose();
        
        // Dispose DynamicData sources
        _seriesSource.Dispose();
        _behaviorsSource.Dispose();

        _disposed = true;
    }
}