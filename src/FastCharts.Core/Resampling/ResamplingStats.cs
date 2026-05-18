namespace FastCharts.Core.Resampling
{
    /// <summary>
    /// Statistics about a resampling operation
    /// </summary>
    public readonly struct ResamplingStats
    {
        public ResamplingStats(int originalCount, int resampledCount, double reductionRatio, double elapsedMs)
        {
            OriginalCount = originalCount;
            ResampledCount = resampledCount;
            ReductionRatio = reductionRatio;
            ElapsedMs = elapsedMs;
        }

        /// <summary>
        /// Original number of data points
        /// </summary>
        public int OriginalCount { get; }

        /// <summary>
        /// Number of points after resampling
        /// </summary>
        public int ResampledCount { get; }

        /// <summary>
        /// Reduction ratio (0.0 to 1.0, where 0.1 = 90% reduction)
        /// </summary>
        public double ReductionRatio { get; }

        /// <summary>
        /// Time taken for resampling operation in milliseconds
        /// </summary>
        public double ElapsedMs { get; }

        /// <summary>
        /// Percentage of data reduction (0-100%)
        /// </summary>
        public double ReductionPercentage => (1.0 - ReductionRatio) * 100.0;

        public override string ToString()
        {
            return $"Resampled {OriginalCount:N0} → {ResampledCount:N0} points ({ReductionPercentage:F1}% reduction) in {ElapsedMs:F2}ms";
        }
    }
}
