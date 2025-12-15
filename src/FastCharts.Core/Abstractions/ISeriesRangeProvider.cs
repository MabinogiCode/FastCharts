using FastCharts.Core.Primitives;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Provides data range information (X and Y) for a series so that generic range aggregation logic
    /// (e.g., auto-fit) does not need to use reflection or large switch statements.
    /// </summary>
    public interface ISeriesRangeProvider
    {
        /// <summary>
        /// Attempts to get the X and Y data ranges for this series.
        /// Returns false when the series is empty or has no meaningful range.
        /// </summary>
        bool TryGetRanges(out FRange xRange, out FRange yRange);
    }
}
