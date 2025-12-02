using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Annotations;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Themes.BuiltIn;
using ReactiveUI;
using DemoApp.Net8.Constants;
using DemoApp.Net8.Services.Abstractions;
using DemoApp.Net8.Services;

namespace DemoApp.Net8.ViewModels
{
    /// <summary>
    /// Reactive ViewModel demonstrating full MVVM with ReactiveUI and FastCharts for .NET 8
    /// </summary>
    public sealed class MainViewModel : ReactiveObject, IDisposable
    {
        private readonly IChartCreationService _chartCreationService;
        private readonly IScheduler _uiScheduler;
        private ChartModel? _selectedChart;
        private string _selectedTheme = ThemeConstants.Light;
        private bool _allowInteraction = true;
        private double _animationProgress;
        private IDisposable? _animationSubscription;

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
            _uiScheduler = DispatcherScheduler.Current;

            Charts = new ObservableCollection<ChartModel>();

            InitializeCommands();
            LoadCharts();
            SetupReactiveBindings();
            
            SelectedChart = Charts.FirstOrDefault();
        }

        /// <summary>
        /// Gets the collection of all available charts
        /// </summary>
        public ObservableCollection<ChartModel> Charts { get; }

        /// <summary>
        /// Gets or sets the currently selected chart for operations
        /// </summary>
        public ChartModel? SelectedChart
        {
            get => _selectedChart;
            set => this.RaiseAndSetIfChanged(ref _selectedChart, value);
        }

        /// <summary>
        /// Gets or sets the selected theme name for reactive theme switching
        /// </summary>
        public string SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        /// <summary>
        /// Gets or sets whether user can interact with charts
        /// </summary>
        public bool AllowInteraction
        {
            get => _allowInteraction;
            set => this.RaiseAndSetIfChanged(ref _allowInteraction, value);
        }

        /// <summary>
        /// Gets or sets the animation progress for demo purposes (0.0 to 1.0)
        /// </summary>
        public double AnimationProgress
        {
            get => _animationProgress;
            set => this.RaiseAndSetIfChanged(ref _animationProgress, value);
        }

        /// <summary>
        /// Gets the command for toggling between themes
        /// </summary>
        public ReactiveCommand<Unit, Unit> ToggleThemeCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command for adding random series to the selected chart
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddRandomSeriesCommand { get; private set; } = null!;

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
        
        // P1-AX-CAT: Add CategoryAxis demo
        Charts.Add(BuildCategoryAxisDemo());    //14 - CategoryAxis demo
        
