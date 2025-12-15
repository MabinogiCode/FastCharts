using System;

namespace FastCharts.Core.Abstractions
{
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