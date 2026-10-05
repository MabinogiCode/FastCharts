using System.Collections.Generic;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Interface for line series implementations
    /// </summary>
    public interface ILineSeries : ISeries
    {
        /// <summary>
        /// Gets the line series data as points
        /// </summary>
        IReadOnlyList<PointD> Data { get; }

        /// <summary>
        /// Gets or sets whether to show markers on data points
        /// </summary>
        bool ShowMarkers { get; set; }

        /// <summary>
        /// Gets or sets the marker size in pixels
        /// </summary>
        double MarkerSize { get; set; }
    }
}
