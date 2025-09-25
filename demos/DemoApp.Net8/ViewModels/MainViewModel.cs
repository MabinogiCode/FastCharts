using System;
using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;

namespace DemoApp.Net8.ViewModels;

public sealed class MainViewModel
{
    public ChartModel Chart { get; }

    public MainViewModel()
    {
        Chart = new ChartModel();
        Chart.Theme = new LightTheme();

        int n = 201;
        var xs = Enumerable.Range(0, n).Select(i => i * 0.1).ToArray();

        // 1) Area (positive part of sin)
        var areaPts = xs.Select(x => new PointD(x, Math.Max(0, Math.Sin(x)))).ToArray();
        var area = new AreaSeries(areaPts) { Title = "Area: max(0, sin x)", Baseline = 0.0, FillOpacity = 0.35 };
        Chart.AddSeries(area);

        // 2) Line (cos)
        var linePts = xs.Select(x => new PointD(x, Math.Cos(x))).ToArray();
        var line = new LineSeries(linePts) { Title = "Line: cos x", StrokeThickness = 1.8 };
        Chart.AddSeries(line);

        // 3) Step line (square wave from sin sign)
        var stepPts = xs.Select(x => new PointD(x, Math.Sign(Math.Sin(x)))).ToArray();
        var step = new StepLineSeries(stepPts) { Title = "Step: sign(sin x)", StrokeThickness = 1.5, Mode = StepMode.Before };
        Chart.AddSeries(step);

        // 4) Band (sin ± 0.3)
        var bandPts = xs.Select(x =>
        {
            var y = Math.Sin(x);
            return new BandPoint(x, y - 0.3, y + 0.3);
        }).ToArray();
        var band = new BandSeries(bandPts) { Title = "Band: sin x ± 0.3", FillOpacity = 0.2, StrokeThickness = 1.0 };
        Chart.AddSeries(band);

        // 5) Scatter (decimated noisy sin)
        var scatterPts = xs.Where((x, i) => i % 8 == 0)
            .Select(x => new PointD(x, Math.Sin(x) + (Math.Sin(3 * x) * 0.05)))
            .ToArray();
        var scatter = new ScatterSeries(scatterPts) { Title = "Scatter: samples", MarkerSize = 4.0, MarkerShape = MarkerShape.Circle };
        Chart.AddSeries(scatter);

        // Nominal size; renderer will update on arrange
        Chart.UpdateScales(800, 400);
    }
}
