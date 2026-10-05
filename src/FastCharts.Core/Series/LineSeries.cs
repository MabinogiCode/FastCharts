using System;
using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Resampling;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Line series with automatic LTTB resampling for optimal performance.
    /// Thread-safety: mutations through the series API and reads by renderers/behaviors are
    /// serialized on <see cref="SeriesBase.SyncRoot"/>, so points may be appended from a
    /// background thread while the chart renders.
    /// </summary>
    public class LineSeries : SeriesBase, ISeriesRangeProvider
    {
        private const int SortUnknown = 0;
        private const int SortedByX = 1;
        private const int NotSortedByX = 2;

        private readonly List<PointD> _data;
        private IResampler? _resampler;
        private bool _enableAutoResampling = true;
        private int _autoResampleThreshold = 2000; // Start resampling above 2K points
        private IReadOnlyList<PointD>? _cachedResampledData;
        private int _cacheVersion = -1;
        private int _cacheCount = -1;
        private int _cacheWidth = -1;
        private int _cacheStart = -1;
        private int _cacheEnd = -1;
        private int _lastSourceCount;
        private int _sortState = SortUnknown;

        /// <summary>
        /// Raw data points. Mutations through this list are visible immediately;
        /// call <see cref="InvalidateCache"/> after external bulk edits so resampling is recomputed
        /// (and lock <see cref="SeriesBase.SyncRoot"/> when editing from another thread).
        /// </summary>
        public IList<PointD> Data => _data;

        public override bool IsEmpty => _data.Count == 0;

        public LineSeries()
        {
            _data = new List<PointD>();
            StrokeThickness = 1.0;
            _resampler = new LttbResampler(); // Default to LTTB
            _sortState = SortedByX;
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
            _sortState = SortedByX;
        }

        /// <summary>
        /// Direct access to the backing list for derived classes (no copy).
        /// Derived classes mutate it under <see cref="SeriesBase.SyncRoot"/>.
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
                lock (SyncRoot)
                {
                    _resampler = value;
                    InvalidateRenderCache();
                }

                NotifyChanged();
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
                lock (SyncRoot)
                {
                    _enableAutoResampling = value;
                    InvalidateRenderCache();
                }

                NotifyChanged();
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
                lock (SyncRoot)
                {
                    _autoResampleThreshold = Math.Max(100, value);
                    InvalidateRenderCache();
                }

                NotifyChanged();
            }
        }

        /// <summary>
        /// Gets the effective data for rendering, with resampling applied if needed, over the
        /// whole series. This is what renderers should use instead of raw Data.
        /// Fast paths return the backing list directly (no per-frame allocation): enumerate the
        /// result while holding <see cref="SeriesBase.SyncRoot"/> if other threads mutate the series.
        /// </summary>
        /// <param name="viewportPixelWidth">Available pixel width for rendering</param>
        /// <returns>Optimized data for rendering</returns>
        public virtual IReadOnlyList<PointD> GetRenderData(int viewportPixelWidth = 800)
        {
            return GetRenderDataCore(viewportPixelWidth, null);
        }

        /// <summary>
        /// Gets the effective data for rendering the visible X window only. When the data is
        /// sorted by X, decimation is applied to the visible slice (plus one point on each side so
        /// segments entering/leaving the plot are drawn): zooming in reveals the full detail
        /// instead of a coarse whole-series decimation. Unsorted data falls back to the whole series.
        /// </summary>
        /// <param name="viewportPixelWidth">Available pixel width for rendering</param>
        /// <param name="visibleXRange">Visible X range of the plot</param>
        /// <returns>Optimized data for rendering</returns>
        public virtual IReadOnlyList<PointD> GetRenderData(int viewportPixelWidth, FRange visibleXRange)
        {
            return GetRenderDataCore(viewportPixelWidth, visibleXRange);
        }

        private IReadOnlyList<PointD> GetRenderDataCore(int viewportPixelWidth, FRange? visibleXRange)
        {
            lock (SyncRoot)
            {
                var count = _data.Count;

                // If resampling is disabled, no resampler, or data is small: render raw data (zero copy)
                if (!_enableAutoResampling || _resampler == null || count <= _autoResampleThreshold)
                {
                    return _data;
                }

                var start = 0;
                var end = count;
                if (visibleXRange.HasValue && TryGetIndexRange(visibleXRange.Value.Min, visibleXRange.Value.Max, out var first, out var last))
                {
                    start = Math.Max(0, first - 1);
                    end = Math.Min(count, last + 1);
                }

                // Cache is valid for the same data, viewport width and visible slice
                if (_cachedResampledData != null &&
                    _cacheVersion == DataVersion &&
                    _cacheCount == count &&
                    _cacheWidth == viewportPixelWidth &&
                    _cacheStart == start &&
                    _cacheEnd == end)
                {
                    return _cachedResampledData;
                }

                var sliceCount = end - start;
                IReadOnlyList<PointD> result;
                if (sliceCount <= _autoResampleThreshold)
                {
                    // Zoomed in enough: every visible point is drawn
                    result = _data.GetRange(start, sliceCount);
                }
                else
                {
                    var source = sliceCount == count ? (IReadOnlyList<PointD>)_data : new ListSegment<PointD>(_data, start, sliceCount);
                    result = _resampler.Resample(source, OptimalPointCount(viewportPixelWidth, sliceCount));
                }

                _cachedResampledData = result;
                _cacheVersion = DataVersion;
                _cacheCount = count;
                _cacheWidth = viewportPixelWidth;
                _cacheStart = start;
                _cacheEnd = end;
                _lastSourceCount = sliceCount;

                return result;
            }
        }

        /// <summary>
        /// Calculates optimal point count based on viewport pixel width
        /// Strategy: ~2 points per pixel for smooth curves, capped at reasonable limits
        /// </summary>
        protected int CalculateOptimalPointCount(int viewportPixelWidth)
        {
            return OptimalPointCount(viewportPixelWidth, _data.Count);
        }

        private static int OptimalPointCount(int viewportPixelWidth, int sourceCount)
        {
            // Base calculation: 2 points per pixel for smooth rendering
            var baseTarget = viewportPixelWidth * 2;

            // Apply reasonable bounds
            var minPoints = Math.Min(100, sourceCount);
            var maxPoints = Math.Min(5000, sourceCount); // Cap at 5K for performance

            return Math.Max(minPoints, Math.Min(maxPoints, baseTarget));
        }

        /// <summary>
        /// Monotonic version incremented on every data mutation through the series API.
        /// Renderers use it to cache derived geometry (paths, pixel buffers) safely.
        /// Direct edits through <see cref="Data"/> bypass it — call <see cref="InvalidateCache"/> after those.
        /// </summary>
        public int DataVersion { get; private set; }

        /// <summary>
        /// Invalidates the resampling cache, forcing recalculation on next render, and notifies
        /// hosts. Call it after editing <see cref="Data"/> directly.
        /// </summary>
        public void InvalidateCache()
        {
            lock (SyncRoot)
            {
                _sortState = SortUnknown; // direct edits may have broken the X ordering
                InvalidateRenderCache();
            }

            NotifyChanged();
        }

        /// <summary>
        /// Resets the render cache and bumps <see cref="DataVersion"/> without raising
        /// <see cref="SeriesBase.Changed"/>. Call under <see cref="SeriesBase.SyncRoot"/>, then
        /// <see cref="SeriesBase.NotifyChanged"/> once the lock is released.
        /// </summary>
        protected void InvalidateRenderCache()
        {
            _cachedResampledData = null;
            _cacheVersion = -1;
            DataVersion++;
        }

        /// <summary>
        /// Keeps the X-ordering knowledge up to date after points were appended at
        /// <paramref name="firstNewIndex"/> (O(appended) instead of re-scanning the series).
        /// Removing points never breaks the ordering. Call under <see cref="SeriesBase.SyncRoot"/>.
        /// </summary>
        protected void TrackAppendedPoints(int firstNewIndex)
        {
            if (_sortState != SortedByX)
            {
                return;
            }

            for (var i = Math.Max(firstNewIndex, 0); i < _data.Count; i++)
            {
                var ordered = i == 0 ? !double.IsNaN(_data[0].X) : IsOrdered(_data[i - 1].X, _data[i].X);
                if (!ordered)
                {
                    _sortState = NotSortedByX;
                    return;
                }
            }
        }

        /// <summary>
        /// Finds the points whose X lies in [<paramref name="minX"/>, <paramref name="maxX"/>]:
        /// indices [<paramref name="start"/>, <paramref name="end"/>) found by binary search.
        /// Returns false when the data is not sorted by X (callers then scan everything).
        /// Call under <see cref="SeriesBase.SyncRoot"/> when other threads mutate the series.
        /// </summary>
        internal bool TryGetIndexRange(double minX, double maxX, out int start, out int end)
        {
            if (!IsSortedByX() || double.IsNaN(minX) || double.IsNaN(maxX))
            {
                start = 0;
                end = 0;
                return false;
            }

            start = LowerBound(minX);
            end = Math.Max(start, UpperBound(maxX));
            return true;
        }

        private bool IsSortedByX()
        {
            if (_sortState == SortUnknown)
            {
                var sorted = _data.Count == 0 || !double.IsNaN(_data[0].X);
                for (var i = 1; sorted && i < _data.Count; i++)
                {
                    sorted = IsOrdered(_data[i - 1].X, _data[i].X);
                }

                _sortState = sorted ? SortedByX : NotSortedByX;
            }

            return _sortState == SortedByX;
        }

        // NaN compares false, so a NaN X marks the series as unsorted (no binary search)
        private static bool IsOrdered(double previous, double next)
        {
            return next >= previous;
        }

        // First index whose X >= value
        private int LowerBound(double value)
        {
            var lo = 0;
            var hi = _data.Count;
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                if (_data[mid].X < value)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }

            return lo;
        }

        // First index whose X > value
        private int UpperBound(double value)
        {
            var lo = 0;
            var hi = _data.Count;
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                if (_data[mid].X <= value)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }

            return lo;
        }

        /// <summary>
        /// Adds a point to the series and invalidates cache
        /// </summary>
        public void AddPoint(PointD point)
        {
            lock (SyncRoot)
            {
                _data.Add(point);
                TrackAppendedPoints(_data.Count - 1);
                InvalidateRenderCache();
            }

            NotifyChanged();
        }

        /// <summary>
        /// Adds multiple points to the series and invalidates cache
        /// </summary>
        public void AddPoints(IEnumerable<PointD> points)
        {
            // Materialize lazy sequences before taking the lock
            var batch = points as ICollection<PointD> ?? new List<PointD>(points);
            lock (SyncRoot)
            {
                var first = _data.Count;
                _data.AddRange(batch);
                TrackAppendedPoints(first);
                InvalidateRenderCache();
            }

            NotifyChanged();
        }

        /// <summary>
        /// Replaces the whole series content in a single operation and invalidates cache.
        /// More efficient than Clear + AddPoints for data-binding scenarios.
        /// </summary>
        public void ReplacePoints(IEnumerable<PointD> points)
        {
            if (ReferenceEquals(points, _data))
            {
                InvalidateCache();
                return;
            }

            var batch = points as ICollection<PointD> ?? new List<PointD>(points);
            lock (SyncRoot)
            {
                _data.Clear();
                _data.AddRange(batch);
                _sortState = SortUnknown;
                InvalidateRenderCache();
            }

            NotifyChanged();
        }

        /// <summary>
        /// Clears all data and invalidates cache
        /// </summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                _data.Clear();
                _sortState = SortedByX;
                InvalidateRenderCache();
            }

            NotifyChanged();
        }

        public virtual FRange GetXRange()
        {
            lock (SyncRoot)
            {
                if (_data.Count == 0)
                {
                    return new FRange(0, 0);
                }

                if (IsSortedByX())
                {
                    return new FRange(_data[0].X, _data[_data.Count - 1].X); // O(1)
                }

                var (min, max) = DataHelper.GetMinMax(_data, p => p.X);
                return new FRange(min, max);
            }
        }

        public virtual FRange GetYRange()
        {
            lock (SyncRoot)
            {
                if (_data.Count == 0)
                {
                    return new FRange(0, 0);
                }

                var (min, max) = DataHelper.GetMinMax(_data, p => p.Y);
                return new FRange(min, max);
            }
        }

        bool ISeriesRangeProvider.TryGetRanges(out FRange xRange, out FRange yRange)
        {
            lock (SyncRoot)
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
        }

        /// <summary>
        /// Gets resampling statistics if available
        /// </summary>
        public ResamplingStats? GetLastResamplingStats()
        {
            lock (SyncRoot)
            {
                if (_resampler is LttbResampler lttb && _cachedResampledData != null)
                {
                    return lttb.GetLastStats(_lastSourceCount, _cachedResampledData.Count);
                }

                return null;
            }
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
