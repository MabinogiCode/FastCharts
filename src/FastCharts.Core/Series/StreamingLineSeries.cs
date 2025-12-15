using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Resampling;
using FastCharts.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// High-performance streaming line series with rolling window support
    /// Optimized for real-time applications with thousands of points per second
    /// </summary>
    public class StreamingLineSeries : LineSeries, IStreamingSeries
    {
        private readonly List<PointD> _streamingData;
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
            _streamingData = new List<PointD>();
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
            _streamingData.AddRange(initialData);

            // Set reference time based on latest data point if available
            if (_streamingData.Count > 0)
            {
                var latestPoint = _streamingData.OrderByDescending(p => p.X).First();
                _referenceTime = DateTime.FromOADate(latestPoint.X);
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
        public int PointCount => _streamingData.Count;

        /// <summary>
        /// Gets the age of the oldest point in the series
        /// </summary>
        public TimeSpan? OldestPointAge
        {
            get
            {
                if (_streamingData.Count == 0)
                    return null;

                var oldestPoint = _streamingData.OrderBy(p => p.X).First();
                var oldestTime = DateTime.FromOADate(oldestPoint.X);
                return DateTime.UtcNow - oldestTime;
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
        /// Appends a new data point efficiently
        /// </summary>
        /// <param name="point">Point to append</param>
        public void AppendPoint(PointD point)
        {
            AppendPoints(new[] { point });
        }

        /// <summary>
        /// Appends multiple data points efficiently with batched window management
        /// </summary>
        /// <param name="points">Points to append</param>
        public void AppendPoints(IEnumerable<PointD> points)
        {
            if (points == null)
                return;

            var pointArray = points.ToArray();
            if (pointArray.Length == 0)
                return;

            // Add points efficiently
            _streamingData.AddRange(pointArray);

            // Update reference time based on latest point
            var latestPoint = pointArray.OrderByDescending(p => p.X).First();
            _referenceTime = DateTime.FromOADate(latestPoint.X);

            // Apply window limits
            var removedCount = TrimToWindowInternal();

            // Invalidate render cache
            InvalidateCache();

            // Raise events
            PointsAdded?.Invoke(this, new StreamingDataEventArgs(_streamingData.Count, pointArray.Length, 0));

            if (removedCount > 0)
            {
                PointsRemoved?.Invoke(this, new StreamingDataEventArgs(_streamingData.Count, 0, removedCount));
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
                PointsRemoved?.Invoke(this, new StreamingDataEventArgs(_streamingData.Count, 0, removedCount));
            }
        }

        /// <summary>
        /// Gets the streaming data as IList for compatibility
        /// This property provides access to the internal data while maintaining streaming capabilities
        /// </summary>
        public new IList<PointD> Data => _streamingData;

        /// <summary>
        /// Override to use streaming data
        /// </summary>
        public override bool IsEmpty => _streamingData.Count == 0;

        /// <summary>
        /// Gets render data optimized for streaming scenarios
        /// </summary>
        /// <param name="viewportPixelWidth">Viewport pixel width</param>
        /// <returns>Optimized render data</returns>
        public new IReadOnlyList<PointD> GetRenderData(int viewportPixelWidth = 800)
        {
            // Use base class LTTB logic but with our streaming data
            if (!EnableAutoResampling || Resampler == null)
            {
                return _streamingData.ToArray();
            }

            if (_streamingData.Count <= AutoResampleThreshold)
            {
                return _streamingData.ToArray();
            }

            // Calculate optimal point count and resample
            var targetPointCount = CalculateOptimalPointCount(viewportPixelWidth);
            return Resampler.Resample(_streamingData.ToArray(), targetPointCount);
        }

        /// <summary>
        /// Override range calculation to use streaming data
        /// </summary>
        public new FRange GetXRange()
        {
            if (_streamingData.Count == 0)
                return new FRange(0, 0);

            var (min, max) = DataHelper.GetMinMax(_streamingData, p => p.X);
            return new FRange(min, max);
        }

        /// <summary>
        /// Override range calculation to use streaming data
        /// </summary>
        public new FRange GetYRange()
        {
            if (_streamingData.Count == 0)
                return new FRange(0, 0);

            var (min, max) = DataHelper.GetMinMax(_streamingData, p => p.Y);
            return new FRange(min, max);
        }

        /// <summary>
        /// Internal method for trimming data with return count
        /// </summary>
        private int TrimToWindowInternal()
        {
            var initialCount = _streamingData.Count;

            // Apply count-based limit
            if (_maxPointCount.HasValue && _streamingData.Count > _maxPointCount.Value)
            {
                var pointsToRemove = _streamingData.Count - _maxPointCount.Value;
                _streamingData.RemoveRange(0, pointsToRemove);
            }

            // Apply time-based limit
            if (_rollingWindowDuration.HasValue && _streamingData.Count > 0)
            {
                var cutoffTime = (_referenceTime - _rollingWindowDuration.Value).ToOADate();
                var oldPointsCount = _streamingData.Count(p => p.X < cutoffTime);

                if (oldPointsCount > 0)
                {
                    _streamingData.RemoveAll(p => p.X < cutoffTime);
                }
            }

            return initialCount - _streamingData.Count;
        }

        /// <summary>
        /// Calculates optimal point count based on viewport (same as base class)
        /// </summary>
        private int CalculateOptimalPointCount(int viewportPixelWidth)
        {
            var baseTarget = viewportPixelWidth * 2;
            var minPoints = Math.Min(100, _streamingData.Count);
            var maxPoints = Math.Min(5000, _streamingData.Count);
            return Math.Max(minPoints, Math.Min(maxPoints, baseTarget));
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
            var points = values.Select(v => new PointD(v.timestamp.ToOADate(), v.value));
            AppendPoints(points);
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
    }
}