        // P1-ANN-LINE: Add Annotation demo
        Charts.Add(BuildAnnotationDemo());      //15 - Annotation demo
    }

    private void ToggleTheme()
    {
        // ✅ SIMPLIFIED: ReactiveUI commands automatically execute on UI thread
        SelectedTheme = SelectedTheme == "Light" ? "Dark" : "Light";
    }

        private void InitializeCommands()
        {
            var canToggleTheme = this.WhenAnyValue(x => x.SelectedTheme)
                .Select(_ => true)
                .ObserveOn(_uiScheduler);
                
            var canAddSeries = this.WhenAnyValue(x => x.SelectedChart)
                .Select(chart => chart != null)
                .ObserveOn(_uiScheduler);
                
            var canReset = this.WhenAnyValue(x => x.SelectedChart)
                .Select(chart => chart != null)
                .ObserveOn(_uiScheduler);

            ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme, canToggleTheme, _uiScheduler);
            AddRandomSeriesCommand = ReactiveCommand.Create(AddRandomSeries, canAddSeries, _uiScheduler);
            ResetViewCommand = ReactiveCommand.Create(ResetView, canReset, _uiScheduler);
        }

        private void LoadCharts()
        {
            RunOnUi(() =>
            {
                var demoCharts = _chartCreationService.CreateDemoCharts();
                foreach (var chart in demoCharts)
                {
                    Charts.Add(chart);
                }
            });
        }

        private void SetupReactiveBindings()
        {
            this.WhenAnyValue(x => x.SelectedTheme)
                .Where(theme => !string.IsNullOrEmpty(theme))
                .ObserveOn(_uiScheduler)
                .Subscribe(ApplyThemeToAllCharts);

            this.WhenAnyValue(x => x.AllowInteraction)
                .ObserveOn(_uiScheduler)
                .Subscribe(_ => { /* Handle interaction changes if needed */ });

            this.WhenAnyValue(x => x.SelectedChart)
                .Where(chart => chart != null)
                .ObserveOn(_uiScheduler)
                .Subscribe(chart => System.Diagnostics.Debug.WriteLine($"Selected chart changed to: {chart!.Title}"));

            _animationSubscription = Observable.Interval(TimeSpan.FromMilliseconds(100))
                .ObserveOn(_uiScheduler)
                .Subscribe(_ => UpdateAnimation());
        }

        private void ToggleTheme()
        {
            SelectedTheme = SelectedTheme == ThemeConstants.Light 
                ? ThemeConstants.Dark 
                : ThemeConstants.Light;
        }

        private void AddRandomSeries()
        {
            if (SelectedChart == null)
            {
                return;
            }

            var randomChart = _chartCreationService.CreateRandomChart(50);
            var randomSeries = randomChart.Series.FirstOrDefault();
            
            if (randomSeries != null)
            {
                randomSeries.Title = $"Random {SelectedChart.Series.Count + 1}";
                randomSeries.StrokeThickness = 2.0;
                
                RunOnUi(() =>
                {
                    SelectedChart.AddSeries(randomSeries);
                });
            }
        }

        private void ResetView()
        {
            if (SelectedChart == null)
            {
                return;
            }

            RunOnUi(() => SelectedChart.AutoFitDataRange());
        }

        private void ApplyThemeToAllCharts(string themeName)
        {
            RunOnUi(() =>
            {
                var theme = themeName == ThemeConstants.Dark 
                    ? (ITheme)new DarkTheme() 
                    : new LightTheme();

                foreach (var chart in Charts)
                {
                    chart.Theme = theme;
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
                // Placeholder for realtime chart updates when we have enough charts
            }
            else
            {
                // Placeholder for realtime chart updates
            }
        }

    /// <summary>
    /// P1-AX-CAT: Build a demo chart showcasing the CategoryAxis functionality
    /// This is specifically for demonstrating the category axis capabilities in a dedicated example.
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
        var supportLevel = AnnotationLine.Horizontal(100.0, "Support Level ($100)");
        supportLevel.Color = new ColorRgba(0, 255, 0, 180); // Green
        supportLevel.LineStyle = FastCharts.Core.Annotations.LineStyle.Dashed;
        supportLevel.Thickness = 2.0;
        supportLevel.LabelPosition = FastCharts.Core.Annotations.LabelPosition.Start;

        var resistanceLevel = AnnotationLine.Horizontal(115.0, "Resistance Level ($115)");
        resistanceLevel.Color = new ColorRgba(255, 0, 0, 180); // Red
        resistanceLevel.LineStyle = FastCharts.Core.Annotations.LineStyle.Dashed;
        resistanceLevel.Thickness = 2.0;
        resistanceLevel.LabelPosition = FastCharts.Core.Annotations.LabelPosition.Start;

        // Add vertical annotations for important events
        var earnings = AnnotationLine.Vertical(3.0, "Earnings Report");
        earnings.Color = new ColorRgba(128, 0, 128, 160); // Purple
        earnings.LineStyle = FastCharts.Core.Annotations.LineStyle.Dotted;
        earnings.Thickness = 2.0;
        earnings.LabelPosition = FastCharts.Core.Annotations.LabelPosition.Middle;

        var announcement = AnnotationLine.Vertical(7.0, "Product Launch");
        announcement.Color = new ColorRgba(0, 0, 255, 160); // Blue
        announcement.LineStyle = FastCharts.Core.Annotations.LineStyle.Solid;
        announcement.Thickness = 1.0;
        announcement.LabelPosition = FastCharts.Core.Annotations.LabelPosition.Start;

        // Add all annotations
        model.AddAnnotation(supportLevel);
        model.AddAnnotation(resistanceLevel);
        model.AddAnnotation(earnings);
        model.AddAnnotation(announcement);

        model.UpdateScales(800, 400);
        return model;
    }

    public void Dispose()
    {
        // ✅ CRITICAL FIX: Enhanced dispose pattern for ViewModels
        _animationSubscription?.Dispose();

        // ✅ CRITICAL FIX: Dispose all charts with proper error handling
        foreach (var chart in Charts)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }
    }
}
