using System;
using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Axes;

namespace DemoApp.Net48.ViewModels
{

    public sealed class MainViewModel
    {
        public ChartModel Chart { get; }

        public MainViewModel()
        {
            Chart = new ChartModel();

            int n = 201;
            var start = DateTime.Today.AddDays(-14);
            var xs = Enumerable.Range(0, n).Select(i => start.AddHours(i * 1.5)).ToArray();
            Chart.Theme = new DarkTheme();

            // Switch X axis to DateTime
            var dtAxis = new DateTimeAxis();
            dtAxis.SetVisibleRange(start, DateTime.Today.AddDays(1));
            Chart.ReplaceXAxis(dtAxis);

            var areaPts = xs.Select(x => new PointD(x.ToOADate(), Math.Max(0, Math.Sin(x.Ticks / 1e10)))).ToArray();
            Chart.AddSeries(new AreaSeries(areaPts) { Title = "Area: daily pattern", Baseline = 0.0, FillOpacity = 0.35 });

            var linePts = xs.Select(x => new PointD(x.ToOADate(), Math.Cos(x.Ticks / 1e10))).ToArray();
            Chart.AddSeries(new LineSeries(linePts) { Title = "Line: trend", StrokeThickness = 1.8 });

            var stepPts = xs.Select(x => new PointD(x.ToOADate(), Math.Sign(Math.Sin(x.Ticks / 1e10)))).ToArray();
            Chart.AddSeries(new StepLineSeries(stepPts) { Title = "Step: regime", StrokeThickness = 1.5, Mode = StepMode.Before });

            var bandPts = xs.Select(x =>
            {
                var t = Math.Sin(x.Ticks / 1e10);
                return new BandPoint(x.ToOADate(), t - 0.3, t + 0.3);
            }).ToArray();
            Chart.AddSeries(new BandSeries(bandPts) { Title = "Band: conf", FillOpacity = 0.2, StrokeThickness = 1.0 });

            var scatterPts = xs.Where((x, i) => i % 16 == 0)
                .Select(x => new PointD(x.ToOADate(), Math.Sin(x.Ticks / 1e10) + (Math.Sin(3 * (x.Ticks / 1e10)) * 0.05)))
                .ToArray();
            Chart.AddSeries(new ScatterSeries(scatterPts) { Title = "Scatter: samples", MarkerSize = 4.0, MarkerShape = MarkerShape.Circle });

            Chart.UpdateScales(800, 400); // nominal size; real renderer will update on arrange
        }
    }
}
