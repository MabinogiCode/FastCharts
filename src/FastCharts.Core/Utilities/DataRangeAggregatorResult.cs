namespace FastCharts.Core.Utilities
{
    /// <summary>
    /// Represents the aggregated data ranges result for multiple series.
    /// </summary>
    internal readonly struct DataRangeAggregatorResult
    {
        /// <summary>
        /// Gets a value indicating whether any series had X data.
        /// </summary>
        public bool HasX { get; }

        /// <summary>
        /// Gets a value indicating whether any series used the primary Y axis.
        /// </summary>
        public bool HasPrimary { get; }

        /// <summary>
        /// Gets a value indicating whether any series used the secondary Y axis.
        /// </summary>
        public bool HasSecondary { get; }

        /// <summary>
        /// Gets the minimum X value across all series.
        /// </summary>
        public double XMin { get; }

        /// <summary>
        /// Gets the maximum X value across all series.
        /// </summary>
        public double XMax { get; }

        /// <summary>
        /// Gets the minimum Y value for primary axis series.
        /// </summary>
        public double PrimaryYMin { get; }

        /// <summary>
        /// Gets the maximum Y value for primary axis series.
        /// </summary>
        public double PrimaryYMax { get; }

        /// <summary>
        /// Gets the minimum Y value for secondary axis series.
        /// </summary>
        public double SecondaryYMin { get; }

        /// <summary>
        /// Gets the maximum Y value for secondary axis series.
        /// </summary>
        public double SecondaryYMax { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRangeAggregatorResult"/> struct.
        /// </summary>
        /// <param name="hasX">Whether any series had X data.</param>
        /// <param name="hasPrimary">Whether any series used the primary Y axis.</param>
        /// <param name="hasSecondary">Whether any series used the secondary Y axis.</param>
        /// <param name="xMin">The minimum X value.</param>
        /// <param name="xMax">The maximum X value.</param>
        /// <param name="yMin">The minimum Y value for primary axis.</param>
        /// <param name="yMax">The maximum Y value for primary axis.</param>
        /// <param name="y2Min">The minimum Y value for secondary axis.</param>
        /// <param name="y2Max">The maximum Y value for secondary axis.</param>
        public DataRangeAggregatorResult(bool hasX, bool hasPrimary, bool hasSecondary, double xMin, double xMax, double yMin, double yMax, double y2Min, double y2Max)
        {
            HasX = hasX;
            HasPrimary = hasPrimary;
            HasSecondary = hasSecondary;
            XMin = xMin;
            XMax = xMax;
            PrimaryYMin = yMin;
            PrimaryYMax = yMax;
            SecondaryYMin = y2Min;
            SecondaryYMax = y2Max;
        }
    }
}