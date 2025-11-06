using FastCharts.Core.Primitives;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Result of data range calculation for chart series.
    /// </summary>
    public readonly struct DataRangeCalculationResult
    {
        /// <summary>
        /// Gets a value indicating whether any data was found.
        /// </summary>
        public bool HasData { get; }

        /// <summary>
        /// Gets a value indicating whether X data was found.
        /// </summary>
        public bool HasX { get; }

        /// <summary>
        /// Gets a value indicating whether primary Y axis data was found.
        /// </summary>
        public bool HasPrimary { get; }

        /// <summary>
        /// Gets a value indicating whether secondary Y axis data was found.
        /// </summary>
        public bool HasSecondary { get; }

        /// <summary>
        /// Gets the X data range.
        /// </summary>
        public FRange XRange { get; }

        /// <summary>
        /// Gets the primary Y axis data range.
        /// </summary>
        public FRange PrimaryYRange { get; }

        /// <summary>
        /// Gets the secondary Y axis data range.
        /// </summary>
        public FRange SecondaryYRange { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRangeCalculationResult"/> struct.
        /// </summary>
        public DataRangeCalculationResult(bool hasData, bool hasX, bool hasPrimary, bool hasSecondary,
            FRange xRange, FRange primaryYRange, FRange secondaryYRange)
        {
            HasData = hasData;
            HasX = hasX;
            HasPrimary = hasPrimary;
            HasSecondary = hasSecondary;
            XRange = xRange;
            PrimaryYRange = primaryYRange;
            SecondaryYRange = secondaryYRange;
        }

        /// <summary>
        /// Creates an empty result when no data is available.
        /// </summary>
        public static DataRangeCalculationResult Empty => new DataRangeCalculationResult(
            false, false, false, false,
            new FRange(0, 1), new FRange(0, 1), new FRange(0, 1));
    }
}