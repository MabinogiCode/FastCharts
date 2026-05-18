using System.Collections.Generic;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Interface for bar series implementations
    /// </summary>
    public interface IBarSeries : ISeries
    {
        /// <summary>
        /// Gets the bar series data as bar points
        /// </summary>
        IReadOnlyList<BarPoint> Data { get; }

        /// <summary>
        /// Gets or sets the bar width (null for auto-sizing)
        /// </summary>
        double? Width { get; set; }

        /// <summary>
        /// Gets or sets the baseline Y value for bars
        /// </summary>
        double Baseline { get; set; }
    }
}
