using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Resampling;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// High-performance streaming line series with rolling-window support.
    /// Designed for real-time scenarios: append operations and the data reads
    /// used by rendering and range calculation are synchronized on a private
    /// lock, so points may be appended from a background thread while the chart
    /// renders. The series stores its points in the inherited <see cref="LineSeries"/>
    /// data list, so it is rendered like any other line series.
    /// </summary>
    public class StreamingLineSeries : LineSeries, IStreamingSeries
    {
        private readonly object _sync = new();
        private int? _maxPointCount;
        private TimeSpan? _rollingWindowDuration;
        private DateTime _referenceTime = DateTime.UtcNow;

        /// <summary>
        /// Creates a new streaming line series.
        /// </summary>
        /// <param name="maxPointCount">Maximum points to keep (null = unlimited).</param>
        /// <param name="rollingWindow">Rolling window duration (null = no time-based trimming).</param>
        public StreamingLineSeries(int? maxPointCount = null, TimeSpan? rollingWindow = null)
        {
            _maxPointCount = maxPointCount;
            _rollingWindowDuration = rollingWindow;

            EnableAutoResampling = true;
            AutoResampleThreshold = 1000;
            Resampler = new LttbResampler();
        }

        /// <summary>
        /// Creates a streaming series seeded with existing data.
        /// </summary>
        /// <param name="initialData">Initial data points.</param>
        /// <param name="maxPointCount">Maximum points to keep.</param>
        /// <param name="rollingWindow">Rolling window duration.</param>
        public StreamingLineSeries(IEnumerable<PointD> initialData, int? maxPointCount = null, TimeSpan? rollingWindow = null)
            : this(maxPointCount, rollingWindow)
        {
            if (initialData == null)
            {
                throw new ArgumentNullException(nameof(initialData));
            }

            lock (_sync)
            {
                Buffer.AddRange(initialData);
                if (Buffer.Count > 0)
                {
                    _referenceTime = DateTime.FromOADate(Buffer.Max(p => p.X));
                }

                TrimToWindowInternal();
                InvalidateCache();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of points to keep in the series.
        /// </summary>
        public int? MaxPointCount
        {
            get
            {
                lock (_sync)
                {
                    return _maxPointCount;
                }
            }

            set
            {
                lock (_sync)
                {
                    _maxPointCount = value;
                }

                TrimToWindow();
            }
        }

        /// <summary>
        /// Gets or sets the rolling window duration.
        /// </summary>
        public TimeSpan? RollingWindowDuration
        {
            get
            {
                lock (_sync)
                {
                    return _rollingWindowDuration;
                }
            }

            set
            {
                lock (_sync)
                {
                    _rollingWindowDuration = value;
                }

                TrimToWindow();
            }
        }

        /// <summary>
        /// Gets the current point count.
        /// </summary>
        public int PointCount
        {
            get
            {
                lock (_sync)
                {
                    return Data.Count;
                }
            }
        }

        /// <summary>
        /// Gets the age of the oldest point, or null when the series is empty.
        /// </summary>
        public TimeSpan? OldestPointAge
        {
            get
            {
                lock (_sync)
                {
                    if (Buffer.Count == 0)
                    {
                        return null;
                    }

                    return DateTime.UtcNow - DateTime.FromOADate(Buffer.Min(p => p.X));
                }
            }
        }

        /// <summary>
        /// Event raised when points are appended to the series.
        /// </summary>
        public event EventHandler<StreamingDataEventArgs>? PointsAdded;

        /// <summary>
        /// Event raised when points are removed from the series by window trimming.
        /// </summary>
        public event EventHandler<StreamingDataEventArgs>? PointsRemoved;

        /// <summary>
        /// Appends a single data point.
        /// </summary>
        /// <param name="point">Point to append.</param>
        public void AppendPoint(PointD point)
        {
            AppendPoints(new[] { point });
        }

        /// <summary>
        /// Appends multiple data points with batched window management.
        /// </summary>
        /// <param name="points">Points to append.</param>
        public void AppendPoints(IEnumerable<PointD> points)
        {
            if (points == null)
            {
                return;
            }

            var pointArray = points as PointD[] ?? points.ToArray();
            if (pointArray.Length == 0)
            {
                return;
            }

            int total;
            int removedCount;

            lock (_sync)
            {
                Buffer.AddRange(pointArray);
                _referenceTime = DateTime.FromOADate(pointArray.Max(p => p.X));
                removedCount = TrimToWindowInternal();
                InvalidateCache();
                total = Data.Count;
            }

            PointsAdded?.Invoke(this, new StreamingDataEventArgs(total, pointArray.Length, 0));
            if (removedCount > 0)
            {
                PointsRemoved?.Invoke(this, new StreamingDataEventArgs(total, 0, removedCount));
            }
        }

        /// <summary>
        /// Trims old data points based on the current window settings.
        /// </summary>
        public void TrimToWindow()
        {
            int total;
            int removedCount;

            lock (_sync)
            {
                removedCount = TrimToWindowInternal();
                if (removedCount > 0)
                {
                    InvalidateCache();
                }

                total = Data.Count;
            }

            if (removedCount > 0)
            {
                PointsRemoved?.Invoke(this, new StreamingDataEventArgs(total, 0, removedCount));
            }
        }

        /// <inheritdoc />
        public override IReadOnlyList<PointD> GetRenderData(int viewportPixelWidth = 800)
        {
            lock (_sync)
            {
                return base.GetRenderData(viewportPixelWidth);
            }
        }

        /// <inheritdoc />
        public override FRange GetXRange()
        {
            lock (_sync)
            {
                return base.GetXRange();
            }
        }

        /// <inheritdoc />
        public override FRange GetYRange()
        {
            lock (_sync)
            {
                return base.GetYRange();
            }
        }

        /// <summary>
        /// Appends a real-time point timestamped with the current (or supplied) time.
        /// </summary>
        /// <param name="yValue">Y value to append.</param>
        /// <param name="timestamp">Optional timestamp (defaults to the current UTC time).</param>
        public void AppendRealTimePoint(double yValue, DateTime? timestamp = null)
        {
            var time = timestamp ?? DateTime.UtcNow;
            AppendPoint(new PointD(time.ToOADate(), yValue));
        }

        /// <summary>
        /// Appends multiple timestamped real-time points.
        /// </summary>
        /// <param name="values">Y values with timestamps.</param>
        public void AppendRealTimePoints(IEnumerable<(DateTime timestamp, double value)> values)
        {
            if (values == null)
            {
                return;
            }

            AppendPoints(values.Select(v => new PointD(v.timestamp.ToOADate(), v.value)));
        }

        /// <summary>
        /// Creates a streaming series preconfigured for real-time data.
        /// </summary>
        /// <param name="maxPoints">Maximum points to keep (default: 10,000).</param>
        /// <param name="rollingWindow">Rolling window duration (default: 1 hour).</param>
        /// <param name="title">Series title.</param>
        /// <returns>A configured streaming series.</returns>
        public static StreamingLineSeries CreateRealTime(int maxPoints = 10_000, TimeSpan? rollingWindow = null, string? title = null)
        {
            return new StreamingLineSeries(maxPoints, rollingWindow ?? TimeSpan.FromHours(1))
            {
                Title = title ?? "Real-time Data",
                StrokeThickness = 1.5,
                EnableAutoResampling = true,
                AutoResampleThreshold = 2000
            };
        }

        /// <summary>
        /// Gets the inherited point list typed as a concrete list for efficient
        /// front-trimming. LineSeries always backs its data with a list of points.
        /// </summary>
        private List<PointD> Buffer => (List<PointD>)Data;

        /// <summary>
        /// Applies window limits. The caller must hold the synchronization lock.
        /// </summary>
        /// <returns>The number of points removed.</returns>
        private int TrimToWindowInternal()
        {
            var initialCount = Buffer.Count;

            if (_maxPointCount.HasValue && Buffer.Count > _maxPointCount.Value)
            {
                Buffer.RemoveRange(0, Buffer.Count - _maxPointCount.Value);
            }

            if (_rollingWindowDuration.HasValue && Buffer.Count > 0)
            {
                var cutoff = (_referenceTime - _rollingWindowDuration.Value).ToOADate();
                Buffer.RemoveAll(p => p.X < cutoff);
            }

            return initialCount - Buffer.Count;
        }
    }
}
