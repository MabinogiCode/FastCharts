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