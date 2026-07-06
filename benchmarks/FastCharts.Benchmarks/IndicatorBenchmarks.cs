using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

using FastCharts.Core.Finance;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Benchmarks
{
    /// <summary>
    /// Indicator computation cost over realistic daily/tick history sizes.
    /// SMA/EMA are O(n) sliding-window; Bollinger recomputes each window (O(n·p)).
    /// </summary>
    [MemoryDiagnoser]
    public class IndicatorBenchmarks
    {
        private IReadOnlyList<PointD> _prices = Array.Empty<PointD>();

        [Params(10_000, 100_000)]
        public int PointCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var rng = new Random(42);
            var prices = new List<PointD>(PointCount);
            var price = 100.0;
            for (var i = 0; i < PointCount; i++)
            {
                price += (rng.NextDouble() - 0.5) * 2;
                prices.Add(new PointD(i, price));
            }

            _prices = prices;
        }

        [Benchmark(Description = "SMA 20")]
        public LineSeries Sma()
        {
            return Indicators.Sma(_prices, 20);
        }

        [Benchmark(Description = "EMA 20")]
        public LineSeries Ema()
        {
            return Indicators.Ema(_prices, 20);
        }

        [Benchmark(Description = "Bollinger 20 ±2σ")]
        public BollingerBandsResult Bollinger()
        {
            return Indicators.BollingerBands(_prices, 20, 2);
        }
    }
}
