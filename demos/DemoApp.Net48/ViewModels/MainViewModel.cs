using DemoApp.Net48.Commands;
using DemoApp.Net48.Constants;
using DemoApp.Net48.Services;
using DemoApp.Net48.Services.Abstractions;
using DemoApp.Net48.ViewModels.Base;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Annotations;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Rendering.Skia;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

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
            Charts.Add(BuildMixedChart());          // 0
            Charts.Add(BuildBarsChart());           // 1
            Charts.Add(BuildStackedBarsChart());    // 2
            Charts.Add(BuildOhlcChart());           // 3
            Charts.Add(BuildErrorBarChart());       // 4
            Charts.Add(BuildMinimalLineChart());    // 5
            Charts.Add(BuildRangeAnnotationDemo()); // 6
            Charts.Add(BuildLogarithmicAxisDemo()); // 7 - P1-AX-LOG

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

        /// <summary>
        /// Downsamples data using the Largest Triangle Three Buckets (LTTB) algorithm.
        /// Simplified but valid implementation for demo purposes.
        /// </summary>
        public static PointD[] LTTB(DateTime[] xValues, double[] yValues, int numPoints)
        {
            if (xValues == null)
            {
                throw new ArgumentNullException(nameof(xValues));
            }
            if (yValues == null)
            {
                throw new ArgumentNullException(nameof(yValues));
            }
            if (numPoints <= 2)
            {
                throw new ArgumentOutOfRangeException(nameof(numPoints));
            }
            if (xValues.Length != yValues.Length)
            {
                throw new ArgumentException("X and Y value arrays must have the same length.");
            }
            if (numPoints >= xValues.Length)
            {
                return xValues.Zip(yValues, (x, y) => new PointD(x.ToOADate(), y)).ToArray();
            }

            var dataCount = xValues.Length;
            var sampled = new List<PointD>(numPoints) { new PointD(xValues[0].ToOADate(), yValues[0]) };
            var xDouble = xValues.Select(v => v.ToOADate()).ToArray();
            var bucketSize = (double)(dataCount - 2) / (numPoints - 2);
            var a = 0;

            for (var i = 0; i < numPoints - 2; i++)
            {
                var bucketStart = (int)Math.Floor(i * bucketSize) + 1;
                var bucketEnd = (int)Math.Floor((i + 1) * bucketSize) + 1;
                if (bucketEnd >= dataCount)
                {
                    bucketEnd = dataCount - 1;
                }

                var nextBucketStart = (int)Math.Floor((i + 1) * bucketSize) + 1;
                var nextBucketEnd = (int)Math.Floor((i + 2) * bucketSize) + 1;
                if (nextBucketEnd >= dataCount)
                {
                    nextBucketEnd = dataCount;
                }
                var avgRange = nextBucketEnd - nextBucketStart;
                double avgX = 0, avgY = 0;
                if (avgRange > 0)
                {
                    for (var j = nextBucketStart; j < nextBucketEnd; j++)
                    {
                        avgX += xDouble[j];
                        avgY += yValues[j];
                    }
                    avgX /= avgRange;
                    avgY /= avgRange;
                }
                else
                {
                    avgX = xDouble[bucketEnd];
                    avgY = yValues[bucketEnd];
                }

                double maxArea = -1;
                var maxAreaIndex = bucketStart;
                for (var j = bucketStart; j < bucketEnd; j++)
                {
                    var area = Math.Abs((xDouble[a] - avgX) * (yValues[j] - yValues[a]) - (xDouble[a] - xDouble[j]) * (avgY - yValues[a]));
                    if (area > maxArea)
                    {
                        maxArea = area;
                        maxAreaIndex = j;
                    }
                }

                sampled.Add(new PointD(xDouble[maxAreaIndex], yValues[maxAreaIndex]));
                a = maxAreaIndex;
            }

            sampled.Add(new PointD(xValues[dataCount - 1].ToOADate(), yValues[dataCount - 1]));
            return sampled.ToArray();
        }
    }
}
