using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace DemoApp.Net8.ViewModels;

/// <summary>
/// Reactive ViewModel demonstrating full MVVM with ReactiveUI and FastCharts
/// </summary>
public sealed class MainViewModel : ReactiveObject, IDisposable
{
    private ChartModel? _selectedChart;
    private string _selectedTheme = "Light";
    private bool _allowInteraction = true;
    private double _animationProgress;
    private IDisposable? _animationSubscription;

    public MainViewModel()
    {
        Charts = new ObservableCollection<ChartModel>();

        // Initialize charts
        InitializeCharts();

        // Select first chart by default
        SelectedChart = Charts.FirstOrDefault();

        // Setup reactive commands
        ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);
        AddRandomSeriesCommand = ReactiveCommand.Create(AddRandomSeries);
        ResetViewCommand = ReactiveCommand.Create(ResetView);

        // Setup reactive properties and animations
        SetupReactiveBindings();
    }

    /// <summary>
    /// Collection of all available charts
    /// </summary>
    public ObservableCollection<ChartModel> Charts { get; }

    /// <summary>
    /// Currently selected chart - supports two-way binding
    /// This property allows the user to select which chart to operate on (add series, reset view, etc.)
    /// When the user selects a chart from the ComboBox, this property is updated and
    /// the commands (Add Random Series, Reset View) will operate on this selected chart.
    /// </summary>
    public ChartModel? SelectedChart
    {
        get => _selectedChart;
        set => this.RaiseAndSetIfChanged(ref _selectedChart, value);
    }

    /// <summary>
    /// Selected theme name for reactive theme switching
    /// </summary>
    public string SelectedTheme
    {
        get => _selectedTheme;
        set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
    }

    /// <summary>
    /// Controls whether user can interact with charts
    /// </summary>
    public bool AllowInteraction
    {
        get => _allowInteraction;
        set => this.RaiseAndSetIfChanged(ref _allowInteraction, value);
    }

    /// <summary>
    /// Animation progress for demo purposes (0.0 to 1.0)
    /// </summary>
    public double AnimationProgress
    {
        get => _animationProgress;
        set => this.RaiseAndSetIfChanged(ref _animationProgress, value);
    }

    // Reactive Commands
    public ICommand ToggleThemeCommand { get; }
    public ICommand AddRandomSeriesCommand { get; }
    public ICommand ResetViewCommand { get; }

    private void SetupReactiveBindings()
    {
        // React to theme changes - now properly marshalled to UI thread
        this.WhenAnyValue(x => x.SelectedTheme)
            .Where(theme => !string.IsNullOrEmpty(theme))
            .ObserveOn(RxApp.MainThreadScheduler) // ✅ Now correctly maps to WPF Dispatcher
            .Subscribe(ApplyThemeToAllCharts);

        // React to interaction changes - now properly marshalled to UI thread
        this.WhenAnyValue(x => x.AllowInteraction)
            .ObserveOn(RxApp.MainThreadScheduler) // ✅ Now correctly maps to WPF Dispatcher
            .Subscribe(enabled =>
            {
                // Collection access is now safe - we're guaranteed to be on UI thread
                foreach (var chart in Charts)
                {
                    // Could disable behaviors here if needed
                }
            });

        // React to chart selection changes for better UX
        this.WhenAnyValue(x => x.SelectedChart)
            .Where(chart => chart != null)
            .ObserveOn(RxApp.MainThreadScheduler) // ✅ Now correctly maps to WPF Dispatcher
            .Subscribe(chart =>
            {
                // Could highlight the selected chart or show additional info
                System.Diagnostics.Debug.WriteLine($"Selected chart changed to: {chart!.Title}");
            });

        // ✅ FIXED: Animation with proper WPF Dispatcher scheduling
        _animationSubscription = Observable.Interval(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxApp.MainThreadScheduler) // ✅ Now correctly maps to WPF Dispatcher
            .Subscribe(_ => UpdateAnimation());
    }

    private void InitializeCharts()
    {
        Charts.Add(BuildMixedChart());          // 0
        Charts.Add(BuildBarsChart());           // 1
        Charts.Add(BuildStackedBarsChart());    // 2
        Charts.Add(BuildOhlcChart());           // 3
        Charts.Add(BuildErrorBarChart());       // 4
        Charts.Add(BuildMinimalLineChart());    // 5
        Charts.Add(BuildAreaOnly());            // 6
        Charts.Add(BuildScatterOnly());         // 7
        Charts.Add(BuildStepLine());            // 8
        Charts.Add(BuildSingleBars());          // 9
        Charts.Add(BuildStacked100());          //10
        Charts.Add(BuildOhlcWithErrorOverlay());//11
        Charts.Add(BuildMultiSeriesTooltipShowcase()); //12
        Charts.Add(BuildRealtimeChart());       //13 - New reactive chart
    }

    private void ToggleTheme()
    {
        // ✅ SIMPLIFIED: ReactiveUI commands automatically execute on UI thread
        SelectedTheme = SelectedTheme == "Light" ? "Dark" : "Light";
    }

    private void AddRandomSeries()
    {
        // ✅ SIMPLIFIED: ReactiveUI commands automatically execute on UI thread
        if (SelectedChart == null)
        {
            return;
        }

        var random = new Random();
        var points = Enumerable.Range(0, 50)
            .Select(i => new PointD(i, random.NextDouble() * 100))
            .ToArray();

        var series = new LineSeries(points)
        {
            Title = $"Random Series {SelectedChart.Series.Count + 1}",
            StrokeThickness = 2.0
        };
        SelectedChart.AddSeries(series);
    }

    private void ResetView()
    {
        // ✅ SIMPLIFIED: ReactiveUI commands automatically execute on UI thread
        SelectedChart?.AutoFitDataRange();
    }

    private void ApplyThemeToAllCharts(string themeName)
    {
        // ✅ SIMPLIFIED: Called from ObserveOn(RxApp.MainThreadScheduler), guaranteed UI thread
        ITheme theme = themeName switch
        {
            "Dark" => new DarkTheme(),
            _ => new LightTheme()
        };

        foreach (var chart in Charts)
        {
            chart.Theme = theme; // This triggers reactive notification!
        }
    }

    private void UpdateAnimation()
    {
        // ✅ Already on UI thread thanks to ObserveOn(RxApp.MainThreadScheduler)
        // But let's be extra safe for complex operations
        AnimationProgress = (AnimationProgress + 0.02) % 1.0;

        // ✅ FIXED: Add bounds check here as well for defense in depth
        if (Charts.Count > 13)
        {
            UpdateRealtimeChart();
        }
    }

    private void UpdateRealtimeChart()
    {
        // ✅ SIMPLIFIED: Called from ObserveOn(RxApp.MainThreadScheduler), guaranteed UI thread
        // ✅ FIXED: Bounds checking to prevent IndexOutOfRangeException
        if (Charts.Count <= 13)
        {
            return; // Chart collection doesn't have enough items
        }

        var realtimeChart = Charts[13];
        if (realtimeChart.Series.FirstOrDefault() is LineSeries)
        {
            // Simulate real-time data update
            // In a real app, you'd update the underlying data and trigger refresh
            // This is just for demo purposes showing reactive binding capabilities
        }
    }

    // Chart builders (same as before but with reactive theme applied)
    private static ChartModel CreateBase(DateTime start, DateTime end, string title = "Chart")
    {
        var m = new ChartModel
        {
            Theme = new LightTheme(),
            Title = title
        };

        var dtAxis = new DateTimeAxis();
        dtAxis.SetVisibleRange(start, end);
        m.ReplaceXAxis(dtAxis);
        return m;
    }

    private static ChartModel CreateBaseNumeric(string title = "Chart")
    {
        var m = new ChartModel
        {
            Theme = new LightTheme(),
            Title = title
        };
        return m;
    }

    private static ChartModel BuildMixedChart()
    {
        var start = DateTime.Today.AddDays(-14);
        var end = DateTime.Today.AddDays(1);
        var n = 201;
        var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.5)).ToArray();
        var m = CreateBase(start, end, "Mixed Chart");

        var areaPts = xs.Select(x => new PointD(x.ToOADate(), Math.Max(0, Math.Sin(x.Ticks / 1e10)))).ToArray();
        m.AddSeries(new AreaSeries(areaPts) { Title = "Area", Baseline = 0.0, FillOpacity = 0.35 });

        var linePts = xs.Select(x => new PointD(x.ToOADate(), Math.Cos(x.Ticks / 1e10))).ToArray();
        m.AddSeries(new LineSeries(linePts) { Title = "Line", StrokeThickness = 1.8 });

        var scatterPts = xs.Where((x, i) => i % 16 == 0)
            .Select(x => new PointD(x.ToOADate(), Math.Sin(x.Ticks / 1e10) + (Math.Sin(3 * (x.Ticks / 1e10)) * 0.05))).ToArray();
        m.AddSeries(new ScatterSeries(scatterPts) { Title = "Scatter", MarkerSize = 4.0 });

        m.UpdateScales(800, 400);
        return m;
    }

    private static ChartModel BuildRealtimeChart()
    {
        var m = CreateBaseNumeric("Realtime Data");
        var start = DateTime.Now.AddMinutes(-5);
        var end = DateTime.Now.AddMinutes(1);

        var dtAxis = new DateTimeAxis();
        dtAxis.SetVisibleRange(start, end);
        m.ReplaceXAxis(dtAxis);

        // Create initial data
        var random = new Random();
        var points = Enumerable.Range(0, 100)
            .Select(i => new PointD(
                start.AddSeconds(i * 3).ToOADate(),
                50 + Math.Sin(i * 0.1) * 20 + random.NextDouble() * 10
            ))
            .ToArray();

        m.AddSeries(new LineSeries(points)
        {
            Title = "Realtime Data",
            StrokeThickness = 2.0
        });

        return m;
    }

    // ✅ CORRECTED: Complete chart builders with actual data
    private static ChartModel BuildBarsChart()
    {
        var m = CreateBaseNumeric("Bar Chart");
        var random = new Random();
        var points = Enumerable.Range(0, 10)
            .Select(i => new BarPoint(i, random.Next(10, 100)))
            .ToArray();
        m.AddSeries(new BarSeries(points) { Title = "Bars" });
        return m;
    }

    private static ChartModel BuildStackedBarsChart()
    {
        var m = CreateBaseNumeric("Stacked Bars");
        var points = Enumerable.Range(0, 5)
            .Select(i => new StackedBarPoint(i, new double[] { 10 + i * 5, 15 + i * 3, 8 + i * 2 }))
            .ToArray();
        m.AddSeries(new StackedBarSeries(points) { Title = "Stacked" });
        return m;
    }

    private static ChartModel BuildOhlcChart()
    {
        var m = CreateBaseNumeric("OHLC Chart");
        var random = new Random();
        var points = Enumerable.Range(0, 20)
            .Select(i =>
            {
                var open = 50 + random.NextDouble() * 20;
                var close = open + (random.NextDouble() - 0.5) * 10;
                var high = Math.Max(open, close) + random.NextDouble() * 5;
                var low = Math.Min(open, close) - random.NextDouble() * 5;
                return new OhlcPoint(i, open, high, low, close);
            })
            .ToArray();
        m.AddSeries(new OhlcSeries(points) { Title = "OHLC" });
        return m;
    }

    private static ChartModel BuildErrorBarChart()
    {
        var m = CreateBaseNumeric("Error Bars");
        var points = Enumerable.Range(0, 10)
            .Select(i => new ErrorBarPoint(i, 50 + i * 5, 5))
            .ToArray();
        m.AddSeries(new ErrorBarSeries(points) { Title = "Error Bars" });
        return m;
    }

    private static ChartModel BuildMinimalLineChart()
    {
        var m = CreateBaseNumeric("Line Chart");
        var points = Enumerable.Range(0, 20)
            .Select(i => new PointD(i, Math.Sin(i * 0.3) * 50 + 50))
            .ToArray();
        m.AddSeries(new LineSeries(points) { Title = "Sine Wave" });
        return m;
    }

    private static ChartModel BuildAreaOnly()
    {
        var m = CreateBaseNumeric("Area Chart");
        var points = Enumerable.Range(0, 30)
            .Select(i => new PointD(i, Math.Abs(Math.Cos(i * 0.2) * 40) + 10))
            .ToArray();
        m.AddSeries(new AreaSeries(points) { Title = "Area", FillOpacity = 0.7 });
        return m;
    }

    private static ChartModel BuildScatterOnly()
    {
        var m = CreateBaseNumeric("Scatter Plot");
        var random = new Random();
        var points = Enumerable.Range(0, 50)
            .Select(i => new PointD(random.NextDouble() * 100, random.NextDouble() * 100))
            .ToArray();
        m.AddSeries(new ScatterSeries(points) { Title = "Random Points", MarkerSize = 6 });
        return m;
    }

    private static ChartModel BuildStepLine()
    {
        var m = CreateBaseNumeric("Step Line");
        var points = Enumerable.Range(0, 15)
            .Select(i => new PointD(i, Math.Floor(i / 3.0) * 20 + 10))
            .ToArray();
        m.AddSeries(new StepLineSeries(points) { Title = "Steps" });
        return m;
    }

    private static ChartModel BuildSingleBars()
    {
        var m = CreateBaseNumeric("Single Bars");
        var points = new[] { new BarPoint(0, 45), new BarPoint(1, 67), new BarPoint(2, 23) };
        m.AddSeries(new BarSeries(points) { Title = "Simple Bars" });
        return m;
    }

    private static ChartModel BuildStacked100()
    {
        var m = CreateBaseNumeric("Stacked 100%");
        var points = Enumerable.Range(0, 4)
            .Select(i => new StackedBarPoint(i, new double[] { 25, 35, 40 }))
            .ToArray();
        m.AddSeries(new StackedBarSeries(points) { Title = "100% Stack" });
        return m;
    }

    private static ChartModel BuildOhlcWithErrorOverlay()
    {
        var m = CreateBaseNumeric("OHLC + Error");
        var random = new Random();

        // Add OHLC
        var ohlcPoints = Enumerable.Range(0, 10)
            .Select(i =>
            {
                var open = 50 + random.NextDouble() * 20;
                var close = open + (random.NextDouble() - 0.5) * 10;
                var high = Math.Max(open, close) + random.NextDouble() * 5;
                var low = Math.Min(open, close) - random.NextDouble() * 5;
                return new OhlcPoint(i, open, high, low, close);
            })
            .ToArray();
        m.AddSeries(new OhlcSeries(ohlcPoints) { Title = "OHLC" });

        // Add Error bars
        var errorPoints = Enumerable.Range(0, 10)
            .Select(i => new ErrorBarPoint(i, 70 + i * 2, 3))
            .ToArray();
        m.AddSeries(new ErrorBarSeries(errorPoints) { Title = "Errors" });

        return m;
    }

    private static ChartModel BuildMultiSeriesTooltipShowcase()
    {
        var m = CreateBaseNumeric("Multi-Series");

        // Add multiple line series
        for (var s = 0; s < 3; s++)
        {
            var points = Enumerable.Range(0, 20)
                .Select(i => new PointD(i, Math.Sin(i * 0.3 + s) * 30 + 50 + s * 20))
                .ToArray();
            m.AddSeries(new LineSeries(points) { Title = $"Series {s + 1}" });
        }

        return m;
    }

    public void Dispose()
    {
        // ✅ CRITICAL FIX: Enhanced dispose pattern for ViewModels
        _animationSubscription?.Dispose();

        // ✅ CRITICAL FIX: Dispose all charts with proper error handling
        foreach (var chart in Charts)
        {
            try
            {
                chart.Dispose();
            }
            catch (Exception ex)
            {
                // Log but don't throw in Dispose
                System.Diagnostics.Debug.WriteLine($"Error disposing chart '{chart.Title}': {ex.Message}");
            }
        }

        // ✅ CRITICAL FIX: Clear collection after disposing items
        Charts.Clear();
    }
}
