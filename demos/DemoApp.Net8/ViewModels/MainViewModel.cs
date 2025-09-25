using System;
using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Axes;

namespace DemoApp.Net8.ViewModels;

public sealed class MainViewModel
{
    public ChartModel Chart { get; }

    public MainViewModel()
    {
        Chart = new ChartModel();
        Chart.Theme = new LightTheme();

        // Switch X axis to DateTime
        var dtAxis = new DateTimeAxis();
        var start = DateTime.Today.AddDays(-14);
        dtAxis.SetVisibleRange(start, DateTime.Today.AddDays(1));
        Chart.ReplaceXAxis(dtAxis);

        int n = 201;
        var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.5)).ToArray();

        // Build series with OADate X values
        var areaPts = xs.Select(x => new PointD(x.ToOADate(), Math.Max(0, Math.Sin(x.Ticks / 1e10)))).ToArray();
        var area = new AreaSeries(areaPts) { Title = "Area: daily pattern", Baseline = 0.0, FillOpacity = 0.35 };
        Chart.AddSeries(area);

        var linePts = xs.Select(x => new PointD(x.ToOADate(), Math.Cos(x.Ticks / 1e10))).ToArray();
        var line = new LineSeries(linePts) { Title = "Line: trend", StrokeThickness = 1.8 };
        Chart.AddSeries(line);

        var stepPts = xs.Select(x => new PointD(x.ToOADate(), Math.Sign(Math.Sin(x.Ticks / 1e10)))).ToArray();
        var step = new StepLineSeries(stepPts) { Title = "Step: regime", StrokeThickness = 1.5, Mode = StepMode.Before };
        Chart.AddSeries(step);

        var bandPts = xs.Select(x =>
        {
            var t = Math.Sin(x.Ticks / 1e10);
            return new BandPoint(x.ToOADate(), t - 0.3, t + 0.3);
        }).ToArray();
        var band = new BandSeries(bandPts) { Title = "Band: conf", FillOpacity = 0.2, StrokeThickness = 1.0 };
        Chart.AddSeries(band);

        var scatterPts = xs.Where((x, i) => i % 16 == 0)
            .Select(x => new PointD(x.ToOADate(), Math.Sin(x.Ticks / 1e10) + (Math.Sin(3 * (x.Ticks / 1e10)) * 0.05)))
            .ToArray();
        var scatter = new ScatterSeries(scatterPts) { Title = "Scatter: samples", MarkerSize = 4.0, MarkerShape = MarkerShape.Circle };
        Chart.AddSeries(scatter);

        // Add grouped BarSeries over daily buckets (10 buckets)
        int buckets = 10;
        var bucketXs = Enumerable.Range(0, buckets)
            .Select(i => start.AddDays(1 + i))
            .ToArray();
        var barsA = bucketXs.Select((x, i) => new BarPoint(x.ToOADate(), (Math.Sin(i * 0.6) + 1.2) * 0.6)).ToArray();
        var barsB = bucketXs.Select((x, i) => new BarPoint(x.ToOADate(), (Math.Cos(i * 0.6) + 1.2) * 0.5)).ToArray();
        var barSeriesA = new BarSeries(barsA) { Title = "Bars A", GroupCount = 2, GroupIndex = 0, FillOpacity = 0.7 };
        var barSeriesB = new BarSeries(barsB) { Title = "Bars B", GroupCount = 2, GroupIndex = 1, FillOpacity = 0.7 };
        Chart.AddSeries(barSeriesA);
        Chart.AddSeries(barSeriesB);

        Chart.UpdateScales(800, 400);
    }
}
