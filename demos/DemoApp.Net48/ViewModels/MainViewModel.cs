using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using FastCharts.Core;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes; // ajout pour ITheme
using FastCharts.Core.Themes.BuiltIn;

namespace DemoApp.Net48.ViewModels
{
    public sealed class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ChartModel> Charts { get; } = new ObservableCollection<ChartModel>();

        public ICommand ToggleThemeCommand { get; }
        public ICommand AddRandomSeriesCommand { get; }

        private string _selectedTheme = "Dark";
        public string SelectedTheme
        {
            get => _selectedTheme;
            set => UpdateSelectedTheme(value);
        }

        private bool UpdateSelectedTheme(string value)
        {
            if (_selectedTheme == value)
            {
                return false;
            }
            _selectedTheme = value;
            Raise(nameof(SelectedTheme));
            return true;
        }

        private void Raise(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public MainViewModel()
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
            Charts.Add(BuildLogYAxisGrowth());
            Charts.Add(BuildLogLogScatter());
            Charts.Add(BuildDualYAxisLogSecondary());

            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
            AddRandomSeriesCommand = new RelayCommand(_ => AddRandomSeries(), _ => Charts.Count > 0);
        }

        private void ToggleTheme()
        {
            SelectedTheme = SelectedTheme == "Dark" ? "Light" : "Dark";
            _dispatcher.Invoke(() =>
            {
                foreach (var c in Charts)
                {
                    c.Theme = SelectedTheme == "Dark" ? new DarkTheme() : new LightTheme();
                }
            });
        }

        private void AddRandomSeries()
        {
            if (Charts.Count == 0)
            {
                return;
            }
            var rand = new Random();
            var target = Charts[0]; // simple: ajoute sur le premier graphique
            var pts = Enumerable.Range(0, 40).Select(i => new PointD(i, rand.NextDouble() * 100)).ToArray();
            var series = new LineSeries(pts) { Title = "Rand " + (target.Series.Count + 1) };
            _dispatcher.Invoke(() =>
            {
                target.AddSeries(series);
                target.UpdateScales(800, 400);
            });
        }

        private ChartModel CreateBase(DateTime start, DateTime end)
        {
            var m = new ChartModel { Theme = new DarkTheme() };
            var dtAxis = new DateTimeAxis();
            dtAxis.SetVisibleRange(start, end);
            m.ReplaceXAxis(dtAxis);
            return m;
        }

