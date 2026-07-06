using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

using FastCharts.Core.Primitives;
using FastCharts.Core.Resampling;
using FastCharts.Core.Series;

namespace FastCharts.Benchmarks
{
    /// <summary>
    /// Roadmap targets (RoadMap.md §7): 100K-1M points must render under 100ms
    /// with decimation. LTTB is the dominant cost, measured here.
    /// </summary>
    [MemoryDiagnoser]
    public class ResamplingBenchmarks
    {
        private IReadOnlyList<PointD> _points = Array.Empty<PointD>();
        private LineSeries _series = new LineSeries();
        private readonly LttbResampler _resampler = new LttbResampler();

        [Params(10_000, 100_000, 1_000_000)]
        public int PointCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var rng = new Random(42);
            var points = new List<PointD>(PointCount);
            for (var i = 0; i < PointCount; i++)
            {
                points.Add(new PointD(i, Math.Sin(i * 0.001) * 100 + rng.NextDouble() * 5));
            }

            _points = points;
            _series = new LineSeries(points);
        }

        [Benchmark(Description = "LTTB resample to 2000 points")]
        public IReadOnlyList<PointD> LttbResample()
        {
            return _resampler.Resample(_points, 2000);
        }

        [Benchmark(Description = "GetRenderData cold (cache invalidated)")]
        public IReadOnlyList<PointD> GetRenderDataCold()
        {
            _series.InvalidateCache();
            return _series.GetRenderData(1000);
        }

        [Benchmark(Description = "GetRenderData warm (cache hit) — per-frame cost")]
        public IReadOnlyList<PointD> GetRenderDataWarm()
        {
            return _series.GetRenderData(1000);
        }
    }
}
