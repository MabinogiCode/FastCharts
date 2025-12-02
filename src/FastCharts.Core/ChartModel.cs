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
using FastCharts.Core.Services;

namespace FastCharts.Core;

/// <summary>
/// Central mutable chart state (axes, series, viewport, legend, behaviors, interaction state).
/// Uses extracted services for cleaner separation of concerns.
/// </summary>
public sealed class ChartModel : ReactiveObject, IChartModel, IDisposable
{
    private readonly List<AxisBase> _axesList;
    private readonly ReadOnlyCollection<AxisBase> _axesView;
    private readonly IDataRangeCalculatorService _dataRangeCalculator;
    private readonly IInteractionService _interactionService;
    private readonly ILegendSyncService _legendSyncService;
    private readonly IAxisManagementService _axisManagementService;

    private ITheme _theme = new LightTheme();
    private IAxis<double> _xAxis;
    private IAxis<double> _yAxis;
    private IAxis<double>? _yAxisSecondary;
    private Margins _plotMargins = new Margins(48, 16, 16, 36);
    private InteractionState? _interactionState;
    private string _title = "Chart";
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of ChartModel with default services
    /// </summary>
    public ChartModel() : this(
        new DataRangeCalculatorService(),
        new InteractionService(),
        new LegendSyncService(),
        new AxisManagementService())
    {
    }

    /// <summary>
    /// Initializes a new instance of ChartModel with dependency injection support
    /// </summary>
    /// <param name="dataRangeCalculator">Service for calculating data ranges</param>
    /// <param name="interactionService">Service for handling interactions</param>
    /// <param name="legendSyncService">Service for legend synchronization</param>
    /// <param name="axisManagementService">Service for axis management</param>
    public ChartModel(
        IDataRangeCalculatorService dataRangeCalculator,
        IInteractionService interactionService,
        ILegendSyncService legendSyncService,
        IAxisManagementService axisManagementService)
    {
        _dataRangeCalculator = dataRangeCalculator ?? throw new ArgumentNullException(nameof(dataRangeCalculator));
        _interactionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
        _legendSyncService = legendSyncService ?? throw new ArgumentNullException(nameof(legendSyncService));
        _axisManagementService = axisManagementService ?? throw new ArgumentNullException(nameof(axisManagementService));

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
    
    public InteractionState? InteractionState 
    { 
        get => _interactionState; 
        set 
        { 
            this.RaiseAndSetIfChanged(ref _interactionState, value);
            _interactionService.UpdateInteractionState(value);
        } 
    }

    public IViewport Viewport { get; }
    public ObservableCollection<SeriesBase> Series { get; }
    public IReadOnlyList<AxisBase> Axes => _axesView;
    IReadOnlyList<AxisBase> IChartModel.Axes => Axes;
    IReadOnlyList<SeriesBase> IChartModel.Series => Series;
    public LegendModel Legend { get; }
    public IList<IBehavior> Behaviors { get; } = new List<IBehavior>();
    
    /// <summary>
    /// Collection of chart annotations (P1-ANN-LINE)
    /// </summary>
    public ObservableCollection<IAnnotation> Annotations { get; } = new ObservableCollection<IAnnotation>();

    private void OnSeriesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_disposed)
        {
            return;
        }
        
