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
        Charts.Add(BuildAreaOnly());            // 6
        Charts.Add(BuildScatterOnly());         // 7
        Charts.Add(BuildStepLine());            // 8
        Charts.Add(BuildSingleBars());          // 9
        Charts.Add(BuildStacked100());          //10
        Charts.Add(BuildOhlcWithErrorOverlay());//11
        Charts.Add(BuildMultiSeriesTooltipShowcase()); //12
        Charts.Add(BuildRealtimeChart());       //13 - New reactive chart
        
        // P1-AX-CAT: Add CategoryAxis demo
        Charts.Add(BuildCategoryAxisDemo());    //14 - CategoryAxis demo
        
        // P1-ANN-LINE: Add Annotation demo
        Charts.Add(BuildAnnotationDemo());      //15 - Annotation demo
        
        // P1-AX-MULTI: Add Multi-Axis demo
        Charts.Add(BuildMultiAxisDemo());       //16 - Multi-Axis demo
        
        // P1-ANN-RANGE: Add Range Annotation demo
        Charts.Add(BuildRangeAnnotationDemo()); //17 - Range Annotation demo
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

    /// <summary>
    /// P1-AX-CAT: Build a demo chart showcasing the CategoryAxis functionality
    /// </summary>
    private static ChartModel BuildCategoryAxisDemo()
    {
        var model = new ChartModel 
        { 
            Theme = new LightTheme(),
            Title = "Quarterly Sales (CategoryAxis Demo)"
        };

        // Create CategoryAxis with quarterly labels
        var categoryAxis = new CategoryAxis(new[] 
        { 
            "Q1 2023", "Q2 2023", "Q3 2023", "Q4 2023",
            "Q1 2024", "Q2 2024", "Q3 2024", "Q4 2024"
        });
        
        model.ReplaceXAxis(categoryAxis);

        // Sales data for each quarter
        var salesData = new[]
        {
            new BarPoint(0, 150), // Q1 2023
            new BarPoint(1, 180), // Q2 2023  
            new BarPoint(2, 220), // Q3 2023
            new BarPoint(3, 280), // Q4 2023
            new BarPoint(4, 320), // Q1 2024
            new BarPoint(5, 380), // Q2 2024
            new BarPoint(6, 420), // Q3 2024
            new BarPoint(7, 450)  // Q4 2024
        };

        model.AddSeries(new BarSeries(salesData)
        {
            Title = "Sales (K€)"
        });

        // Revenue trend line
        var revenueData = new[]
        {
            new PointD(0, 140),
            new PointD(1, 175),
            new PointD(2, 210),
            new PointD(3, 270),
            new PointD(4, 310),
            new PointD(5, 375),
            new PointD(6, 415),
            new PointD(7, 445)
        };

        model.AddSeries(new LineSeries(revenueData)
        {
            Title = "Revenue Trend",
            StrokeThickness = 3
        });

        model.UpdateScales(800, 400);
        return model;
    }

    /// <summary>
    /// Demonstrates annotation functionality (P1-ANN-LINE)
    /// Shows horizontal and vertical lines with different styles and labels
    /// </summary>
    private static ChartModel BuildAnnotationDemo()
    {
        var model = new ChartModel 
        { 
            Theme = new DarkTheme(),
            Title = "Stock Price with Annotations (P1-ANN-LINE)"
        };

        // Stock price data (simulated)
        var stockData = new[]
        {
            new PointD(0, 100),   // Day 0
            new PointD(1, 102),   // Day 1
            new PointD(2, 98),    // Day 2
            new PointD(3, 105),   // Day 3
            new PointD(4, 108),   // Day 4
            new PointD(5, 103),   // Day 5
            new PointD(6, 110),   // Day 6
            new PointD(7, 115),   // Day 7
            new PointD(8, 112),   // Day 8
            new PointD(9, 118),   // Day 9
            new PointD(10, 120)   // Day 10
        };

        model.AddSeries(new LineSeries(stockData)
        {
            Title = "Stock Price ($)",
            StrokeThickness = 2
        });

        // Add horizontal annotations for key levels
        var supportLevel = new AnnotationLine(100.0, AnnotationOrientation.Horizontal, "Support Level ($100)")
        {
            Color = new ColorRgba(0, 255, 0, 180), // Green
            LineStyle = LineStyle.Dashed,
            Thickness = 2.0,
            LabelPosition = LabelPosition.Start
        };

        var resistanceLevel = new AnnotationLine(115.0, AnnotationOrientation.Horizontal, "Resistance Level ($115)")
        {
            Color = new ColorRgba(255, 0, 0, 180), // Red
            LineStyle = LineStyle.Dashed,
            Thickness = 2.0,
            LabelPosition = LabelPosition.Start
        };

        // Add vertical annotations for important events
        var earnings = new AnnotationLine(3.0, AnnotationOrientation.Vertical, "Earnings Report")
        {
            Color = new ColorRgba(128, 0, 128, 160), // Purple
            LineStyle = LineStyle.Dotted,
            Thickness = 2.0,
            LabelPosition = LabelPosition.Middle
        };

        // Add all annotations
        model.AddAnnotation(supportLevel);
        model.AddAnnotation(resistanceLevel);
        model.AddAnnotation(earnings);

        model.UpdateScales(800, 400);
        return model;
    }

    /// <summary>
    /// Demonstrates multiple Y axes functionality (P1-AX-MULTI)
    /// Shows price data on primary axis and volume on secondary axis
    /// </summary>
    private static ChartModel BuildMultiAxisDemo()
    {
        var model = new ChartModel 
        { 
            Theme = new LightTheme(),
            Title = "Stock Price & Volume (Multi-Axis Demo P1-AX-MULTI)"
        };

        // Ensure secondary Y axis exists
        model.EnsureSecondaryYAxis();

        // Stock price data (primary Y axis)
        var priceData = new[]
        {
            new PointD(0, 100.0), // Day 0: $100
            new PointD(1, 102.5), // Day 1: $102.50
            new PointD(2, 98.2),  // Day 2: $98.20
            new PointD(3, 105.8), // Day 3: $105.80
            new PointD(4, 108.1), // Day 4: $108.10
            new PointD(5, 103.5), // Day 5: $103.50
            new PointD(6, 110.3), // Day 6: $110.30
            new PointD(7, 115.7), // Day 7: $115.70
            new PointD(8, 112.4), // Day 8: $112.40
            new PointD(9, 118.9), // Day 9: $118.90
        };

        // Volume data (secondary Y axis) - much larger scale
        var volumeData = new[]
        {
            new BarPoint(0, 1200000), // Day 0: 1.2M shares
            new BarPoint(1, 850000),  // Day 1: 850K shares
            new BarPoint(2, 1800000), // Day 2: 1.8M shares (high volume on price drop)
            new BarPoint(3, 1100000), // Day 3: 1.1M shares
            new BarPoint(4, 950000),  // Day 4: 950K shares
            new BarPoint(5, 1350000), // Day 5: 1.35M shares
            new BarPoint(6, 1050000), // Day 6: 1.05M shares
            new BarPoint(7, 1600000), // Day 7: 1.6M shares (breakout volume)
            new BarPoint(8, 1000000), // Day 8: 1M shares
            new BarPoint(9, 1750000), // Day 9: 1.75M shares
        };

        // Add price series to primary Y axis (YAxisIndex = 0)
        var priceSeries = new LineSeries(priceData)
        {
            Title = "Price ($)",
            StrokeThickness = 3,
            YAxisIndex = 0 // Primary Y axis
        };
        model.Series.Add(priceSeries);

        // Add volume series to secondary Y axis (YAxisIndex = 1)
        var volumeSeries = new BarSeries(volumeData)
        {
            Title = "Volume (shares)",
            YAxisIndex = 1 // Secondary Y axis
        };
        model.Series.Add(volumeSeries);

        // Auto fit the ranges
        model.AutoFitDataRange();

        return model;
    }

    /// <summary>
    /// Demonstrates range annotation functionality (P1-ANN-RANGE)
    /// Shows horizontal and vertical range highlights with different styles
    /// </summary>
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