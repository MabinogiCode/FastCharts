using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FastCharts.Core.Resampling
{
    /// <summary>
    /// LTTB (Largest Triangle Three Buckets) resampling algorithm implementation
    /// Optimized for preserving visual characteristics while dramatically reducing point count
    /// Automatically handles edge cases and small datasets gracefully
    /// </summary>
    public sealed class LttbResampler : IResampler
    {
        public string AlgorithmName => "LTTB (Largest Triangle Three Buckets)";
        public bool PreservesShape => true;
        public (int MinPoints, int MaxPoints) OptimalRange => (100, 10_000_000);

        /// <summary>
        /// Resamples data using the LTTB algorithm with intelligent edge case handling
        /// </summary>
        /// <param name="data">Source data points (must be sorted by X)</param>
        /// <param name="targetCount">Desired number of output points</param>
        /// <returns>Resampled points preserving visual characteristics</returns>
        public IReadOnlyList<PointD> Resample(IReadOnlyList<PointD> data, int targetCount)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                // 🛡️ Edge case: No data
                if (data == null || data.Count == 0 || targetCount <= 0)
                {
                    return Array.Empty<PointD>();
                }

                // 🛡️ Edge case: Already smaller than target - NO RESAMPLING NEEDED!
                if (data.Count <= targetCount)
                {
                    return data; // Return original data - ZERO quality loss!
                }

                // 🛡️ Edge case: Tiny datasets (can't form triangles)
                if (data.Count <= 3 || targetCount <= 2)
                {
                    return HandleTinyDataset(data, targetCount);
                }

                // ⚡ Main LTTB algorithm for larger datasets
                return PerformLttbResampling(data, targetCount);
            }
            finally
            {
                sw.Stop();
                LastOperationTime = sw.Elapsed.TotalMilliseconds;
            }
        }

        /// <summary>
        /// Time taken for the last resampling operation in milliseconds
        /// </summary>
        public double LastOperationTime { get; private set; }

        /// <summary>
        /// Gets resampling statistics from the last operation
        /// </summary>
        public ResamplingStats GetLastStats(int originalCount, int resampledCount)
        {
            var reductionRatio = originalCount > 0 ? (double)resampledCount / originalCount : 1.0;
            return new ResamplingStats(originalCount, resampledCount, reductionRatio, LastOperationTime);
        }

        /// <summary>
        /// Handles very small datasets that can't use full LTTB algorithm
        /// </summary>
        private static IReadOnlyList<PointD> HandleTinyDataset(IReadOnlyList<PointD> data, int targetCount)
        {
            if (targetCount <= 0)
            {
                return Array.Empty<PointD>();
            }
            if (targetCount == 1)
            {
                return new[] { data[data.Count / 2] };
            }
            if (targetCount == 2)
            {
                return new[] { data[0], data[data.Count - 1] };
            }
            if (data.Count <= targetCount)
            {
                return data;
            }
            var result = new List<PointD>(targetCount) { data[0] };
            for (var i = 1; i < targetCount - 1; i++)
            {
                var index = (int)Math.Round(i * (data.Count - 1.0) / (targetCount - 1.0));
                if (index >= data.Count) index = data.Count - 1;
                result.Add(data[index]);
            }
            result.Add(data[data.Count - 1]);
            return result;
        }

        /// <summary>
        /// Performs the main LTTB resampling algorithm
        /// </summary>
        private static IReadOnlyList<PointD> PerformLttbResampling(IReadOnlyList<PointD> data, int targetCount)
        {
            var result = new List<PointD>(targetCount) { data[0] }; // Always include the first point

            // Calculate bucket size (how many original points per output point)
            var bucketSize = (double)(data.Count - 2) / (targetCount - 2);

            // Process buckets (skip first and last)
            for (var i = 1; i < targetCount - 1; i++)
            {
                // Calculate bucket range
                var bucketStart = (int)Math.Floor(bucketSize * (i - 1)) + 1;
                var bucketEnd = (int)Math.Floor(bucketSize * i) + 1;
                if (bucketEnd >= data.Count - 1) bucketEnd = data.Count - 2;

                // Get the previous point (from result)
                var prevPoint = result[result.Count - 1];

                // Calculate average of next bucket for triangle calculation
                var nextBucketStart = (int)Math.Floor(bucketSize * i) + 1;
                var nextBucketEnd = (int)Math.Floor(bucketSize * (i + 1)) + 1;
                if (nextBucketEnd >= data.Count) nextBucketEnd = data.Count - 1;

                var avgX = 0.0;
                var avgY = 0.0;
                var count = 0;

                for (var j = nextBucketStart; j < nextBucketEnd; j++)
                {
                    avgX += data[j].X;
                    avgY += data[j].Y;
                    count++;
                }

                if (count > 0)
                {
                    avgX /= count;
                    avgY /= count;
                }
                else
                {
                    avgX = data[bucketEnd].X;
                    avgY = data[bucketEnd].Y;
                }

                // Find point in current bucket that creates largest triangle
                var maxArea = -1.0;
                var selectedIndex = bucketStart;

                for (var j = bucketStart; j < bucketEnd; j++)
                {
                    var area = CalculateTriangleArea(
                        prevPoint.X, prevPoint.Y,
                        data[j].X, data[j].Y,
                        avgX, avgY);

                    if (area > maxArea)
                    {
                        maxArea = area;
                        selectedIndex = j;
                    }
                }

                result.Add(data[selectedIndex]);
            }

            // Always include the last point
            result.Add(data[data.Count - 1]);

            return result;
        }

        /// <summary>
        /// Calculates the area of a triangle formed by three points
        /// Uses the cross product formula for efficiency
        /// </summary>
        private static double CalculateTriangleArea(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            // Triangle area = |x1(y2-y3) + x2(y3-y1) + x3(y1-y2)| / 2
            // We can skip division by 2 since we only need relative areas
            return Math.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
        }
    }

    /// <summary>
    /// Statistics about a resampling operation
    /// </summary>
    public readonly struct ResamplingStats
    {
        public ResamplingStats(int originalCount, int resampledCount, double reductionRatio, double elapsedMs)
        {
            OriginalCount = originalCount;
            ResampledCount = resampledCount;
            ReductionRatio = reductionRatio;
            ElapsedMs = elapsedMs;
        }

        /// <summary>
        /// Original number of data points
        /// </summary>
        public int OriginalCount { get; }

        /// <summary>
        /// Number of points after resampling
        /// </summary>
        public int ResampledCount { get; }

        /// <summary>
        /// Reduction ratio (0.0 to 1.0, where 0.1 = 90% reduction)
        /// </summary>
        public double ReductionRatio { get; }

        /// <summary>
        /// Time taken for resampling operation in milliseconds
        /// </summary>
        public double ElapsedMs { get; }

        /// <summary>
        /// Percentage of data reduction (0-100%)
        /// </summary>
        public double ReductionPercentage => (1.0 - ReductionRatio) * 100.0;

        public override string ToString()
        {
            return $"Resampled {OriginalCount:N0} → {ResampledCount:N0} points ({ReductionPercentage:F1}% reduction) in {ElapsedMs:F2}ms";
        }
    }
}