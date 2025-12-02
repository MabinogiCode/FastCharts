using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Annotations;
using FastCharts.Rendering.Skia;
using DemoApp.Net48.ViewModels.Base;
using DemoApp.Net48.Commands;
using DemoApp.Net48.Constants;
using DemoApp.Net48.Services.Abstractions;
using DemoApp.Net48.Services;
using Microsoft.Win32;

namespace DemoApp.Net48.ViewModels
{
    /// <summary>
    /// Main view model for the .NET Framework 4.8 demo application
    /// </summary>
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly IChartCreationService _chartCreationService;
        private readonly Dispatcher _dispatcher;
        private readonly SkiaChartRenderer _renderer;
        private string _selectedTheme = ThemeConstants.Dark;
        private ChartModel? _selectedChart;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class
        /// </summary>
        public MainViewModel() : this(new ChartCreationService())
        {
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class with dependency injection
        /// </summary>
        /// <param name="chartCreationService">Service for creating charts</param>
        public MainViewModel(IChartCreationService chartCreationService)
        {
            _chartCreationService = chartCreationService ?? throw new ArgumentNullException(nameof(chartCreationService));
            _dispatcher = Dispatcher.CurrentDispatcher;
            _renderer = new SkiaChartRenderer();

            Charts = new ObservableCollection<ChartModel>();
            InitializeCommands();
            LoadCharts();
            
            // Select first chart by default
            SelectedChart = Charts.FirstOrDefault();
        }

        /// <summary>
        /// Gets the collection of charts displayed in the UI
        /// </summary>
        public ObservableCollection<ChartModel> Charts { get; }

        /// <summary>
        /// Gets or sets the currently selected chart for export operations
        /// </summary>
        public ChartModel? SelectedChart
        {
            get => _selectedChart;
            set => SetProperty(ref _selectedChart, value);
        }

        /// <summary>
        /// Gets the command for toggling between themes
        /// </summary>
        public ICommand ToggleThemeCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command for adding random series to charts
        /// </summary>
        public ICommand AddRandomSeriesCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command for exporting the selected chart to PNG
        /// </summary>
        public ICommand ExportSelectedChartCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command for exporting all charts to PNG files
        /// </summary>
        public ICommand ExportAllChartsCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command for copying the selected chart to clipboard
        /// </summary>
        public ICommand CopySelectedChartToClipboardCommand { get; private set; } = null!;

        /// <summary>
        /// Gets or sets the selected theme name
        /// </summary>
        public string SelectedTheme
        {
            get => _selectedTheme;
            set => SetProperty(ref _selectedTheme, value);
        }

        private void InitializeCommands()
        {
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
            AddRandomSeriesCommand = new RelayCommand(
                _ => AddRandomSeries(), 
                _ => Charts.Count > 0);
            
            // P1-EXPORT-PNG: Add export commands
            ExportSelectedChartCommand = new AsyncRelayCommand(
                _ => ExportSelectedChart(),
                _ => SelectedChart != null);
            ExportAllChartsCommand = new AsyncRelayCommand(_ => ExportAllCharts());
            CopySelectedChartToClipboardCommand = new AsyncRelayCommand(
                _ => CopySelectedChartToClipboard(),
                _ => SelectedChart != null);
        }

        private void LoadCharts()
        {
            // Basic demo charts
            Charts.Add(BuildMixedChart());
            Charts.Add(BuildBarsChart());
            Charts.Add(BuildStackedBarsChart());
            Charts.Add(BuildOhlcChart());
            Charts.Add(BuildErrorBarChart());
            Charts.Add(BuildMinimalLineChart());
            Charts.Add(BuildAreaOnly());
            Charts.Add(BuildScatterOnly());
            Charts.Add(BuildStepLine());
            Charts.Add(BuildSingleBars());
            Charts.Add(BuildStacked100());
            Charts.Add(BuildOhlcWithErrorOverlay());
            Charts.Add(BuildMultiSeriesTooltipShowcase());
            
            // P1-AX-CAT: Add CategoryAxis demo
            Charts.Add(BuildCategoryAxisDemo());
            
            // P1-ANN-LINE: Add Annotation demo
            Charts.Add(BuildAnnotationDemo());
            
            // P1-AX-MULTI: Add Multi-Axis demo
            Charts.Add(BuildMultiAxisDemo());
            
            // P1-ANN-RANGE: Add Range Annotation demo
            Charts.Add(BuildRangeAnnotationDemo());
            
            // Add demo charts from service
            var demoCharts = _chartCreationService.CreateDemoCharts();
            foreach (var chart in demoCharts)
            {
                Charts.Add(chart);
            }
        }

        private void ToggleTheme()
        {
            SelectedTheme = SelectedTheme == ThemeConstants.Dark 
                ? ThemeConstants.Light 
                : ThemeConstants.Dark;

            _dispatcher.Invoke(() =>
            {
                ApplyThemeToAllCharts();
            });
        }

        private void AddRandomSeries()
        {
            if (Charts.Count == 0)
            {
                return;
            }

            var targetChart = Charts.FirstOrDefault();
            if (targetChart == null)
            {
                return;
            }

            var randomChart = _chartCreationService.CreateRandomChart(40);
            var randomSeries = randomChart.Series.FirstOrDefault();
            
            if (randomSeries != null)
            {
                randomSeries.Title = $"Rand {targetChart.Series.Count + 1}";
                
                _dispatcher.Invoke(() =>
                {
                    targetChart.AddSeries(randomSeries);
                    targetChart.UpdateScales(800, 400);
                });
            }
        }

        private void ApplyThemeToAllCharts()
        {
            var theme = SelectedTheme == ThemeConstants.Dark 
                ? (ITheme)new DarkTheme() 
                : new LightTheme();

            foreach (var chart in Charts)
            {
                chart.Theme = theme;
            }
        }

        // P1-EXPORT-PNG: Export functionality for .NET Framework 4.8
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
                FileName = $"{SelectedChart.Title?.Replace(" ", "_") ?? "Chart"}.png",
                DefaultExt = ".png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        await _renderer.ExportPngAsync(SelectedChart, fileStream, 1200, 800, quality: 95);
                    }
                    
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
            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder for exporting all charts",
                ShowNewFolderButton = true
            })
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var folderPath = folderDialog.SelectedPath;
                    
                    try
                    {
                        var exportTasks = Charts.Select(async (chart, index) =>
                        {
                            var fileName = $"{index:00}_{chart.Title?.Replace(" ", "_") ?? "Chart"}.png";
                            var fullPath = Path.Combine(folderPath, fileName);
                            
                            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                            {
                                await _renderer.ExportPngAsync(chart, fileStream, 1200, 800, quality: 95);
                            }
                            
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
                using (var memoryStream = new MemoryStream())
                using (var image = SkiaSharp.SKImage.FromPixels(bitmap.PeekPixels()))
                using (var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                {
                    data.SaveTo(memoryStream);
                    memoryStream.Position = 0;
                    var drawingBitmap = new System.Drawing.Bitmap(memoryStream);
                    
                    // Copy to clipboard
                    System.Windows.Clipboard.SetImage(System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        drawingBitmap.GetHbitmap(),
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()));
                    
                    drawingBitmap.Dispose();
                }
                
                bitmap.Dispose();
                
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

        // Chart builder methods
        private static ChartModel CreateBase(DateTime start, DateTime end, string title = "Chart")
        {
            var m = new ChartModel { Theme = new DarkTheme(), Title = title };
            var dtAxis = new DateTimeAxis();
            dtAxis.SetVisibleRange(start, end);
            m.ReplaceXAxis(dtAxis);
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
            var start = DateTime.Today.AddDays(-10);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "Bar Chart");
            var buckets = 10;
            var bucketXs = Enumerable.Range(0, buckets).Select(i => start.AddDays(i)).ToArray();
            var barsA = bucketXs.Select((x, i) => new BarPoint(x.ToOADate(), (Math.Sin(i * 0.6) + 1.2) * 0.6)).ToArray();
            var barsB = bucketXs.Select((x, i) => new BarPoint(x.ToOADate(), (Math.Cos(i * 0.6) + 1.2) * 0.5)).ToArray();
            m.AddSeries(new BarSeries(barsA) { Title = "Bars A", GroupCount = 2, GroupIndex = 0, FillOpacity = 0.7 });
            m.AddSeries(new BarSeries(barsB) { Title = "Bars B", GroupCount = 2, GroupIndex = 1, FillOpacity = 0.7 });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildStackedBarsChart()
        {
            var start = DateTime.Today.AddDays(-12);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "Stacked Bars");
            var sbBuckets = 8;
            var sbXs = Enumerable.Range(0, sbBuckets).Select(i => start.AddDays(i * 1.5)).ToArray();
            var stackA = sbXs.Select((x, i) => new StackedBarPoint(x.ToOADate(), new[] { (Math.Sin(i * 0.5) + 1.1) * 0.4, (Math.Cos(i * 0.6) + 1.1) * 0.3, (Math.Sin(i * 0.7) + 1.1) * 0.2 })).ToArray();
            m.AddSeries(new StackedBarSeries(stackA) { Title = "Stack A", FillOpacity = 0.8 });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildOhlcChart()
        {
            var start = DateTime.Today.AddDays(-20);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "OHLC Chart");
            var rand = new Random(123);
            var n = 60;
            double price = 100;
            var list = new OhlcPoint[n];
            for (var i = 0; i < n; i++)
            {
                var open = price;
                var change = (rand.NextDouble() - 0.5) * 4;
                var close = open + change;
                var high = Math.Max(open, close) + rand.NextDouble() * 2;
                var low = Math.Min(open, close) - rand.NextDouble() * 2;
                price = close;
                list[i] = new OhlcPoint(start.AddDays(i).ToOADate(), open, high, low, close);
            }
            m.AddSeries(new OhlcSeries(list) { Title = "OHLC" });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildErrorBarChart()
        {
            var start = DateTime.Today.AddDays(-10);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "Error Bar Chart");
            var n = 20;
            var xs = Enumerable.Range(0, n).Select(i => start.AddDays(i * 0.5)).ToArray();
            var rand = new Random(456);
            var pts = xs.Select(x =>
            {
                var y = 50 + Math.Sin(x.DayOfYear * 0.2) * 10 + rand.NextDouble() * 4;
                var err = 2 + rand.NextDouble() * 2;
                return new ErrorBarPoint(x.ToOADate(), y, err, err * (0.5 + rand.NextDouble() * 0.5));
            }).ToArray();
            m.AddSeries(new ErrorBarSeries(pts) { Title = "ErrorBars" });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildMinimalLineChart()
        {
            var start = DateTime.Today.AddDays(-5);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "Minimal Line Chart");
            var n = 60;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 2)).ToArray();
            var pts = xs.Select((x, i) => new PointD(x.ToOADate(), Math.Sin(i * 0.2) * 5 + 20)).ToArray();
            m.AddSeries(new LineSeries(pts) { Title = "Line" });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildAreaOnly()
        {
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "Area Only");
            var n = 120;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i)).ToArray();
            var pts = xs.Select((x, i) => new PointD(x.ToOADate(), Math.Sin(i * 0.1) * 10 + 30)).ToArray();
            m.AddSeries(new AreaSeries(pts) { Title = "Area Only", Baseline = 20, FillOpacity = 0.45 });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildScatterOnly()
        {
            var start = DateTime.Today.AddDays(-3);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "Scatter Only");
            var rand = new Random(789);
            var n = 80;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.2)).ToArray();
            var pts = xs.Select(x => new PointD(x.ToOADate(), 40 + Math.Sin(x.Ticks / 6e11) * 5 + rand.NextDouble() * 3)).ToArray();
            m.AddSeries(new ScatterSeries(pts) { Title = "Scatter Only", MarkerSize = 5.5 });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildStepLine()
        {
            var start = DateTime.Today.AddDays(-6);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "Step Line");
            var n = 40;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 4)).ToArray();
            var pts = xs.Select((x, i) => new PointD(x.ToOADate(), (i % 2 == 0 ? 10 : 20) + (i % 5 == 0 ? 5 : 0))).ToArray();
            m.AddSeries(new StepLineSeries(pts) { Title = "Step", Mode = StepMode.Before, StrokeThickness = 2 });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildSingleBars()
        {
            var start = DateTime.Today.AddDays(-8);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "Single Bars");
            var n = 12;
            var xs = Enumerable.Range(0, n).Select(i => start.AddDays(i)).ToArray();
            var pts = xs.Select((x, i) => new BarPoint(x.ToOADate(), Math.Sin(i * 0.4) * 8 + 15)).ToArray();
            m.AddSeries(new BarSeries(pts) { Title = "Bars", FillOpacity = 0.75 });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildStacked100()
        {
            var start = DateTime.Today.AddDays(-10);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "Stacked 100%");
            var n = 10;
            var xs = Enumerable.Range(0, n).Select(i => start.AddDays(i)).ToArray();
            var rand = new Random(321);
            var stackPoints = xs.Select((x, i) =>
            {
                var a = rand.NextDouble() * 1 + 0.2;
                var b = rand.NextDouble() * 1 + 0.2;
                var c = rand.NextDouble() * 1 + 0.2;
                var sum = a + b + c;
                return new StackedBarPoint(x.ToOADate(), new[] { a / sum, b / sum, c / sum });
            }).ToArray();
            m.AddSeries(new StackedBarSeries(stackPoints) { Title = "Stacked 100%", FillOpacity = 0.85 });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildOhlcWithErrorOverlay()
        {
            var start = DateTime.Today.AddDays(-15);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end, "OHLC + Error");
            var rand = new Random(654);
            var n = 40;
            double price = 50;
            var ohlc = new OhlcPoint[n];
            var errs = new ErrorBarPoint[n];
            for (var i = 0; i < n; i++)
            {
                var open = price;
                var change = (rand.NextDouble() - 0.5) * 3;
                var close = open + change;
                var high = Math.Max(open, close) + rand.NextDouble() * 1.5;
                var low = Math.Min(open, close) - rand.NextDouble() * 1.5;
                price = close;
                var xOa = start.AddDays(i).ToOADate();
                ohlc[i] = new OhlcPoint(xOa, open, high, low, close);
                var central = (open + close) * 0.5;
                var err = 0.5 + rand.NextDouble();
                errs[i] = new ErrorBarPoint(xOa, central, err, err * 0.7);
            }
            m.AddSeries(new OhlcSeries(ohlc) { Title = "OHLC" });
            m.AddSeries(new ErrorBarSeries(errs) { Title = "Err" });
            m.UpdateScales(800, 400);
            return m;
        }

        private static ChartModel BuildMultiSeriesTooltipShowcase()
        {
            var start = DateTime.Today.AddDays(-3);
            var end = DateTime.Today.AddDays(0.5);
            var m = CreateBase(start, end, "Multi-Series Tooltip");
            var xs = Enumerable.Range(0, 300).Select(i => start.AddMinutes(i * 15)).ToArray();
            var line = xs.Select((x, i) => new PointD(x.ToOADate(), 50 + Math.Sin(i * 0.15) * 10 + Math.Sin(i * 0.03) * 5)).ToArray();
            m.AddSeries(new LineSeries(line) { Title = "Line A", StrokeThickness = 1.4 });
            var area = xs.Where((_, i) => i % 3 == 0).Select((x, i) => new PointD(x.ToOADate(), 40 + Math.Cos(i * 0.18) * 6)).ToArray();
            m.AddSeries(new AreaSeries(area) { Title = "Area B", Baseline = 35, FillOpacity = 0.35 });
            var scatter = xs.Where((_, i) => i % 20 == 0).Select((x, i) => new PointD(x.ToOADate(), 55 + Math.Sin(i * 0.9) * 4)).ToArray();
            m.AddSeries(new ScatterSeries(scatter) { Title = "Scatter C", MarkerSize = 4 });
            m.UpdateScales(800, 400);
            return m;
        }

        /// <summary>
        /// Demonstrates CategoryAxis functionality (P1-AX-CAT)
        /// Shows quarterly sales data with category labels
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
    }
}