        _legendSyncService.SyncLegendWithSeries(Legend, Series);
    }

    /// <summary>
    /// Ensures a secondary Y axis exists, creating one if necessary
    /// </summary>
    public void EnsureSecondaryYAxis()
    {
        YAxisSecondary = _axisManagementService.EnsureSecondaryYAxis(YAxisSecondary, _axesList);
    }

    /// <summary>
    /// Replaces the X axis with a new axis, preserving data range
    /// </summary>
    /// <param name="newAxis">New X axis to replace with</param>
    public void ReplaceXAxis(IAxis<double> newAxis)
    {
        XAxis = _axisManagementService.ReplaceXAxis(XAxis, newAxis, _axesList);
    }

    /// <summary>
    /// Replaces the Y axis with a new axis, preserving data range
    /// </summary>
    /// <param name="newAxis">New Y axis to replace with</param>
    public void ReplaceYAxis(IAxis<double> newAxis)
    {
        YAxis = _axisManagementService.ReplaceYAxis(YAxis, newAxis, _axesList);
    }

    /// <summary>
    /// Replaces the secondary Y axis with a new axis, preserving data range
    /// </summary>
    /// <param name="newAxis">New secondary Y axis to replace with</param>
    public void ReplaceSecondaryYAxis(IAxis<double> newAxis)
    {
        EnsureSecondaryYAxis();
        YAxisSecondary = _axisManagementService.ReplaceSecondaryYAxis(YAxisSecondary, newAxis, _axesList);
    }

    /// <summary>
    /// Adds a series to the chart and automatically fits the data range
    /// </summary>
    /// <param name="series">Series to add</param>
    public void AddSeries(SeriesBase series)
    {
        Series.Add(series);
        AutoFitDataRange();
    }

    /// <summary>
    /// Clears all series and resets axis ranges to defaults
    /// </summary>
    public void ClearSeries()
    {
        Series.Clear();
        _axisManagementService.ClearAxisRanges(XAxis, YAxis, YAxisSecondary, Viewport);
    }

    /// <summary>
    /// Updates scales for all axes based on pixel dimensions
    /// </summary>
    /// <param name="widthPx">Chart width in pixels</param>
    /// <param name="heightPx">Chart height in pixels</param>
    public void UpdateScales(double widthPx, double heightPx)
    {
        _axisManagementService.UpdateScales(XAxis, YAxis, YAxisSecondary, Viewport, widthPx, heightPx);
    }

    /// <summary>
    /// Automatically calculates and applies data ranges from all series
    /// </summary>
    public void AutoFitDataRange()
    {
        var result = _dataRangeCalculator.CalculateDataRanges(Series);
        ApplyDataRangeResult(result);
    }

    /// <summary>
    /// Performs zoom operation at specified center point
    /// </summary>
    /// <param name="factorX">Zoom factor for X axis (> 0)</param>
    /// <param name="factorY">Zoom factor for Y axis (> 0)</param>
    /// <param name="centerDataX">Center point X in data coordinates</param>
    /// <param name="centerDataY">Center point Y in data coordinates</param>
    public void ZoomAt(double factorX, double factorY, double centerDataX, double centerDataY)
    {
        _interactionService.ZoomAt(XAxis, YAxis, YAxisSecondary, factorX, factorY, centerDataX, centerDataY);
    }

    /// <summary>
    /// Performs pan operation by specified deltas
    /// </summary>
    /// <param name="deltaDataX">Pan delta for X axis in data coordinates</param>
    /// <param name="deltaDataY">Pan delta for Y axis in data coordinates</param>
    public void Pan(double deltaDataX, double deltaDataY)
    {
        _interactionService.Pan(XAxis, YAxis, YAxisSecondary, deltaDataX, deltaDataY);
    }

    /// <summary>
    /// Adds an annotation to the chart
    /// </summary>
    /// <param name="annotation">Annotation to add</param>
    public void AddAnnotation(IAnnotation annotation)
    {
        if (annotation != null)
        {
            Annotations.Add(annotation);
        }
    }

    /// <summary>
    /// Removes an annotation from the chart
    /// </summary>
    /// <param name="annotation">Annotation to remove</param>
    /// <returns>True if the annotation was found and removed, false otherwise</returns>
    public bool RemoveAnnotation(IAnnotation annotation)
    {
        return annotation != null && Annotations.Remove(annotation);
    }

    /// <summary>
    /// Clears all annotations from the chart
    /// </summary>
    public void ClearAnnotations()
    {
        Annotations.Clear();
    }

    private void ApplyDataRangeResult(DataRangeCalculationResult result)
    {
        if (!result.HasData)
        {
            return;
        }

        if (result.HasX)
        {
            XAxis.DataRange = result.XRange;
        }

        if (result.HasPrimary)
        {
            YAxis.DataRange = result.PrimaryYRange;
        }

        if (result.HasSecondary)
        {
            EnsureSecondaryYAxis();
            YAxisSecondary!.DataRange = result.SecondaryYRange;
        }

        Viewport.SetVisible(XAxis.DataRange, YAxis.DataRange);
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
        
        if (_theme is IDisposable themeDisposable)
        {
            themeDisposable.Dispose();
        }
        
        if (Viewport is IDisposable viewportDisposable)
        {
            viewportDisposable.Dispose();
        }
        
        foreach (var series in Series.OfType<IDisposable>())
        {
            series.Dispose();
        }
        
        foreach (var behavior in Behaviors.OfType<IDisposable>())
        {
            behavior.Dispose();
        }
        
        Series.Clear();
        Behaviors.Clear();
        _axesList.Clear();
        _disposed = true;
    }
}
