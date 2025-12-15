using System;
using System.Collections.Generic;
using System.Linq;
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
        private IResampler? _resampler;
        private bool _enableAutoResampling = true;
        private int _autoResampleThreshold = 2000; // Start resampling above 2K points
        private IReadOnlyList<PointD>? _cachedResampledData;
        private int _lastViewportPixelWidth = -1;

        public IList<PointD> Data { get; }
        public override bool IsEmpty => Data == null || Data.Count == 0;

        public LineSeries()
        {
            Data = new List<PointD>();
            StrokeThickness = 1.0;
            _resampler = new LttbResampler(); // Default to LTTB
        }

        public LineSeries(IEnumerable<PointD> points)
        {
            Data = new List<PointD>(points);
            StrokeThickness = 1.0;
            _resampler = new LttbResampler(); // Default to LTTB
        }

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
        /// Gets the effective data for rendering, with resampling applied if needed
        /// This is what renderers should use instead of raw Data
        /// </summary>
        /// <param name="viewportPixelWidth">Available pixel width for rendering</param>
        /// <returns>Optimized data for rendering</returns>
        public IReadOnlyList<PointD> GetRenderData(int viewportPixelWidth = 800)
        {
            // If resampling is disabled or no resampler, return raw data
            if (!_enableAutoResampling || _resampler == null)
            {
                return Data.ToArray(); // Convert to IReadOnlyList
            }

            // If data is small, no need to resample
            if (Data.Count <= _autoResampleThreshold)
            {
                return Data.ToArray(); // Convert to IReadOnlyList
            }

            // Check cache validity
            if (_cachedResampledData != null && _lastViewportPixelWidth == viewportPixelWidth)
            {
                return _cachedResampledData;
            }

            // Calculate optimal point count based on viewport
            var targetPointCount = CalculateOptimalPointCount(viewportPixelWidth);

            // Perform resampling
            _cachedResampledData = _resampler.Resample(Data.ToArray(), targetPointCount); // Convert to IReadOnlyList
            _lastViewportPixelWidth = viewportPixelWidth;

            return _cachedResampledData;
        }

        /// <summary>
        /// Calculates optimal point count based on viewport pixel width
        /// Strategy: ~2-4 points per pixel for smooth curves, capped at reasonable limits
        /// </summary>
        private int CalculateOptimalPointCount(int viewportPixelWidth)
        {
            // Base calculation: 2-3 points per pixel for smooth rendering
            var baseTarget = viewportPixelWidth * 2;

            // Apply reasonable bounds
            var minPoints = Math.Min(100, Data.Count);
            var maxPoints = Math.Min(5000, Data.Count); // Cap at 5K for performance

            return Math.Max(minPoints, Math.Min(maxPoints, baseTarget));
        }

        /// <summary>
        /// Invalidates the resampling cache, forcing recalculation on next render
        /// </summary>
        public void InvalidateCache()
        {
            _cachedResampledData = null;
            _lastViewportPixelWidth = -1;
        }

        /// <summary>
        /// Adds a point to the series and invalidates cache
        /// </summary>
        public void AddPoint(PointD point)
        {
            Data.Add(point);
            InvalidateCache();
        }

        /// <summary>
        /// Adds multiple points to the series and invalidates cache
        /// </summary>
        public void AddPoints(IEnumerable<PointD> points)
        {
            foreach (var point in points)
            {
                Data.Add(point);
            }
            InvalidateCache();
        }

        /// <summary>
        /// Clears all data and invalidates cache
        /// </summary>
        public void Clear()
        {
            Data.Clear();
            InvalidateCache();
        }

        public FRange GetXRange()
        {
            if (Data.Count == 0)
            {
                return new FRange(0, 0);
            }
            var (min, max) = DataHelper.GetMinMax(Data, p => p.X);
            return new FRange(min, max);
        }

        public FRange GetYRange()
        {
            if (Data.Count == 0)
            {
                return new FRange(0, 0);
            }
            var (min, max) = DataHelper.GetMinMax(Data, p => p.Y);
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
                return lttb.GetLastStats(Data.Count, _cachedResampledData.Count);
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
