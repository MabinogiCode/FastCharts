using FastCharts.Core.Primitives;
using System;
using System.Collections.Generic;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Interface for series that support efficient streaming operations
    /// Enables real-time data updates with rolling window support
    /// </summary>
    public interface IStreamingSeries
    {
        /// <summary>
        /// Gets or sets the maximum number of points to keep in the series
        /// When this limit is reached, old points are automatically removed (FIFO)
        /// </summary>
        int? MaxPointCount { get; set; }

        /// <summary>
        /// Gets or sets the rolling window duration
        /// Points older than this duration are automatically removed
        /// </summary>
        TimeSpan? RollingWindowDuration { get; set; }

        /// <summary>
        /// Appends a new data point to the series
        /// Automatically manages window limits and triggers necessary updates
        /// </summary>
        /// <param name="point">Point to append</param>
        void AppendPoint(PointD point);

        /// <summary>
        /// Appends multiple data points efficiently
        /// </summary>
        /// <param name="points">Points to append</param>
        void AppendPoints(IEnumerable<PointD> points);

        /// <summary>
        /// Trims old data points based on current window settings
        /// Called automatically by append operations, but can be called manually
        /// </summary>
        void TrimToWindow();

        /// <summary>
        /// Gets the current point count in the series
        /// </summary>
        int PointCount { get; }

        /// <summary>
        /// Gets the age of the oldest point in the series
        /// </summary>
        TimeSpan? OldestPointAge { get; }

        /// <summary>
        /// Event raised when points are added to the series
        /// </summary>
        event EventHandler<StreamingDataEventArgs>? PointsAdded;

        /// <summary>
        /// Event raised when points are removed from the series (due to window limits)
        /// </summary>
        event EventHandler<StreamingDataEventArgs>? PointsRemoved;
    }

    /// <summary>
    /// Event args for streaming data changes
    /// </summary>
    public class StreamingDataEventArgs : EventArgs
    {
        public StreamingDataEventArgs(int pointCount, int pointsAdded = 0, int pointsRemoved = 0)
        {
            PointCount = pointCount;
            PointsAdded = pointsAdded;
            PointsRemoved = pointsRemoved;
        }

        /// <summary>
        /// Total number of points in the series after the operation
        /// </summary>
        public int PointCount { get; }

        /// <summary>
        /// Number of points that were added in this operation
        /// </summary>
        public int PointsAdded { get; }

        /// <summary>
        /// Number of points that were removed in this operation
        /// </summary>
        public int PointsRemoved { get; }

        /// <summary>
        /// Whether the data range might have changed significantly
        /// </summary>
        public bool RangeChanged => PointsAdded > 0 || PointsRemoved > 0;
    }
}