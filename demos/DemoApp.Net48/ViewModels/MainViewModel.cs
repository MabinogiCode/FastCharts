using System;
using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;

namespace DemoApp.Net48.ViewModels
{

    public sealed class MainViewModel
    {
        public ChartModel Chart { get; }

        public MainViewModel()
        {
            Chart = new ChartModel();

            int n = 201;
            var xs = Enumerable.Range(0, n).Select(i => i * 0.1).ToArray();
            Chart.Theme = new DarkTheme();

            var areaPts = xs.Select(x => new PointD(x, Math.Max(0, Math.Sin(x)))).ToArray();
            Chart.AddSeries(new AreaSeries(areaPts) { Title = "Area: max(0, sin x)", Baseline = 0.0, FillOpacity = 0.35 });

            var linePts = xs.Select(x => new PointD(x, Math.Cos(x))).ToArray();
            Chart.AddSeries(new LineSeries(linePts) { Title = "Line: cos x", StrokeThickness = 1.8 });

            var stepPts = xs.Select(x => new PointD(x, Math.Sign(Math.Sin(x)))).ToArray();
            Chart.AddSeries(new StepLineSeries(stepPts) { Title = "Step: sign(sin x)", StrokeThickness = 1.5, Mode = StepMode.Before });

            var bandPts = xs.Select(x =>
            {
                var y = Math.Sin(x);
                return new BandPoint(x, y - 0.3, y + 0.3);
            }).ToArray();
            Chart.AddSeries(new BandSeries(bandPts) { Title = "Band: sin x ± 0.3", FillOpacity = 0.2, StrokeThickness = 1.0 });

            var scatterPts = xs.Where((x, i) => i % 8 == 0)
                .Select(x => new PointD(x, Math.Sin(x) + (Math.Sin(3 * x) * 0.05)))
                .ToArray();
            Chart.AddSeries(new ScatterSeries(scatterPts) { Title = "Scatter: samples", MarkerSize = 4.0, MarkerShape = MarkerShape.Circle });

            Chart.UpdateScales(800, 400); // nominal size; real renderer will update on arrange
        }
    }
}
