using System;
using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Resampling;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Line series with automatic LTTB resampling for optimal performance
    /// </summary>
    public class LineSeries : SeriesBase, ISeriesRangeProvider
    {
        private readonly List<PointD> _data;
        private IResampler? _resampler;
        private bool _enableAutoResampling = true;
        private int _autoResampleThreshold = 2000; // Start resampling above 2K points
        private IReadOnlyList<PointD>? _cachedResampledData;
        private int _lastViewportPixelWidth = -1;
        private int _lastResampledCount = -1;

        /// <summary>
        /// Raw data points. Mutations through this list are visible immediately;
        /// call <see cref="InvalidateCache"/> after external bulk edits so resampling is recomputed.
        /// </summary>
        public IList<PointD> Data => _data;

        public override bool IsEmpty => _data.Count == 0;

        public LineSeries()
        {
            _data = new List<PointD>();
            StrokeThickness = 1.0;
            _resampler = new LttbResampler(); // Default to LTTB
        }

        public LineSeries(IEnumerable<PointD> points)
        {
            _data = new List<PointD>(points);
            StrokeThickness = 1.0;
            _resampler = new LttbResampler(); // Default to LTTB
        }

        /// <summary>
        /// Creates a line series from X/Y pairs — e.g. a <c>Dictionary&lt;double, double&gt;</c>.
        /// Points are sorted by X so the curve renders correctly regardless of source ordering.
        /// </summary>
        /// <param name="points">X/Y pairs (key = X, value = Y)</param>
        public LineSeries(IEnumerable<KeyValuePair<double, double>> points)
        {
            _data = new List<PointD>();
            if (points != null)
            {
                foreach (var pair in points)
                {
                    _data.Add(new PointD(pair.Key, pair.Value));
                }

                _data.Sort((a, b) => a.X.CompareTo(b.X));
            }

            StrokeThickness = 1.0;
            _resampler = new LttbResampler(); // Default to LTTB
        }

        /// <summary>
        /// Creates a line series from Y values only; X becomes the 0-based index.
        /// </summary>
        /// <param name="values">Y values</param>
        public LineSeries(IEnumerable<double> values)
        {
            _data = new List<PointD>();
            if (values != null)
            {
                var i = 0;
                foreach (var value in values)
                {
                    _data.Add(new PointD(i++, value));
                }
            }

            StrokeThickness = 1.0;
            _resampler = new LttbResampler(); // Default to LTTB
        }

        /// <summary>
        /// Direct access to the backing list for derived classes (no copy).
        /// </summary>
        protected List<PointD> DataCore => _data;

        /// <summary>
        /// Gets or sets whether markers are drawn on data points
        /// </summary>
        public bool ShowMarkers { get; set; }

        /// <summary>
        /// Gets or sets the marker size in pixels (when <see cref="ShowMarkers"/> is true)
        /// </summary>
        public double MarkerSize { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the marker shape (when <see cref="ShowMarkers"/> is true)
        /// </summary>
        public MarkerShape MarkerShape { get; set; } = MarkerShape.Circle;

        /// <summary>
        /// Gets or sets the line interpolation mode. <see cref="LineSmoothing.Spline"/>
        /// renders a smooth curve through the points.
        /// </summary>
        public LineSmoothing Smoothing { get; set; } = LineSmoothing.None;

        /// <summary>
        /// Gets or sets the resampling algorithm used for large datasets
        /// </summary>
        public IResampler? Resampler
        {
            get => _resampler;
            set
            {
                _resampler = value;
                InvalidateCache();
            }
        }

        /// <summary>
        /// Gets or sets whether automatic resampling is enabled
        /// When true, large datasets are automatically resampled for better performance
        /// </summary>
        public bool EnableAutoResampling
        {
            get => _enableAutoResampling;
            set
            {
                _enableAutoResampling = value;
                InvalidateCache();
            }
        }

        /// <summary>
        /// Gets or sets the point count threshold above which auto-resampling kicks in
        /// </summary>
        public int AutoResampleThreshold
        {
            get => _autoResampleThreshold;
            set
            {
                _autoResampleThreshold = Math.Max(100, value);
                InvalidateCache();
            }
        }

        /// <summary>
        /// Gets the effective data for rendering, with resampling applied if needed.
        /// This is what renderers should use instead of raw Data.
        /// Fast paths return the backing list directly (no per-frame allocation).
        /// </summary>
        /// <param name="viewportPixelWidth">Available pixel width for rendering</param>
        /// <returns>Optimized data for rendering</returns>
        public virtual IReadOnlyList<PointD> GetRenderData(int viewportPixelWidth = 800)
        {
            // If resampling is disabled, no resampler, or data is small: render raw data (zero copy)
            if (!_enableAutoResampling || _resampler == null || _data.Count <= _autoResampleThreshold)
            {
                return _data;
            }

            // Cache is valid only for the same viewport width and unchanged point count
            if (_cachedResampledData != null &&
                _lastViewportPixelWidth == viewportPixelWidth &&
                _lastResampledCount == _data.Count)
            {
                return _cachedResampledData;
            }

            var targetPointCount = CalculateOptimalPointCount(viewportPixelWidth);

            _cachedResampledData = _resampler.Resample(_data, targetPointCount);
            _lastViewportPixelWidth = viewportPixelWidth;
            _lastResampledCount = _data.Count;

            return _cachedResampledData;
        }

        /// <summary>
        /// Calculates optimal point count based on viewport pixel width
        /// Strategy: ~2 points per pixel for smooth curves, capped at reasonable limits
        /// </summary>
        protected int CalculateOptimalPointCount(int viewportPixelWidth)
        {
            // Base calculation: 2 points per pixel for smooth rendering
            var baseTarget = viewportPixelWidth * 2;

            // Apply reasonable bounds
            var minPoints = Math.Min(100, _data.Count);
            var maxPoints = Math.Min(5000, _data.Count); // Cap at 5K for performance

            return Math.Max(minPoints, Math.Min(maxPoints, baseTarget));
        }

        /// <summary>
        /// Monotonic version incremented on every data mutation through the series API.
        /// Renderers use it to cache derived geometry (paths, pixel buffers) safely.
        /// Direct edits through <see cref="Data"/> bypass it — call <see cref="InvalidateCache"/> after those.
        /// </summary>
        public int DataVersion { get; private set; }

        /// <summary>
        /// Invalidates the resampling cache, forcing recalculation on next render
        /// </summary>
        public void InvalidateCache()
        {
            _cachedResampledData = null;
            _lastViewportPixelWidth = -1;
            _lastResampledCount = -1;
            DataVersion++;
        }

        /// <summary>
        /// Adds a point to the series and invalidates cache
        /// </summary>
        public void AddPoint(PointD point)
        {
            _data.Add(point);
            InvalidateCache();
        }

        /// <summary>
        /// Adds multiple points to the series and invalidates cache
        /// </summary>
        public void AddPoints(IEnumerable<PointD> points)
        {
            _data.AddRange(points);
            InvalidateCache();
        }

        /// <summary>
        /// Replaces the whole series content in a single operation and invalidates cache.
        /// More efficient than Clear + AddPoints for data-binding scenarios.
        /// </summary>
        public void ReplacePoints(IEnumerable<PointD> points)
        {
            _data.Clear();
            _data.AddRange(points);
            InvalidateCache();
        }

        /// <summary>
        /// Clears all data and invalidates cache
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            InvalidateCache();
        }

        public virtual FRange GetXRange()
        {
            if (_data.Count == 0)
            {
                return new FRange(0, 0);
            }
            var (min, max) = DataHelper.GetMinMax(_data, p => p.X);
            return new FRange(min, max);
        }

        public virtual FRange GetYRange()
        {
            if (_data.Count == 0)
            {
                return new FRange(0, 0);
            }
            var (min, max) = DataHelper.GetMinMax(_data, p => p.Y);
            return new FRange(min, max);
        }

        bool ISeriesRangeProvider.TryGetRanges(out FRange xRange, out FRange yRange)
        {
            if (IsEmpty)
            {
                xRange = default;
                yRange = default;
                return false;
            }
            xRange = GetXRange();
            yRange = GetYRange();
            return true;
        }

        /// <summary>
        /// Gets resampling statistics if available
        /// </summary>
        public ResamplingStats? GetLastResamplingStats()
        {
            if (_resampler is LttbResampler lttb && _cachedResampledData != null)
            {
                return lttb.GetLastStats(_data.Count, _cachedResampledData.Count);
            }
            return null;
        }

        /// <summary>
        /// Creates a high-performance LineSeries optimized for large datasets
        /// </summary>
        /// <param name="points">Data points</param>
        /// <param name="title">Series title</param>
        /// <param name="autoResampleThreshold">Point count above which resampling kicks in</param>
        /// <returns>Optimized LineSeries</returns>
        public static LineSeries CreateHighPerformance(IEnumerable<PointD> points, string? title = null, int autoResampleThreshold = 1000)
        {
            var series = new LineSeries(points)
            {
                Title = title,
                AutoResampleThreshold = autoResampleThreshold,
                EnableAutoResampling = true,
                Resampler = new LttbResampler()
            };
            return series;
        }
    }
}
