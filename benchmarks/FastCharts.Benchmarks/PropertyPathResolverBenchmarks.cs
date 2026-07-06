using BenchmarkDotNet.Attributes;

using FastCharts.Core.DataBinding;

namespace FastCharts.Benchmarks
{
    /// <summary>
    /// Data-binding hot path: resolving X/Y property paths per item per refresh.
    /// Compares the compiled-delegate cache (v1.1 default) against raw reflection.
    /// </summary>
    [MemoryDiagnoser]
    public class PropertyPathResolverBenchmarks
    {
        private sealed class Sample
        {
            public double Value { get; set; }

            public Sample? Nested { get; set; }
        }

        private readonly Sample _item = new Sample { Value = 42, Nested = new Sample { Value = 7 } };

        [Benchmark(Baseline = true, Description = "Reflection resolver, simple path x1000")]
        public object? Reflection_Simple()
        {
            object? last = null;
            for (var i = 0; i < 1000; i++)
            {
                last = ReflectionPropertyPathResolver.Instance.GetValue(_item, "Value");
            }

            return last;
        }

        [Benchmark(Description = "Cached resolver, simple path x1000")]
        public object? Cached_Simple()
        {
            object? last = null;
            for (var i = 0; i < 1000; i++)
            {
                last = CachedPropertyPathResolver.Instance.GetValue(_item, "Value");
            }

            return last;
        }

        [Benchmark(Description = "Reflection resolver, nested path x1000")]
        public object? Reflection_Nested()
        {
            object? last = null;
            for (var i = 0; i < 1000; i++)
            {
                last = ReflectionPropertyPathResolver.Instance.GetValue(_item, "Nested.Value");
            }

            return last;
        }

        [Benchmark(Description = "Cached resolver, nested path x1000")]
        public object? Cached_Nested()
        {
            object? last = null;
            for (var i = 0; i < 1000; i++)
            {
                last = CachedPropertyPathResolver.Instance.GetValue(_item, "Nested.Value");
            }

            return last;
        }
    }
}
