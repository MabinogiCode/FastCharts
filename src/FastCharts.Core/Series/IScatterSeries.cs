using System.Collections.Generic;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Interface for scatter series implementations
    /// </summary>
    public interface IScatterSeries : ISeries
    {
        /// <summary>
        /// Gets the scatter series data as points
        /// </summary>
        IReadOnlyList<PointD> Data { get; }

        /// <summary>
        /// Gets or sets the marker size in pixels
        /// </summary>
        double MarkerSize { get; set; }
    }
}
