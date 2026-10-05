namespace FastCharts.Benchmarks
{
    /// <summary>
    /// Bound item used by <see cref="PropertyPathResolverBenchmarks"/>.
    /// </summary>
    public sealed class ResolverSample
    {
        public double Value { get; set; }

        public ResolverSample? Nested { get; set; }
    }
}
