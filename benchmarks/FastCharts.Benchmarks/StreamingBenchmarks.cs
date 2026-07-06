using BenchmarkDotNet.Attributes;

using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Benchmarks
{
    /// <summary>
    /// Roadmap target (RoadMap.md §7): sustain 1K+ points/second with a rolling window.
    /// Measures single appends and batched appends against a bounded series.
    /// </summary>
    [MemoryDiagnoser]
    public class StreamingBenchmarks
    {
        private StreamingLineSeries _series = new StreamingLineSeries();
        private PointD[] _batch = System.Array.Empty<PointD>();
        private double _x;

        [Params(10_000)]
        public int WindowSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _series = new StreamingLineSeries(maxPointCount: WindowSize);

            // Pre-fill to steady state so trimming cost is included
            for (var i = 0; i < WindowSize; i++)
            {
                _series.AppendPoint(new PointD(i, i));
            }

            _x = WindowSize;

            _batch = new PointD[1000];
            for (var i = 0; i < _batch.Length; i++)
            {
                _batch[i] = new PointD(i, i);
            }
        }

        [Benchmark(Description = "AppendPoint x1000 (steady state, FIFO trim)")]
        public int AppendPoints_OneByOne()
        {
            for (var i = 0; i < 1000; i++)
            {
                _series.AppendPoint(new PointD(_x++, i));
            }

            return _series.PointCount;
        }

        [Benchmark(Description = "AppendPoints batch of 1000")]
        public int AppendPoints_Batch()
        {
            _series.AppendPoints(_batch);
            return _series.PointCount;
        }
    }
}
