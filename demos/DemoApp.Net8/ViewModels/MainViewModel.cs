using System;
using System.Collections.ObjectModel;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Axes;

namespace DemoApp.Net8.ViewModels;

public sealed class MainViewModel
{
    public ObservableCollection<ChartModel> Charts { get; } = new();

    public MainViewModel()
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
        Charts.Add(BuildOhlcWithErrorOverlay()); //11
        Charts.Add(BuildMultiSeriesTooltipShowcase()); //12
    }

    private ChartModel CreateBase(DateTime start, DateTime end)
    {
        var m = new ChartModel { Theme = new LightTheme() };
        var dtAxis = new DateTimeAxis();
        dtAxis.SetVisibleRange(start, end);
        m.ReplaceXAxis(dtAxis);
        return m;
    }

    private ChartModel BuildMixedChart()
    {
        var start = DateTime.Today.AddDays(-14);
        var end = DateTime.Today.AddDays(1);
        int n = 201;
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
        int buckets = 10;
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
        int sbBuckets = 8;
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
        int n = 60;
        double price = 100;
        var list = new OhlcPoint[n];
        for (int i = 0; i < n; i++)
        {
            double open = price;
            double change = (rand.NextDouble() - 0.5) * 4;
            double close = open + change;
            double high = System.Math.Max(open, close) + rand.NextDouble() * 2;
            double low = System.Math.Min(open, close) - rand.NextDouble() * 2;
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
        int n = 20;
        var xs = Enumerable.Range(0, n).Select(i => start.AddDays(i * 0.5)).ToArray();
        var rand = new Random(456);
        var pts = xs.Select(x =>
        {
            double y = 50 + Math.Sin(x.DayOfYear * 0.2) * 10 + rand.NextDouble() * 4;
            double err = 2 + rand.NextDouble() * 2;
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
        int n = 60;
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
        int n = 120;
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
        int n = 80;
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
        int n = 40;
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
        int n = 12;
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
        int n = 10;
        var xs = Enumerable.Range(0, n).Select(i => start.AddDays(i)).ToArray();
        var rand = new Random(321);
        var stackPoints = xs.Select((x, i) =>
        {
            double a = rand.NextDouble() * 1 + 0.2;
            double b = rand.NextDouble() * 1 + 0.2;
            double c = rand.NextDouble() * 1 + 0.2;
            double sum = a + b + c;
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
        int n = 40;
        double price = 50;
        var ohlc = new OhlcPoint[n];
        var errs = new ErrorBarPoint[n];
        for (int i = 0; i < n; i++)
        {
            double open = price;
            double change = (rand.NextDouble() - 0.5) * 3;
            double close = open + change;
            double high = System.Math.Max(open, close) + rand.NextDouble() * 1.5;
            double low = System.Math.Min(open, close) - rand.NextDouble() * 1.5;
            price = close;
            double xOa = start.AddDays(i).ToOADate();
            ohlc[i] = new OhlcPoint(xOa, open, high, low, close);
            double central = (open + close) * 0.5;
            double err = 0.5 + rand.NextDouble();
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
        // Dense line
        var xs = Enumerable.Range(0, 300).Select(i => start.AddMinutes(i * 15)).ToArray();
        var line = xs.Select((x,i) => new PointD(x.ToOADate(), 50 + Math.Sin(i * 0.15)*10 + Math.Sin(i*0.03)*5)).ToArray();
        m.AddSeries(new LineSeries(line) { Title = "Line A", StrokeThickness = 1.4 });
        // Area
        var area = xs.Where((_,i)=> i%3==0).Select((x,i) => new PointD(x.ToOADate(), 40 + Math.Cos(i*0.18)*6)).ToArray();
        m.AddSeries(new AreaSeries(area) { Title = "Area B", Baseline = 35, FillOpacity = 0.35 });
        // Scatter sparse
        var scatter = xs.Where((_,i)=> i%20==0).Select((x,i) => new PointD(x.ToOADate(), 55 + Math.Sin(i*0.9)*4)).ToArray();
        m.AddSeries(new ScatterSeries(scatter) { Title = "Scatter C", MarkerSize = 4 });
        // Bars daily
        var dayXs = Enumerable.Range(0, 4).Select(i => start.AddDays(i)).ToArray();
        var bars = dayXs.Select((x,i) => new BarPoint(x.ToOADate(), 30 + i*2 + Math.Sin(i)*3)).ToArray();
        m.AddSeries(new BarSeries(bars) { Title = "Bars D", FillOpacity = 0.6 });
        m.UpdateScales(800, 400);
        // Note: ESC to unlock tooltip; click to lock/unlock already enabled by behavior.
        return m;
    }
}
