using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Resampling;
using System;
using System.Collections.Generic;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// High-performance streaming line series with rolling window support
    /// Optimized for real-time applications with thousands of points per second.
    /// Data is stored in the base <see cref="LineSeries"/> list, so rendering,
    /// resampling and range calculation all see the streamed points.
    /// </summary>
    public class StreamingLineSeries : LineSeries, IStreamingSeries
    {
        private int? _maxPointCount;
        private TimeSpan? _rollingWindowDuration;
        private DateTime _referenceTime = DateTime.UtcNow;

        /// <summary>
        /// Creates a new streaming line series
        /// </summary>
        /// <param name="maxPointCount">Maximum points to keep (null = unlimited)</param>
        /// <param name="rollingWindow">Rolling window duration (null = no time-based trimming)</param>
        public StreamingLineSeries(int? maxPointCount = null, TimeSpan? rollingWindow = null)
        {
            _maxPointCount = maxPointCount;
            _rollingWindowDuration = rollingWindow;

            // Optimize for streaming performance
            EnableAutoResampling = true;
            AutoResampleThreshold = 1000; // Start resampling at 1K points
            Resampler = new LttbResampler();
        }

        /// <summary>
        /// Creates a streaming series from existing data
        /// </summary>
        /// <param name="initialData">Initial data points</param>
        /// <param name="maxPointCount">Maximum points to keep</param>
        /// <param name="rollingWindow">Rolling window duration</param>
        public StreamingLineSeries(IEnumerable<PointD> initialData, int? maxPointCount = null, TimeSpan? rollingWindow = null)
            : this(maxPointCount, rollingWindow)
        {
            DataCore.AddRange(initialData);

            // Set reference time based on latest data point if available
            if (DataCore.Count > 0)
            {
                var maxX = double.NegativeInfinity;
                for (var i = 0; i < DataCore.Count; i++)
                {
                    if (DataCore[i].X > maxX)
                    {
                        maxX = DataCore[i].X;
                    }
                }

                _referenceTime = FromOADateSafe(maxX) ?? _referenceTime;
            }

            TrimToWindow(); // Apply window limits to initial data
            InvalidateCache();
        }

        /// <summary>
        /// Gets or sets the maximum number of points to keep in the series
        /// </summary>
        public int? MaxPointCount
        {
            get => _maxPointCount;
            set
            {
                _maxPointCount = value;
                if (_maxPointCount.HasValue && _maxPointCount.Value > 0)
                {
                    TrimToWindow();
                }
            }
        }

        /// <summary>
        /// Gets or sets the rolling window duration
        /// </summary>
        public TimeSpan? RollingWindowDuration
        {
            get => _rollingWindowDuration;
            set
            {
                _rollingWindowDuration = value;
                if (_rollingWindowDuration.HasValue)
                {
                    TrimToWindow();
                }
            }
        }

        /// <summary>
        /// Gets the current point count
        /// </summary>
        public int PointCount => DataCore.Count;

        /// <summary>
        /// Gets the age of the oldest point in the series
        /// </summary>
        public TimeSpan? OldestPointAge
        {
            get
            {
                if (DataCore.Count == 0)
                {
                    return null;
                }

                var minX = double.PositiveInfinity;
                for (var i = 0; i < DataCore.Count; i++)
                {
                    if (DataCore[i].X < minX)
                    {
                        minX = DataCore[i].X;
                    }
                }

                var oldestTime = FromOADateSafe(minX);
                return oldestTime.HasValue ? DateTime.UtcNow - oldestTime.Value : (TimeSpan?)null;
            }
        }

        /// <summary>
        /// Event raised when points are added to the series
        /// </summary>
        public event EventHandler<StreamingDataEventArgs>? PointsAdded;

        /// <summary>
        /// Event raised when points are removed from the series
        /// </summary>
        public event EventHandler<StreamingDataEventArgs>? PointsRemoved;

        /// <summary>
        /// Appends a new data point efficiently (no intermediate allocations)
        /// </summary>
        /// <param name="point">Point to append</param>
        public void AppendPoint(PointD point)
        {
            DataCore.Add(point);

            var time = FromOADateSafe(point.X);
            if (time.HasValue)
            {
                _referenceTime = time.Value;
            }

            FinishAppend(addedCount: 1);
        }

        /// <summary>
        /// Appends multiple data points efficiently with batched window management
        /// </summary>
        /// <param name="points">Points to append</param>
        public void AppendPoints(IEnumerable<PointD> points)
        {
            if (points == null)
            {
                return;
            }

            var added = 0;
            var maxX = double.NegativeInfinity;

            foreach (var point in points)
            {
                DataCore.Add(point);
                added++;
                if (point.X > maxX)
                {
                    maxX = point.X;
                }
            }

            if (added == 0)
            {
                return;
            }

            var latestTime = FromOADateSafe(maxX);
            if (latestTime.HasValue)
            {
                _referenceTime = latestTime.Value;
            }

            FinishAppend(added);
        }

        private void FinishAppend(int addedCount)
        {
            // Apply window limits
            var removedCount = TrimToWindowInternal();

            // Invalidate render cache
            InvalidateCache();

            // Raise events
            PointsAdded?.Invoke(this, new StreamingDataEventArgs(DataCore.Count, addedCount, 0));

            if (removedCount > 0)
            {
                PointsRemoved?.Invoke(this, new StreamingDataEventArgs(DataCore.Count, 0, removedCount));
            }
        }

        /// <summary>
        /// Trims old data points based on current window settings
        /// </summary>
        public void TrimToWindow()
        {
            var removedCount = TrimToWindowInternal();

            if (removedCount > 0)
            {
                InvalidateCache();
                PointsRemoved?.Invoke(this, new StreamingDataEventArgs(DataCore.Count, 0, removedCount));
            }
        }

        /// <summary>
        /// Internal method for trimming data with return count
        /// </summary>
        private int TrimToWindowInternal()
        {
            var initialCount = DataCore.Count;

            // Apply count-based limit
            if (_maxPointCount.HasValue && DataCore.Count > _maxPointCount.Value)
            {
                var pointsToRemove = DataCore.Count - _maxPointCount.Value;
                DataCore.RemoveRange(0, pointsToRemove);
            }

            // Apply time-based limit
            if (_rollingWindowDuration.HasValue && DataCore.Count > 0)
            {
                var cutoffTime = (_referenceTime - _rollingWindowDuration.Value).ToOADate();
                DataCore.RemoveAll(p => p.X < cutoffTime);
            }

            return initialCount - DataCore.Count;
        }

        /// <summary>
        /// Appends a real-time data point with current timestamp
        /// Convenience method for real-time scenarios
        /// </summary>
        /// <param name="yValue">Y value to append</param>
        /// <param name="timestamp">Optional timestamp (defaults to current time)</param>
        public void AppendRealTimePoint(double yValue, DateTime? timestamp = null)
        {
            var time = timestamp ?? DateTime.UtcNow;
            var point = new PointD(time.ToOADate(), yValue);
            AppendPoint(point);
        }

        /// <summary>
        /// Appends multiple real-time points with timestamps
        /// </summary>
        /// <param name="values">Y values with timestamps</param>
        public void AppendRealTimePoints(IEnumerable<(DateTime timestamp, double value)> values)
        {
            if (values == null)
            {
                return;
            }

            var added = 0;
            DateTime latest = default;

            foreach (var (timestamp, value) in values)
            {
                DataCore.Add(new PointD(timestamp.ToOADate(), value));
                added++;
                if (timestamp > latest)
                {
                    latest = timestamp;
                }
            }

            if (added == 0)
            {
                return;
            }

            _referenceTime = latest;
            FinishAppend(added);
        }

        /// <summary>
        /// Creates a streaming series optimized for real-time data
        /// </summary>
        /// <param name="maxPoints">Maximum points to keep (default: 10,000)</param>
        /// <param name="rollingWindow">Rolling window duration (default: 1 hour)</param>
        /// <param name="title">Series title</param>
        /// <returns>Configured streaming series</returns>
        public static StreamingLineSeries CreateRealTime(int maxPoints = 10_000, TimeSpan? rollingWindow = null, string? title = null)
        {
            var series = new StreamingLineSeries(maxPoints, rollingWindow ?? TimeSpan.FromHours(1))
            {
                Title = title ?? "Real-time Data",
                StrokeThickness = 1.5,
                EnableAutoResampling = true,
                AutoResampleThreshold = 2000
            };

            return series;
        }

        /// <summary>
        /// Converts an OADate X value to DateTime, returning null when the value is out of the valid OADate range.
        /// Streaming series may carry non-time X values; time-based trimming is then skipped gracefully.
        /// </summary>
        private static DateTime? FromOADateSafe(double value)
        {
            // Valid OADate range is roughly [-657435.0, 2958466.0]
            if (double.IsNaN(value) || double.IsInfinity(value) || value < -657435.0 || value > 2958465.999999)
            {
                return null;
            }

            try
            {
                return DateTime.FromOADate(value);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
