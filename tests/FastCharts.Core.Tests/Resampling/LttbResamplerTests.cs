using FastCharts.Core.Primitives;
using FastCharts.Core.Resampling;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FastCharts.Core.Tests.Resampling
{
    /// <summary>
    /// Tests for LttbResampler implementation (P1-RESAMPLE-LTTB)
    /// Validates edge cases, quality preservation, and performance
    /// </summary>
    public class LttbResamplerTests
    {
        private readonly LttbResampler _resampler = new();

        [Fact]
        public void LttbResampler_Properties_AreCorrect()
        {
            // Assert
            _resampler.AlgorithmName.Should().Contain("LTTB");
            _resampler.PreservesShape.Should().BeTrue();
            _resampler.OptimalRange.MinPoints.Should().BeGreaterThan(0);
            _resampler.OptimalRange.MaxPoints.Should().BeGreaterThan(_resampler.OptimalRange.MinPoints);
        }

        [Fact]
        public void Resample_EmptyData_ReturnsEmpty()
        {
            // Arrange
            var emptyData = Array.Empty<PointD>();

            // Act
            var result = _resampler.Resample(emptyData, 100);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Resample_NullData_ReturnsEmpty()
        {
            // Act
            var result = _resampler.Resample(null!, 100);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Resample_SmallDatasetLargerThanTarget_ReturnsOriginal()
        {
            // Arrange - 🛡️ KEY TEST: Small series protection!
            var smallData = new[]
            {
                new PointD(0, 10),
                new PointD(1, 20),
                new PointD(2, 15),
                new PointD(3, 25),
                new PointD(4, 30)
            };

            // Act - Request MORE points than available
            var result = _resampler.Resample(smallData, 10);

            // Assert - Should return ALL original points (ZERO loss!)
            result.Should().BeEquivalentTo(smallData);
            result.Count.Should().Be(5); // Preserved all 5 points
        }

        [Fact]
        public void Resample_ExactTargetCount_ReturnsOriginal()
        {
            // Arrange
            var data = GenerateLinearData(100);

            // Act - Request exact same number of points
            var result = _resampler.Resample(data, 100);

            // Assert - Should return original (no processing needed)
            result.Should().BeEquivalentTo(data);
        }

        [Fact]
        public void Resample_TinyDataset_HandlesGracefully()
        {
            // Arrange - Very small datasets
            var twoPoints = new[] { new PointD(0, 10), new PointD(10, 20) };
            var threePoints = new[] { new PointD(0, 10), new PointD(5, 15), new PointD(10, 20) };

            // Act & Assert - 2 points
            var result2 = _resampler.Resample(twoPoints, 5);
            result2.Should().BeEquivalentTo(twoPoints); // Can't resample 2 points to 5

            // Act & Assert - 3 points  
            var result3 = _resampler.Resample(threePoints, 3);
            result3.Should().BeEquivalentTo(threePoints); // Perfect match
        }

        [Fact]
        public void Resample_SinglePoint_HandlesSafely()
        {
            // Arrange
            var singlePoint = new[] { new PointD(5, 10) };

            // Act
            var result = _resampler.Resample(singlePoint, 10);

            // Assert
            result.Should().BeEquivalentTo(singlePoint);
        }

        [Fact]
        public void Resample_LargeDataset_ReducesPointsEffectively()
        {
            // Arrange - Large dataset (this is where LTTB shines!)
            var largeData = GenerateSinusoidalData(10000); // 10K points

            // Act - Reduce to 100 points
            var result = _resampler.Resample(largeData, 100);

            // Assert
            result.Count.Should().Be(100); // Exact target
            result[0].Should().Be(largeData[0]); // First point preserved
            result[^1].Should().Be(largeData[^1]); // Last point preserved
        }

        [Fact]
        public void Resample_PreservesEndpoints()
        {
            // Arrange
            var data = GenerateLinearData(1000);
            var firstPoint = data[0];
            var lastPoint = data[^1];

            // Act
            var result = _resampler.Resample(data, 50);

            // Assert - Endpoints MUST be preserved for continuity
            result[0].Should().Be(firstPoint);
            result[^1].Should().Be(lastPoint);
        }

        [Fact]
        public void Resample_MaintainsXOrdering()
        {
            // Arrange
            var data = GenerateRandomData(1000);
            data = data.OrderBy(p => p.X).ToArray(); // Ensure sorted

            // Act
            var result = _resampler.Resample(data, 100);

            // Assert - X values should remain in ascending order
            for (var i = 1; i < result.Count; i++)
            {
                result[i].X.Should().BeGreaterThanOrEqualTo(result[i - 1].X);
            }
        }

        [Fact]
        public void Resample_PreservesVisualShape_SinWave()
        {
            // Arrange - Sin wave with clear peaks and valleys
            var sinData = GenerateSinusoidalData(5000);

            // Act - Aggressive reduction
            var result = _resampler.Resample(sinData, 200);

            // Assert - Should preserve general sin wave characteristics
            result.Count.Should().Be(200);

            // Find peaks in original and resampled data
            var originalPeaks = FindPeaks(sinData).Count;
            var resampledPeaks = FindPeaks(result).Count;

            // Should preserve most peaks (LTTB's strength!)
            var peakPreservationRatio = (double)resampledPeaks / originalPeaks;
            peakPreservationRatio.Should().BeGreaterThan(0.7); // At least 70% of peaks preserved
        }

        [Fact]
        public void Resample_PerformanceTest_CompletesQuickly()
        {
            // Arrange - Very large dataset
            var hugeData = GenerateLinearData(100_000); // 100K points

            // Act & Measure
            var startTime = DateTime.UtcNow;
            var result = _resampler.Resample(hugeData, 1000);
            var elapsed = DateTime.UtcNow - startTime;

            // Assert - Should complete in reasonable time
            result.Count.Should().Be(1000);
            elapsed.TotalMilliseconds.Should().BeLessThan(100); // < 100ms for 100K points
            _resampler.LastOperationTime.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Resample_ZeroTargetCount_ReturnsEmpty()
        {
            // Arrange
            var data = GenerateLinearData(100);

            // Act
            var result = _resampler.Resample(data, 0);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Resample_VerySmallTarget_ReturnsEndpoints()
        {
            // Arrange
            var data = GenerateLinearData(1000);

            // Act
            var result = _resampler.Resample(data, 2);

            // Assert - Should return first and last points
            result.Count.Should().Be(2);
            result[0].Should().Be(data[0]);
            result[1].Should().Be(data[^1]);
        }

        // Helper methods for test data generation
        private static PointD[] GenerateLinearData(int count)
        {
            var points = new PointD[count];
            for (var i = 0; i < count; i++)
            {
                points[i] = new PointD(i, i * 0.5);
            }
            return points;
        }

        private static PointD[] GenerateSinusoidalData(int count)
        {
            var points = new PointD[count];
            for (var i = 0; i < count; i++)
            {
                var x = i * Math.PI * 4 / count; // 4 complete cycles
                var y = Math.Sin(x) * 100;
                points[i] = new PointD(x, y);
            }
            return points;
        }

        private static PointD[] GenerateRandomData(int count)
        {
            var random = new Random(42); // Fixed seed for reproducible tests
            var points = new PointD[count];
            for (var i = 0; i < count; i++)
            {
                points[i] = new PointD(i, random.NextDouble() * 100);
            }
            return points;
        }

        private static List<PointD> FindPeaks(IReadOnlyList<PointD> data)
        {
            var peaks = new List<PointD>();
            for (var i = 1; i < data.Count - 1; i++)
            {
                if (data[i].Y > data[i - 1].Y && data[i].Y > data[i + 1].Y)
                {
                    peaks.Add(data[i]);
                }
            }
            return peaks;
        }
    }
}