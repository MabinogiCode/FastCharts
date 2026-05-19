using System;

using BenchmarkDotNet.Attributes;

using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;

namespace FastCharts.Benchmarks;

/// <summary>
/// Measures a full render of a single line series to a bitmap, which is the
/// path that backs the "60 FPS" target in RoadMap section 7.
/// </summary>
[MemoryDiagnoser]
public class RenderBenchmarks
{
    private readonly SkiaChartRenderer _renderer = new();
    private ChartModel _model = null!;

    /// <summary>Number of points in the rendered series.</summary>
    [Params(1_000, 100_000)]
    public int PointCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var points = new PointD[PointCount];
        for (var i = 0; i < PointCount; i++)
        {
            points[i] = new PointD(i, Math.Sin(i * 0.01) * 100.0);
        }

        _model = new ChartModel();
        _model.AddSeries(new LineSeries(points) { Title = "Benchmark" });
    }

    [Benchmark]
    public int RenderToBitmap()
    {
        using var bitmap = _renderer.RenderToBitmap(_model, 1280, 720);
        return bitmap.Height;
    }
}
