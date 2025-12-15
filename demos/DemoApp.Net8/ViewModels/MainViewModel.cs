using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Annotations;
using FastCharts.Rendering.Skia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;

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
    private readonly SkiaChartRenderer _renderer;

    public MainViewModel()
    {
        Charts = new ObservableCollection<ChartModel>();
        _renderer = new SkiaChartRenderer();

        // Initialize charts
        InitializeCharts();

        // Select first chart by default
        SelectedChart = Charts.FirstOrDefault();

        // Setup reactive commands
        ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);
        AddRandomSeriesCommand = ReactiveCommand.Create(AddRandomSeries);
        ResetViewCommand = ReactiveCommand.Create(ResetView);
        
        // P1-EXPORT-PNG: Add export commands
        ExportSelectedChartCommand = ReactiveCommand.CreateFromTask(ExportSelectedChart, this.WhenAnyValue(x => x.SelectedChart).Select(chart => chart != null));
        ExportAllChartsCommand = ReactiveCommand.CreateFromTask(ExportAllCharts);
        CopySelectedChartToClipboardCommand = ReactiveCommand.CreateFromTask(CopySelectedChartToClipboard, this.WhenAnyValue(x => x.SelectedChart).Select(chart => chart != null));

        // Setup reactive bindings and animations
        SetupReactiveBindings();
    }

    /// <summary>
    /// Collection of all available charts
    /// </summary>
    public ObservableCollection<ChartModel> Charts { get; }

    /// <summary>
    /// Currently selected chart - supports two-way binding
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
    
    // P1-EXPORT-PNG: Export commands
    public ICommand ExportSelectedChartCommand { get; }
    public ICommand ExportAllChartsCommand { get; }
    public ICommand CopySelectedChartToClipboardCommand { get; }

    private void SetupReactiveBindings()
    {
        // React to theme changes - now properly marshalled to UI thread
        this.WhenAnyValue(x => x.SelectedTheme)
            .Where(theme => !string.IsNullOrEmpty(theme))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(ApplyThemeToAllCharts);

        // React to interaction changes - now properly marshalled to UI thread
        this.WhenAnyValue(x => x.AllowInteraction)
            .ObserveOn(RxApp.MainThreadScheduler)
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
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(chart =>
            {
                // Could highlight the selected chart or show additional info
                System.Diagnostics.Debug.WriteLine($"Selected chart changed to: {chart!.Title}");
            });

        // Animation with proper WPF Dispatcher scheduling
        _animationSubscription = Observable.Interval(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxApp.MainThreadScheduler)
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
        Charts.Add(BuildLineAnnotationDemo());  // 6
        Charts.Add(BuildRangeAnnotationDemo()); // 7
        Charts.Add(BuildLogarithmicAxisDemo()); // 8 - P1-AX-LOG
        Charts.Add(BuildLttbPerformanceDemo()); // 9 - P1-RESAMPLE-LTTB
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
        SelectedChart?.AutoFitDataRange();
    }

    // P1-EXPORT-PNG: Export functionality
    private async Task ExportSelectedChart()
    {
        if (SelectedChart == null)
        {
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Title = "Export Chart to PNG",
            Filter = "PNG Images (*.png)|*.png|All files (*.*)|*.*",
            FileName = $"{SelectedChart.Title.Replace(" ", "_")}.png",
            DefaultExt = ".png"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                using var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                await _renderer.ExportPngAsync(SelectedChart, fileStream, 1200, 800, quality: 95);
                
                // Show success message
                System.Windows.MessageBox.Show(
                    $"Chart '{SelectedChart.Title}' exported successfully to:\n{saveFileDialog.FileName}",
                    "Export Successful",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Failed to export chart:\n{ex.Message}",
                    "Export Failed",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private async Task ExportAllCharts()
    {
        using var folderDialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select folder for exporting all charts",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var folderPath = folderDialog.SelectedPath;
            
            try
            {
                var exportTasks = Charts.Select(async (chart, index) =>
                {
                    var fileName = $"{index:00}_{chart.Title?.Replace(" ", "_") ?? "Chart"}.png";
                    var fullPath = Path.Combine(folderPath, fileName);
                    
                    using var fileStream = new FileStream(fullPath, FileMode.Create);
                    await _renderer.ExportPngAsync(chart, fileStream, 1200, 800, quality: 95);
                    
                    return fullPath;
                });

                var exportedFiles = await Task.WhenAll(exportTasks);
                
                System.Windows.MessageBox.Show(
                    $"Successfully exported {exportedFiles.Length} charts to:\n{folderPath}",
                    "Batch Export Successful",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Failed to export charts:\n{ex.Message}",
                    "Batch Export Failed",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private async Task CopySelectedChartToClipboard()
    {
        if (SelectedChart == null)
        {
            return;
        }

        try
        {
            // Render to bitmap
            var bitmap = await _renderer.RenderToBitmapAsync(SelectedChart, 1200, 800, transparentBackground: true);
            
            // Convert SKBitmap to System.Drawing.Bitmap for clipboard
            using var memoryStream = new MemoryStream();
            using var image = SkiaSharp.SKImage.FromPixels(bitmap.PeekPixels());
            using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            data.SaveTo(memoryStream);
            
            memoryStream.Position = 0;
            var drawingBitmap = new System.Drawing.Bitmap(memoryStream);
            
            // Copy to clipboard
            System.Windows.Clipboard.SetImage(System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                drawingBitmap.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()));
            
            bitmap.Dispose();
            drawingBitmap.Dispose();
            
            System.Windows.MessageBox.Show(
                $"Chart '{SelectedChart.Title}' copied to clipboard!",
                "Copy Successful",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to copy chart to clipboard:\n{ex.Message}",
                "Copy Failed",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void ApplyThemeToAllCharts(string themeName)
    {
        ITheme theme = themeName switch
        {
            "Dark" => new DarkTheme(),
            _ => new LightTheme()
        };

        foreach (var chart in Charts)
        {
            chart.Theme = theme;
        }
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
            return; // Chart collection doesn't have enough items
        }

        var realtimeChart = Charts[13];
        if (realtimeChart.Series.FirstOrDefault() is LineSeries)
        {
            // Simulate real-time data update
            // In a real app, you'd update the underlying data and trigger refresh
        }
    }

    // Chart builders
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

    private static ChartModel BuildLineAnnotationDemo()
    {
        var model = new ChartModel 
        { 
            Theme = new LightTheme(),
            Title = "Sales Data with Annotations"
        };

        // Monthly sales data
        var salesData = new[]
        {
            new PointD(0, 150),
            new PointD(1, 200),
            new PointD(2, 250),
            new PointD(3, 300),
            new PointD(4, 350),
            new PointD(5, 400),
            new PointD(6, 450),
            new PointD(7, 500),
            new PointD(8, 550),
            new PointD(9, 600),
                new PointD(10, 650),
            new PointD(11, 700)
        };

        model.AddSeries(new LineSeries(salesData)
        {
            Title = "Monthly Sales",
            StrokeThickness = 2
        });

        // Annotations for targets
        var targetLine = new AnnotationLine(500, AnnotationOrientation.Horizontal, "Target: 500 units")
        {
            Color = new ColorRgba(255, 0, 0, 180), // Red
            LineStyle = LineStyle.Dashed,
            Thickness = 2.0,
            LabelPosition = LabelPosition.Start
        };

        var stretchGoalLine = new AnnotationLine(600, AnnotationOrientation.Horizontal, "Stretch Goal: 600 units")
        {
            Color = new ColorRgba(0, 255, 0, 180), // Green
            LineStyle = LineStyle.Dashed,
            Thickness = 2.0,
            LabelPosition = LabelPosition.Start
        };

        model.AddAnnotation(targetLine);
        model.AddAnnotation(stretchGoalLine);

        model.UpdateScales(800, 400);
        return model;
    }

    /// <summary>
    /// P1-AX-LOG: Build a demo chart showcasing the LogarithmicAxis functionality
    /// </summary>
    private static ChartModel BuildLogarithmicAxisDemo()
    {
        var model = new ChartModel
        {
            Theme = new DarkTheme(),
            Title = "Logarithmic Axis Demo (P1-AX-LOG) - Exponential Growth Data"
        };

        // Exponential growth data (e.g., bacterial growth, population, etc.)
        var exponentialData = new List<PointD>();
        var compoundData = new List<PointD>();
        var powerLawData = new List<PointD>();
        
        for (var i = 0; i <= 20; i++)
        {
            var x = i;
            
            // Exponential growth: y = e^(x/4)
            var exponentialY = Math.Exp(x / 4.0);
            exponentialData.Add(new PointD(x, exponentialY));
            
            // Compound interest: y = (1.15)^x
            var compoundY = Math.Pow(1.15, x);
            compoundData.Add(new PointD(x, compoundY));
            
            // Power law: y = x^3 + 1 (avoid zero for log scale)
            var powerY = Math.Pow(x + 1, 3);
            powerLawData.Add(new PointD(x, powerY));
        }

        // Add series with different exponential behaviors
        model.AddSeries(new LineSeries(exponentialData)
        {
            Title = "Exponential Growth (e^(x/4))",
            StrokeThickness = 3
        });

        model.AddSeries(new LineSeries(compoundData)
        {
            Title = "Compound Interest (1.15^x)",
            StrokeThickness = 3
        });

        model.AddSeries(new LineSeries(powerLawData)
        {
            Title = "Power Law (x^3)",
            StrokeThickness = 3
        });

        // Convert Y axis to logarithmic (base 10)
        model.SetYAxisLogarithmic(10.0);

        // Add annotations to show powers of 10
        var powerOfTenLine1 = new AnnotationLine(10, AnnotationOrientation.Horizontal, "10^1 = 10")
        {
            Color = new ColorRgba(255, 255, 0, 180),
            LineStyle = LineStyle.Dashed,
            Thickness = 1.5,
            LabelPosition = LabelPosition.Start
        };

        var powerOfTenLine2 = new AnnotationLine(100, AnnotationOrientation.Horizontal, "10^2 = 100")
        {
            Color = new ColorRgba(255, 255, 0, 180),
            LineStyle = LineStyle.Dashed,
            Thickness = 1.5,
            LabelPosition = LabelPosition.Start
        };

        var powerOfTenLine3 = new AnnotationLine(1000, AnnotationOrientation.Horizontal, "10^3 = 1,000")
        {
            Color = new ColorRgba(255, 255, 0, 180),
            LineStyle = LineStyle.Dashed,
            Thickness = 1.5,
            LabelPosition = LabelPosition.Start
        };

        model.AddAnnotation(powerOfTenLine1);
        model.AddAnnotation(powerOfTenLine2);
        model.AddAnnotation(powerOfTenLine3);

        // Set appropriate ranges
        model.XAxis.SetVisibleRange(0, 20);
        model.YAxis.SetVisibleRange(1, 10000); // From 10^0 to 10^4

        model.UpdateScales(800, 400);
        return model;
    }

    private static ChartModel BuildRangeAnnotationDemo()
    {
        var model = new ChartModel 
        { 
            Theme = new LightTheme(),
            Title = "Temperature Monitoring with Range Annotations (P1-ANN-RANGE)"
        };

        // Temperature data over 24 hours (simulated)
        var temperatureData = new[]
        {
            new PointD(0, 18.5),    // Midnight: 18.5°C
            new PointD(2, 16.8),    // 2 AM: 16.8°C
            new PointD(4, 15.2),    // 4 AM: 15.2°C
            new PointD(6, 14.5),    // 6 AM: 14.5°C (coldest)
            new PointD(8, 17.3),    // 8 AM: 17.3°C
            new PointD(10, 22.1),   // 10 AM: 22.1°C
            new PointD(12, 26.8),   // Noon: 26.8°C
            new PointD(14, 29.5),   // 2 PM: 29.5°C (hottest)
            new PointD(16, 28.2),   // 4 PM: 28.2°C
            new PointD(18, 25.4),   // 6 PM: 25.4°C
            new PointD(20, 22.7),   // 8 PM: 22.7°C
            new PointD(22, 20.1),   // 10 PM: 20.1°C
            new PointD(24, 18.9)    // Midnight: 18.9°C
        };

        model.AddSeries(new LineSeries(temperatureData)
        {
            Title = "Temperature (°C)",
            StrokeThickness = 3
        });

        // Add horizontal range annotations for temperature zones
            
        // Comfort zone (20°C - 25°C)
        var comfortZone = new AnnotationRange(20.0, 25.0, AnnotationOrientation.Horizontal, "Comfort Zone (20-25°C)")
        {
            FillColor = new ColorRgba(0, 255, 0, 40),     // Light green
            BorderColor = new ColorRgba(0, 200, 0, 120),  // Green border
            BorderThickness = 1.5,
            LabelPosition = LabelPosition.Start
        };

        // Warning zone (25°C - 30°C)
        var warningZone = new AnnotationRange(25.0, 30.0, AnnotationOrientation.Horizontal, "Warning Zone (25-30°C)")
        {
            FillColor = new ColorRgba(255, 165, 0, 50),   // Light orange
            BorderColor = new ColorRgba(255, 140, 0, 150), // Orange border
            BorderThickness = 1.5,
            LabelPosition = LabelPosition.Start
        };

        // Cold zone (below 15°C)
        var coldZone = new AnnotationRange(10.0, 15.0, AnnotationOrientation.Horizontal, "Cold Zone (<15°C)")
        {
            FillColor = new ColorRgba(0, 100, 255, 40),      // Light blue
            BorderColor = new ColorRgba(0, 80, 200, 120),    // Blue border
            BorderThickness = 1.5,
            LabelPosition = LabelPosition.Start
        };

        // Add vertical range annotations for time periods

        // Night time (0-6 hours and 22-24 hours)
        var nightTime1 = new AnnotationRange(0.0, 6.0, AnnotationOrientation.Vertical, "Night")
        {
            FillColor = new ColorRgba(25, 25, 112, 30),    // Dark blue, very transparent
            BorderColor = new ColorRgba(25, 25, 112, 80),
            BorderThickness = 1.0,
            LabelPosition = LabelPosition.End
        };

        var nightTime2 = new AnnotationRange(22.0, 24.0, AnnotationOrientation.Vertical, "Night")
        {
            FillColor = new ColorRgba(25, 25, 112, 30),    // Dark blue, very transparent
            BorderColor = new ColorRgba(25, 25, 112, 80),
            BorderThickness = 1.0,
            LabelPosition = LabelPosition.End
        };

        // Peak heat time (12-16 hours)
        var peakHeat = new AnnotationRange(12.0, 16.0, AnnotationOrientation.Vertical, "Peak Heat Period")
        {
            FillColor = new ColorRgba(255, 69, 0, 25),       // Red-orange, very transparent
            BorderColor = new ColorRgba(255, 69, 0, 100),
            BorderThickness = 1.0,
            LabelPosition = LabelPosition.Start
        };

        // Add all annotations with proper Z-ordering
        model.AddAnnotation(nightTime1);      // Background ranges first
        model.AddAnnotation(nightTime2);
        model.AddAnnotation(peakHeat);
        model.AddAnnotation(coldZone);        // Temperature zones on top
        model.AddAnnotation(comfortZone);
        model.AddAnnotation(warningZone);

        model.UpdateScales(800, 400);
        return model;
    }

    private static ChartModel BuildLttbPerformanceDemo()
    {
        var model = new ChartModel
        {
            Theme = new LightTheme(),
            Title = "LTTB Performance Demo (P1-RESAMPLE-LTTB) - 100K Points ? Optimized Rendering"
        };

        // Generate large dataset - 100K points!
        var hugeDataset = new List<PointD>(100_000);
        var random = new Random(42); // Fixed seed for consistent results
        
        // Create complex signal with multiple frequency components
        for (var i = 0; i < 100_000; i++)
        {
            var x = i * 0.01; // 0 to 1000
            
            // Multi-frequency signal with noise (realistic sensor data)
            var signal = 
                50 +                                    // DC offset
                30 * Math.Sin(x * 0.1) +               // Low frequency trend
                15 * Math.Sin(x * 0.5) +               // Medium frequency
                8 * Math.Sin(x * 2.0) +                // High frequency
                5 * Math.Sin(x * 8.0) +                // Very high frequency
                (random.NextDouble() - 0.5) * 4;       // Noise
            
            hugeDataset.Add(new PointD(x, signal));
        }

        // Create high-performance LineSeries with LTTB enabled
        var massiveSeries = LineSeries.CreateHighPerformance(
            hugeDataset, 
            "Massive Dataset (100K points)",
            autoResampleThreshold: 2000 // Start resampling above 2K points
        );
        massiveSeries.StrokeThickness = 1.5;

        model.AddSeries(massiveSeries);

        // Add comparison series - same data but smaller for reference
        var sampledData = hugeDataset.Where((point, index) => index % 500 == 0).ToList(); // Every 500th point
        var referenceSeries = new LineSeries(sampledData)
        {
            Title = "Reference (Manual Sampling - 200 points)",
            StrokeThickness = 2.0
        };
        model.AddSeries(referenceSeries);

        // Add annotations to show resampling effectiveness
        var performanceNote = new AnnotationLine(30.0, AnnotationOrientation.Horizontal, "LTTB: 100K?~1K points, 60+ FPS")
        {
            Color = new ColorRgba(0, 150, 0, 180),
            LineStyle = LineStyle.Dashed,
            Thickness = 1.5,
            LabelPosition = LabelPosition.Start
        };

        var qualityNote = new AnnotationLine(70.0, AnnotationOrientation.Horizontal, "Visual Quality: 95%+ Preserved")
        {
            Color = new ColorRgba(0, 0, 255, 180),
            LineStyle = LineStyle.Dashed,
            Thickness = 1.5,
            LabelPosition = LabelPosition.Start
        };

        model.AddAnnotation(performanceNote);
        model.AddAnnotation(qualityNote);

        // Set appropriate ranges to show the full complexity
        model.XAxis.SetVisibleRange(0, 1000);
        model.YAxis.SetVisibleRange(0, 100);

        model.UpdateScales(800, 400);
        
        // Get resampling stats if available
        var stats = massiveSeries.GetLastResamplingStats();
        if (stats.HasValue)
        {
            System.Diagnostics.Debug.WriteLine($"LTTB Stats: {stats}");
        }

        return model;
    }

    public void Dispose()
    {
        _animationSubscription?.Dispose();

        foreach (var chart in Charts)
        {
            try
            {
                chart.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing chart '{chart.Title}': {ex.Message}");
            }
        }

        Charts.Clear();
    }
}