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

        /// <summary>
        /// Gets the command for resetting the view of the selected chart
        /// </summary>
        public ReactiveCommand<Unit, Unit> ResetViewCommand { get; private set; } = null!;

        /// <summary>
        /// Disposes resources used by this view model
        /// </summary>
        public void Dispose()
        {
            _animationSubscription?.Dispose();
            
            foreach (var chart in Charts)
            {
                try
                {
                    chart.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
            
            Charts.Clear();
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

        private static void RunOnUi(Action action)
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