        private ChartModel BuildMixedChart()
        {
            var start = DateTime.Today.AddDays(-14);
            var end = DateTime.Today.AddDays(1);
            var n = 201;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.5)).ToArray();
            var m = CreateBase(start, end);
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

        private ChartModel BuildBarsChart()
        {
            var start = DateTime.Today.AddDays(-10);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
            var buckets = 10;
            var bucketXs = Enumerable.Range(0, buckets).Select(i => start.AddDays(i)).ToArray();
            var barsA = bucketXs.Select((x, i) => new BarPoint(x.ToOADate(), (Math.Sin(i * 0.6) + 1.2) * 0.6)).ToArray();
            var barsB = bucketXs.Select((x, i) => new BarPoint(x.ToOADate(), (Math.Cos(i * 0.6) + 1.2) * 0.5)).ToArray();
            m.AddSeries(new BarSeries(barsA) { Title = "Bars A", GroupCount = 2, GroupIndex = 0, FillOpacity = 0.7 });
            m.AddSeries(new BarSeries(barsB) { Title = "Bars B", GroupCount = 2, GroupIndex = 1, FillOpacity = 0.7 });
            m.UpdateScales(800, 400);
            return m;
        }

        private ChartModel BuildStackedBarsChart()
        {
            var start = DateTime.Today.AddDays(-12);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
            var sbBuckets = 8;
            var sbXs = Enumerable.Range(0, sbBuckets).Select(i => start.AddDays(i * 1.5)).ToArray();
            var stackA = sbXs.Select((x, i) => new StackedBarPoint(x.ToOADate(), new[] { (Math.Sin(i * 0.5) + 1.1) * 0.4, (Math.Cos(i * 0.6) + 1.1) * 0.3, (Math.Sin(i * 0.7) + 1.1) * 0.2 })).ToArray();
            var stackB = sbXs.Select((x, i) => new StackedBarPoint(x.ToOADate(), new[] { (Math.Cos(i * 0.5) + 1.1) * 0.35, (Math.Sin(i * 0.6) + 1.1) * 0.25, (Math.Cos(i * 0.4) + 1.1) * 0.2 })).ToArray();
            m.AddSeries(new StackedBarSeries(stackA) { Title = "Stack A", GroupCount = 2, GroupIndex = 0, FillOpacity = 0.8 });
            m.AddSeries(new StackedBarSeries(stackB) { Title = "Stack B", GroupCount = 2, GroupIndex = 1, FillOpacity = 0.8 });
            m.UpdateScales(800, 400);
            return m;
        }

        private ChartModel BuildOhlcChart()
        {
            var start = DateTime.Today.AddDays(-20);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
            var rand = new Random(123);
            var n = 60;
            double price = 100;
            var list = new OhlcPoint[n];
            for (var i = 0; i < n; i++)
            {
                var open = price;
                var change = (rand.NextDouble() - 0.5) * 4;
                var close = open + change;
                var high = System.Math.Max(open, close) + rand.NextDouble() * 2;
                var low = System.Math.Min(open, close) - rand.NextDouble() * 2;
                price = close;
                list[i] = new OhlcPoint(start.AddDays(i).ToOADate(), open, high, low, close);
            }
            m.AddSeries(new OhlcSeries(list) { Title = "OHLC" });
            m.UpdateScales(800, 400);
            return m;
        }

        private ChartModel BuildErrorBarChart()
        {
            var start = DateTime.Today.AddDays(-10);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
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

        private ChartModel BuildMinimalLineChart()
        {
            var start = DateTime.Today.AddDays(-5);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
            var n = 60;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 2)).ToArray();
            var pts = xs.Select((x, i) => new PointD(x.ToOADate(), Math.Sin(i * 0.2) * 5 + 20)).ToArray();
            m.AddSeries(new LineSeries(pts) { Title = "Line" });
            m.UpdateScales(800, 400);
            return m;
        }

        private ChartModel BuildAreaOnly()
        {
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
            var n = 120;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i)).ToArray();
            var pts = xs.Select((x, i) => new PointD(x.ToOADate(), Math.Sin(i * 0.1) * 10 + 30)).ToArray();
            m.AddSeries(new AreaSeries(pts) { Title = "Area Only", Baseline = 20, FillOpacity = 0.45 });
            m.UpdateScales(800, 400);
            return m;
        }

        private ChartModel BuildScatterOnly()
        {
            var start = DateTime.Today.AddDays(-3);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
            var rand = new Random(789);
            var n = 80;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.2)).ToArray();
            var pts = xs.Select(x => new PointD(x.ToOADate(), 40 + Math.Sin(x.Ticks / 6e11) * 5 + rand.NextDouble() * 3)).ToArray();
            m.AddSeries(new ScatterSeries(pts) { Title = "Scatter Only", MarkerSize = 5.5 });
            m.UpdateScales(800, 400);
            return m;
        }

        private ChartModel BuildStepLine()
        {
            var start = DateTime.Today.AddDays(-6);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
            var n = 40;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 4)).ToArray();
            var pts = xs.Select((x, i) => new PointD(x.ToOADate(), (i % 2 == 0 ? 10 : 20) + (i % 5 == 0 ? 5 : 0))).ToArray();
            m.AddSeries(new StepLineSeries(pts) { Title = "Step", Mode = StepMode.Before, StrokeThickness = 2 });
            m.UpdateScales(800, 400);
            return m;
        }

        private ChartModel BuildSingleBars()
        {
            var start = DateTime.Today.AddDays(-8);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
            var n = 12;
            var xs = Enumerable.Range(0, n).Select(i => start.AddDays(i)).ToArray();
            var pts = xs.Select((x, i) => new BarPoint(x.ToOADate(), Math.Sin(i * 0.4) * 8 + 15)).ToArray();
            m.AddSeries(new BarSeries(pts) { Title = "Bars", FillOpacity = 0.75 });
            m.UpdateScales(800, 400);
            return m;
        }

        private ChartModel BuildStacked100()
        {
            var start = DateTime.Today.AddDays(-10);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
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

        private ChartModel BuildOhlcWithErrorOverlay()
        {
            var start = DateTime.Today.AddDays(-15);
            var end = DateTime.Today.AddDays(1);
            var m = CreateBase(start, end);
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
                var high = System.Math.Max(open, close) + rand.NextDouble() * 1.5;
                var low = System.Math.Min(open, close) - rand.NextDouble() * 1.5;
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

        private ChartModel BuildMultiSeriesTooltipShowcase()
        {
            var start = DateTime.Today.AddDays(-3);
            var end = DateTime.Today.AddDays(0.5);
            var m = CreateBase(start, end);
            var xs = Enumerable.Range(0, 300).Select(i => start.AddMinutes(i * 15)).ToArray();
            var line = xs.Select((x, i) => new PointD(x.ToOADate(), 50 + Math.Sin(i * 0.15) * 10 + Math.Sin(i * 0.03) * 5)).ToArray();
            m.AddSeries(new LineSeries(line) { Title = "Line A", StrokeThickness = 1.4 });
            var area = xs.Where((_, i) => i % 3 == 0).Select((x, i) => new PointD(x.ToOADate(), 40 + Math.Cos(i * 0.18) * 6)).ToArray();
            m.AddSeries(new AreaSeries(area) { Title = "Area B", Baseline = 35, FillOpacity = 0.35 });
            var scatter = xs.Where((_, i) => i % 20 == 0).Select((x, i) => new PointD(x.ToOADate(), 55 + Math.Sin(i * 0.9) * 4)).ToArray();
            m.AddSeries(new ScatterSeries(scatter) { Title = "Scatter C", MarkerSize = 4 });
            var dayXs = Enumerable.Range(0, 4).Select(i => start.AddDays(i)).ToArray();
            var bars = dayXs.Select((x, i) => new BarPoint(x.ToOADate(), 30 + i * 2 + Math.Sin(i) * 3)).ToArray();
            m.AddSeries(new BarSeries(bars) { Title = "Bars D", FillOpacity = 0.6 });
            m.UpdateScales(800, 400);
            return m;
        }

        // Nouvel exemple : croissance exponentielle avec axe Y logarithmique
        private ChartModel BuildLogYAxisGrowth()
        {
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today.AddDays(0.5);
            var m = CreateBase(start, end);
            m.Title = "Croissance (Y Log)";
            var logY = new LogarithmicAxis { LogBase = 10.0 };
            logY.SetVisibleRange(1, 10000); // 10^0 à 10^4
            m.ReplaceYAxis(logY);
            var n = 120;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.2)).ToArray();
            // y augmente ~exp pour démonstration
            var pts = xs.Select((x, i) => new PointD(x.ToOADate(), Math.Pow(10, i / 30.0))).ToArray();
            m.AddSeries(new LineSeries(pts) { Title = "Exp", StrokeThickness = 1.6 });
            m.UpdateScales(800, 400);
            return m;
        }

        // Nouvel exemple : scatter log-log (axes X et Y logarithmiques)
        private ChartModel BuildLogLogScatter()
        {
            var m = new ChartModel { Theme = new DarkTheme(), Title = "Scatter Log-Log" };
            var logX = new LogarithmicAxis { LogBase = 10.0 }; logX.SetVisibleRange(1, 1000);
            var logY = new LogarithmicAxis { LogBase = 10.0 }; logY.SetVisibleRange(1, 100000);
            m.ReplaceXAxis(logX);
            m.ReplaceYAxis(logY);
            var xs = Enumerable.Range(0, 60).Select(i => 1.0 + i * (999.0 / 59.0)).ToArray();
            // relation puissance y = x^2.5 environ
            var pts = xs.Select(x => new PointD(x, Math.Pow(x, 2.5))).ToArray();
            m.AddSeries(new ScatterSeries(pts) { Title = "x^2.5", MarkerSize = 4 });
            m.UpdateScales(800, 400);
            return m;
        }

        // Nouvel exemple : axe Y secondaire logarithmique
        private ChartModel BuildDualYAxisLogSecondary()
        {
            var start = DateTime.Today.AddDays(-5);
            var end = DateTime.Today.AddDays(0.2);
            var m = CreateBase(start, end);
            m.Title = "Dual Y (secondaire log)";
            // Série linéaire sur axe primaire
            var n = 80;
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.5)).ToArray();
            var linear = xs.Select((x, i) => new PointD(x.ToOADate(), 50 + Math.Sin(i * 0.2) * 10)).ToArray();
            m.AddSeries(new LineSeries(linear) { Title = "Lin", StrokeThickness = 1.4, YAxisIndex = 0 });
            // Série exponentielle sur axe secondaire log
            m.EnsureSecondaryYAxis();
            var logAxis = new LogarithmicAxis { LogBase = 10.0 }; logAxis.SetVisibleRange(1, 10000);
            m.ReplaceSecondaryYAxis(logAxis);
            var exp = xs.Select((x, i) => new PointD(x.ToOADate(), Math.Pow(10, i / 20.0))).ToArray();
            m.AddSeries(new LineSeries(exp) { Title = "Exp", StrokeThickness = 1.6, YAxisIndex = 1 });
            m.UpdateScales(800, 400);
            return m;
        }
    }

    internal sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
