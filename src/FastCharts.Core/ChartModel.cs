using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Helpers;
using FastCharts.Core.Interactivity;
using FastCharts.Core.Interaction;
using FastCharts.Core.Legend;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Utilities;

namespace FastCharts.Core;

/// <summary>
/// Central mutable chart state (axes, series, viewport, legend, behaviors, interaction state).
/// Roadmap: extract sub-services (range calc, interaction, legend sync) for cleaner architecture.
/// </summary>
public sealed class ChartModel : ReactiveObject, IChartModel, IDisposable
{
    private readonly List<AxisBase> _axesList;
    private readonly ReadOnlyCollection<AxisBase> _axesView;

    private ITheme _theme = new LightTheme();
    private IAxis<double> _xAxis;
    private IAxis<double> _yAxis;
    private IAxis<double>? _yAxisSecondary;
    private Margins _plotMargins = new Margins(48, 16, 16, 36);
    private InteractionState? _interactionState;
    private string _title = "Chart";
    private bool _disposed;

    public ChartModel()
    {
        _xAxis = new NumericAxis();
        _yAxis = new NumericAxis();
        Viewport = new Interactivity.Viewport(new FRange(0, 1), new FRange(0, 1));
        Series = new ObservableCollection<SeriesBase>();
        _axesList = new List<AxisBase> { (AxisBase)_xAxis, (AxisBase)_yAxis };
        _axesView = new ReadOnlyCollection<AxisBase>(_axesList);
        Legend = new LegendModel();
        Series.CollectionChanged += OnSeriesCollectionChanged;
    }

