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
    }
}
