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
using System.Reactive.Concurrency; // ajout
using System.Windows.Input;
using System.Windows; // Dispatcher

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

        // UI scheduler (WPF dispatcher)
        var uiScheduler = DispatcherScheduler.Current;

        // CanExecute observables
        var canToggleTheme = this.WhenAnyValue(_ => _.SelectedTheme).Select(_ => true).ObserveOn(uiScheduler);
        var canAddSeries = this.WhenAnyValue(_ => _.SelectedChart).Select(c => c != null).ObserveOn(uiScheduler);
        var canReset = this.WhenAnyValue(_ => _.SelectedChart).Select(c => c != null).ObserveOn(uiScheduler);

        ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme, canToggleTheme, uiScheduler);
        AddRandomSeriesCommand = ReactiveCommand.Create(AddRandomSeries, canAddSeries, uiScheduler);
        ResetViewCommand = ReactiveCommand.Create(ResetView, canReset, uiScheduler);

        InitializeCharts();
        SelectedChart = Charts.FirstOrDefault();
        SetupReactiveBindings(uiScheduler);
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

    private static void RunOnUi(Action action)
    {
        var disp = Application.Current?.Dispatcher;
        if (disp == null || disp.CheckAccess())
        {
            action();
        }
        else
        {
            disp.Invoke(action);
        }
    }

    private void SetupReactiveBindings(IScheduler uiScheduler)
    {
        this.WhenAnyValue(x => x.SelectedTheme)
            .Where(theme => !string.IsNullOrEmpty(theme))
            .ObserveOn(uiScheduler)
            .Subscribe(ApplyThemeToAllCharts);

        this.WhenAnyValue(x => x.AllowInteraction)
            .ObserveOn(uiScheduler)
            .Subscribe(_ => { });

        this.WhenAnyValue(x => x.SelectedChart)
            .Where(chart => chart != null)
            .ObserveOn(uiScheduler)
            .Subscribe(chart => System.Diagnostics.Debug.WriteLine($"Selected chart changed to: {chart!.Title}"));

        _animationSubscription = Observable.Interval(TimeSpan.FromMilliseconds(100))
            .ObserveOn(uiScheduler)
            .Subscribe(_ => UpdateAnimation());
    }

    private void InitializeCharts()
    {
        RunOnUi(() =>
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
            Charts.Add(BuildLogYAxisGrowth());      //14 - New logarithmic chart
            Charts.Add(BuildLogLogScatter());       //15 - New log-log scatter chart
            Charts.Add(BuildDualYAxisLogSecondary());//16 - New dual Y secondary log chart
        });
    }

    private void ToggleTheme()
    {
        SelectedTheme = SelectedTheme == "Light" ? "Dark" : "Light";
    }

    private void AddRandomSeries()
    {
        if (SelectedChart == null)
        {
            return;
        }
        var random = new Random();
        var points = Enumerable.Range(0, 50).Select(i => new PointD(i, random.NextDouble() * 100)).ToArray();
        RunOnUi(() =>
        {
            SelectedChart.AddSeries(new LineSeries(points)
            {
                Title = "Random " + (SelectedChart.Series.Count + 1),
                StrokeThickness = 2.0
            });
        });
    }

    private void ResetView()
    {
        RunOnUi(() => SelectedChart?.AutoFitDataRange());
    }

    private void ApplyThemeToAllCharts(string themeName)
    {
        RunOnUi(() =>
        {
            foreach (var c in Charts)
            {
                c.Theme = themeName == "Dark" ? new DarkTheme() : new LightTheme();
            }
        });
    }

    private void UpdateAnimation()
    {
        AnimationProgress = (AnimationProgress + 0.02) % 1.0;
        if (Charts.Count > 13)
        {
            UpdateRealtimeChart();
        }
    }
    private void UpdateRealtimeChart()
    {
        if (Charts.Count <= 13)
        {
            return;
        }
        // realtime update placeholder
    }

    // Chart builders (same as before but with reactive theme applied)
    private static ChartModel CreateBase(DateTime start, DateTime end, string title)
    {
        var m = new ChartModel { Theme = new LightTheme(), Title = title };
        var dtAxis = new DateTimeAxis();
        dtAxis.SetVisibleRange(start, end);
        m.ReplaceXAxis(dtAxis);
        return m;
    }
    private static ChartModel CreateBaseNumeric(string title)
    {
        return new ChartModel { Theme = new LightTheme(), Title = title };
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

    // Nouveaux builders logarithmiques
    private static ChartModel BuildLogYAxisGrowth()
    {
        var start = DateTime.Today.AddDays(-7);
        var end = DateTime.Today.AddDays(0.5);
        var m = CreateBase(start, end, "Croissance (Y Log)");

        var logY = new LogarithmicAxis { LogBase = 10.0 };
        logY.SetVisibleRange(1, 10000);
        m.ReplaceYAxis(logY);

        var n = 120;
        var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.2)).ToArray();
        var pts = xs.Select((x, i) => new PointD(x.ToOADate(), Math.Pow(10, i / 30.0))).ToArray();
        m.AddSeries(new LineSeries(pts) { Title = "Exp", StrokeThickness = 1.6 });

        m.UpdateScales(800, 400);
        return m;
    }

    private static ChartModel BuildLogLogScatter()
    {
        var m = new ChartModel { Theme = new LightTheme(), Title = "Scatter Log-Log" };

        var logX = new LogarithmicAxis { LogBase = 10.0 };
        logX.SetVisibleRange(1, 1000);
        var logY = new LogarithmicAxis { LogBase = 10.0 };
        logY.SetVisibleRange(1, 100000);
        m.ReplaceXAxis(logX);
        m.ReplaceYAxis(logY);

        var xs = Enumerable.Range(0, 60).Select(i => 1.0 + i * (999.0 / 59.0)).ToArray();
        var pts = xs.Select(x => new PointD(x, Math.Pow(x, 2.5))).ToArray();
        m.AddSeries(new ScatterSeries(pts) { Title = "x^2.5", MarkerSize = 4 });

        m.UpdateScales(800, 400);
        return m;
    }

    private static ChartModel BuildDualYAxisLogSecondary()
    {
        var start = DateTime.Today.AddDays(-5);
        var end = DateTime.Today.AddDays(0.2);
        var m = CreateBase(start, end, "Dual Y (secondaire log)");

        var n = 80;
        var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.5)).ToArray();
        var linear = xs.Select((x, i) => new PointD(x.ToOADate(), 50 + Math.Sin(i * 0.2) * 10)).ToArray();
        m.AddSeries(new LineSeries(linear) { Title = "Lin", StrokeThickness = 1.4, YAxisIndex = 0 });

        m.EnsureSecondaryYAxis();
        var logAxis = new LogarithmicAxis { LogBase = 10.0 };
        logAxis.SetVisibleRange(1, 10000);
        m.ReplaceSecondaryYAxis(logAxis);

        var exp = xs.Select((x, i) => new PointD(x.ToOADate(), Math.Pow(10, i / 20.0))).ToArray();
        m.AddSeries(new LineSeries(exp) { Title = "Exp", StrokeThickness = 1.6, YAxisIndex = 1 });

        m.UpdateScales(800, 400);
        return m;
    }

    public void Dispose()
    {
        _animationSubscription?.Dispose();
        foreach (var chart in Charts) { try { chart.Dispose(); } catch { } }
        Charts.Clear();
    }
}