    public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }
    public ITheme Theme { get => _theme; set => this.RaiseAndSetIfChanged(ref _theme, value); }
    public IAxis<double> XAxis { get => _xAxis; private set => this.RaiseAndSetIfChanged(ref _xAxis, value); }
    public IAxis<double> YAxis { get => _yAxis; private set => this.RaiseAndSetIfChanged(ref _yAxis, value); }
    public IAxis<double>? YAxisSecondary { get => _yAxisSecondary; private set => this.RaiseAndSetIfChanged(ref _yAxisSecondary, value); }
    public Margins PlotMargins { get => _plotMargins; set => this.RaiseAndSetIfChanged(ref _plotMargins, value); }
    public InteractionState? InteractionState { get => _interactionState; set => this.RaiseAndSetIfChanged(ref _interactionState, value); }

    public IViewport Viewport { get; }
    public ObservableCollection<SeriesBase> Series { get; }
    public IReadOnlyList<AxisBase> Axes => _axesView;
    IReadOnlyList<AxisBase> IChartModel.Axes => Axes;
    IReadOnlyList<SeriesBase> IChartModel.Series => Series;
    public LegendModel Legend { get; }
    public IList<IBehavior> Behaviors { get; } = new List<IBehavior>();

    private void OnSeriesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_disposed)
        {
            return;
        }
        Legend.SyncFromSeries(Series);
    }

    public void EnsureSecondaryYAxis()
    {
        if (YAxisSecondary != null)
        {
            return;
        }
        YAxisSecondary = new NumericAxis();
        _axesList.Add((AxisBase)YAxisSecondary);
    }

    public void ReplaceXAxis(IAxis<double> newAxis)
    {
        if (newAxis == null)
        {
            return;
        }
        newAxis.DataRange = XAxis.DataRange;
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
        newAxis.DataRange = YAxis.DataRange;
        YAxis = newAxis;
        if (_axesList.Count > 1)
        {
            _axesList[1] = (AxisBase)newAxis;
        }
    }

    public void ReplaceSecondaryYAxis(IAxis<double> newAxis)
    {
        EnsureSecondaryYAxis();
        if (newAxis == null)
        {
            return;
        }
        newAxis.DataRange = YAxisSecondary!.DataRange;
        YAxisSecondary = newAxis;
        _axesList[2] = (AxisBase)newAxis;
    }

    public void AddSeries(SeriesBase series)
    {
        Series.Add(series);
        AutoFitDataRange();
    }

    public void ClearSeries()
    {
        Series.Clear();
        XAxis.DataRange = new FRange(0, 1);
        YAxis.DataRange = new FRange(0, 1);
        if (YAxisSecondary != null)
        {
            YAxisSecondary.DataRange = new FRange(0, 1);
        }
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
            YAxisSecondary.VisibleRange = YAxis.VisibleRange;
            ((AxisBase)YAxisSecondary).UpdateScale(heightPx, 0);
        }
    }

    public void AutoFitDataRange()
    {
        if (Series.Count == 0)
        {
            return;
        }
        var anyVisible = false;
        foreach (var s in Series)
        {
            if (s != null && s.IsVisible && !s.IsEmpty)
            {
                anyVisible = true;
                break;
            }
        }
        if (!anyVisible)
        {
            return;
        }
        var agg = DataRangeAggregator.Aggregate(Series);
        if (!agg.HasX)
        {
            return;
        }
        var xMin = agg.XMin; var xMax = agg.XMax;
        if (DoubleUtils.AreEqual(xMin, xMax))
        {
            xMin -= 0.5;
            xMax += 0.5;
        }
        XAxis.DataRange = new FRange(xMin, xMax);
        if (agg.HasPrimary)
        {
            var yMin = agg.PrimaryYMin; var yMax = agg.PrimaryYMax;
            if (DoubleUtils.AreEqual(yMin, yMax))
            {
                yMin -= 0.5;
                yMax += 0.5;
            }
            YAxis.DataRange = new FRange(yMin, yMax);
        }
        if (agg.HasSecondary)
        {
            EnsureSecondaryYAxis();
            var y2Min = agg.SecondaryYMin; var y2Max = agg.SecondaryYMax;
            if (DoubleUtils.AreEqual(y2Min, y2Max))
            {
                y2Min -= 0.5;
                y2Max += 0.5;
            }
            YAxisSecondary!.DataRange = new FRange(y2Min, y2Max);
        }
        Viewport.SetVisible(XAxis.DataRange, YAxis.DataRange);
    }

    public void ZoomAt(double factorX, double factorY, double centerDataX, double centerDataY)
    {
        var invalidFactorX = double.IsNaN(factorX) || double.IsInfinity(factorX) || factorX <= 0d;
        if (invalidFactorX)
        {
            throw new ArgumentOutOfRangeException(nameof(factorX), factorX, "Zoom factor X must be finite and positive");
        }
        var invalidFactorY = double.IsNaN(factorY) || double.IsInfinity(factorY) || factorY <= 0d;
        if (invalidFactorY)
        {
            throw new ArgumentOutOfRangeException(nameof(factorY), factorY, "Zoom factor Y must be finite and positive");
        }
        var invalidCenterX = double.IsNaN(centerDataX) || double.IsInfinity(centerDataX);
        if (invalidCenterX)
        {
            throw new ArgumentOutOfRangeException(nameof(centerDataX), centerDataX, "Center X must be finite");
        }
        var invalidCenterY = double.IsNaN(centerDataY) || double.IsInfinity(centerDataY);
        if (invalidCenterY)
        {
            throw new ArgumentOutOfRangeException(nameof(centerDataY), centerDataY, "Center Y must be finite");
        }

        var xr = XAxis.VisibleRange; var yr = YAxis.VisibleRange;
        var newSizeX = xr.Size * factorX; var newSizeY = yr.Size * factorY;
        var newMinX = centerDataX - (centerDataX - xr.Min) * factorX; var newMaxX = newMinX + newSizeX;
        var newMinY = centerDataY - (centerDataY - yr.Min) * factorY; var newMaxY = newMinY + newSizeY;
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
        if (!ValidationHelper.IsValidRange(newMinX, newMaxX))
        {
            throw new InvalidOperationException($"Resulting X range is invalid: [{newMinX}, {newMaxX}]");
        }
        if (!ValidationHelper.IsValidRange(newMinY, newMaxY))
        {
            throw new InvalidOperationException($"Resulting Y range is invalid: [{newMinY}, {newMaxY}]");
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
        var invalidDeltaX = double.IsNaN(deltaDataX) || double.IsInfinity(deltaDataX);
        if (invalidDeltaX)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaDataX), deltaDataX, "Delta X must be finite");
        }
        var invalidDeltaY = double.IsNaN(deltaDataY) || double.IsInfinity(deltaDataY);
        if (invalidDeltaY)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaDataY), deltaDataY, "Delta Y must be finite");
        }
        var xr = XAxis.VisibleRange; var yr = YAxis.VisibleRange;
        var newMinX = xr.Min + deltaDataX; var newMaxX = xr.Max + deltaDataX;
        var newMinY = yr.Min + deltaDataY; var newMaxY = yr.Max + deltaDataY;
        if (!ValidationHelper.IsValidRange(newMinX, newMaxX))
        {
            throw new InvalidOperationException($"Resulting X range is invalid: [{newMinX}, {newMaxX}]");
        }
        if (!ValidationHelper.IsValidRange(newMinY, newMaxY))
        {
            throw new InvalidOperationException($"Resulting Y range is invalid: [{newMinY}, {newMaxY}]");
        }
        XAxis.VisibleRange = new FRange(newMinX, newMaxX);
        YAxis.VisibleRange = new FRange(newMinY, newMaxY);
        if (YAxisSecondary != null)
        {
            YAxisSecondary.VisibleRange = YAxis.VisibleRange;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        Series.CollectionChanged -= OnSeriesCollectionChanged;
        foreach (var axis in _axesList.OfType<IDisposable>())
        {
            axis.Dispose();
        }
        if (_theme is IDisposable td)
        {
            td.Dispose();
        }
        if (Viewport is IDisposable vp)
        {
            vp.Dispose();
        }
        foreach (var s in Series.OfType<IDisposable>())
        {
            s.Dispose();
        }
        foreach (var b in Behaviors.OfType<IDisposable>())
        {
            b.Dispose();
        }
        Series.Clear();
        Behaviors.Clear();
        _axesList.Clear();
        _disposed = true;
    }
}
