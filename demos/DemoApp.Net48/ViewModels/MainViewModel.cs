using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Themes.BuiltIn;
using DemoApp.Net48.ViewModels.Base;
using DemoApp.Net48.Commands;
using DemoApp.Net48.Constants;
using DemoApp.Net48.Services.Abstractions;
using DemoApp.Net48.Services;
using FastCharts.Core.Annotations;

namespace DemoApp.Net48.ViewModels
{
    /// <summary>
    /// Main view model for the .NET Framework 4.8 demo application
    /// </summary>
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly IChartCreationService _chartCreationService;
        private readonly Dispatcher _dispatcher;
        private string _selectedTheme = ThemeConstants.Dark;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class
        /// </summary>
        public MainViewModel() : this(new ChartCreationService())
        {
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
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class with dependency injection
        /// </summary>
        /// <param name="chartCreationService">Service for creating charts</param>
        public MainViewModel(IChartCreationService chartCreationService)
        {
            _chartCreationService = chartCreationService ?? throw new ArgumentNullException(nameof(chartCreationService));
            _dispatcher = Dispatcher.CurrentDispatcher;

            Charts = new ObservableCollection<ChartModel>();
            InitializeCommands();
            LoadCharts();
        }

        /// <summary>
        /// Gets the collection of charts displayed in the UI
        /// </summary>
        public ObservableCollection<ChartModel> Charts { get; }

        /// <summary>
        /// Gets the command for toggling between themes
        /// </summary>
        public ICommand ToggleThemeCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command for adding random series to charts
        /// </summary>
        public ICommand AddRandomSeriesCommand { get; private set; } = null!;

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
        }

        private void LoadCharts()
        {
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
            var supportLevel = AnnotationLine.Horizontal(100.0, "Support Level ($100)");
            supportLevel.Color = new ColorRgba(0, 255, 0, 180); // Green
            supportLevel.LineStyle = LineStyle.Dashed;
            supportLevel.Thickness = 2.0;
            supportLevel.LabelPosition = LabelPosition.Start;

            var resistanceLevel = AnnotationLine.Horizontal(115.0, "Resistance Level ($115)");
            resistanceLevel.Color = new ColorRgba(255, 0, 0, 180); // Red
            resistanceLevel.LineStyle = LineStyle.Dashed;
            resistanceLevel.Thickness = 2.0;
            resistanceLevel.LabelPosition = LabelPosition.Start;

            // Add vertical annotations for important events
            var earnings = AnnotationLine.Vertical(3.0, "Earnings Report");
            earnings.Color = new ColorRgba(128, 0, 128, 160); // Purple
            earnings.LineStyle = LineStyle.Dotted;
            earnings.Thickness = 2.0;
            earnings.LabelPosition = LabelPosition.Middle;

            // Add all annotations
            model.AddAnnotation(supportLevel);
            model.AddAnnotation(resistanceLevel);
            model.AddAnnotation(earnings);

            model.UpdateScales(800, 400);
            return model;
        }
    }
}
