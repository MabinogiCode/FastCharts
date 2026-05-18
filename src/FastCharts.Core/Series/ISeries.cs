namespace FastCharts.Core.Series
{
    /// <summary>
    /// Base interface for all series types
    /// </summary>
    public interface ISeries
    {
        /// <summary>
        /// Gets or sets the series title
        /// </summary>
        string? Title { get; set; }

        /// <summary>
        /// Gets or sets whether the series is visible
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets whether the series has no data
        /// </summary>
        bool IsEmpty { get; }
    }
}
