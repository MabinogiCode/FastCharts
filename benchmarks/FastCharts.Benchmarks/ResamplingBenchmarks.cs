using System;

using BenchmarkDotNet.Attributes;

using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Benchmarks;

/// <summary>
/// Measures LTTB decimation via <see cref="LineSeries.GetRenderData(int)"/>,
/// the path used to keep large datasets renderable at interactive frame rates.
/// </summary>
[MemoryDiagnoser]
public class ResamplingBenchmarks
{
    private LineSeries _series = null!;

    /// <summary>Number of source points before decimation.</summary>
    [Params(100_000, 1_000_000)]
    public int PointCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var points = new PointD[PointCount];
        for (var i = 0; i < PointCount; i++)
        {
            points[i] = new PointD(i, Math.Sin(i * 0.001) * 100.0);
        }

        _series = new LineSeries(points);
    }

    [Benchmark]
    public int LttbDecimation()
    {
        _series.InvalidateCache();
        return _series.GetRenderData(1280).Count;
    }
}
