using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FastCharts.Core.Tests.Series
{
    /// <summary>
    /// Tests for StreamingLineSeries implementation (P1-STREAM-APPEND)
    /// Validates streaming operations, rolling windows, and performance
    /// </summary>
    public class StreamingLineSeriesTests
    {
        [Fact]
        public void StreamingLineSeries_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var series = new StreamingLineSeries(maxPointCount: 1000, rollingWindow: TimeSpan.FromMinutes(30));

            // Assert
            series.MaxPointCount.Should().Be(1000);
            series.RollingWindowDuration.Should().Be(TimeSpan.FromMinutes(30));
            series.PointCount.Should().Be(0);
            series.IsEmpty.Should().BeTrue();
            series.EnableAutoResampling.Should().BeTrue();
        }

        [Fact]
        public void StreamingLineSeries_WithInitialData_LoadsCorrectly()
        {
            // Arrange
            var initialData = new[]
            {
                new PointD(1, 10),
                new PointD(2, 20),
                new PointD(3, 30)
            };

            // Act
            var series = new StreamingLineSeries(initialData, maxPointCount: 1000);

            // Assert
            series.PointCount.Should().Be(3);
            series.Data.Should().BeEquivalentTo(initialData);
            series.IsEmpty.Should().BeFalse();
        }

        [Fact]
        public void AppendPoint_SinglePoint_AddsSuccessfully()
        {
            // Arrange
            var series = new StreamingLineSeries();
            var pointsAddedEventFired = false;
            series.PointsAdded += (s, e) => pointsAddedEventFired = true;

            var newPoint = new PointD(1, 100);

            // Act
            series.AppendPoint(newPoint);

            // Assert
            series.PointCount.Should().Be(1);
            series.Data.Should().Contain(newPoint);
            pointsAddedEventFired.Should().BeTrue();
        }

        [Fact]
        public void AppendPoints_MultiplePoints_AddsBatch()
        {
            // Arrange
            var series = new StreamingLineSeries();
            StreamingDataEventArgs? lastEvent = null;
            series.PointsAdded += (s, e) => lastEvent = e;

            var newPoints = new[]
            {
                new PointD(1, 10),
                new PointD(2, 20),
                new PointD(3, 30)
            };

            // Act
            series.AppendPoints(newPoints);

            // Assert
            series.PointCount.Should().Be(3);
            series.Data.Should().BeEquivalentTo(newPoints);
            lastEvent.Should().NotBeNull();
            lastEvent!.PointsAdded.Should().Be(3);
            lastEvent.PointCount.Should().Be(3);
        }

        [Fact]
        public void MaxPointCount_EnforcesLimit_RemovesOldestPoints()
        {
            // Arrange
            var series = new StreamingLineSeries(maxPointCount: 3);
            StreamingDataEventArgs? lastRemovedEvent = null;
            series.PointsRemoved += (s, e) => lastRemovedEvent = e;

            // Act - Add more points than the limit
            for (var i = 0; i < 5; i++)
            {
                series.AppendPoint(new PointD(i, i * 10));
            }

            // Assert - Should keep only the last 3 points
            series.PointCount.Should().Be(3);
            series.Data.Should().BeEquivalentTo(new[]
            {
                new PointD(2, 20),
                new PointD(3, 30),
                new PointD(4, 40)
            });
            lastRemovedEvent.Should().NotBeNull();
            lastRemovedEvent!.PointsRemoved.Should().BeGreaterThan(0);
        }

        [Fact]
        public void RollingWindowDuration_EnforcesTimeLimit_RemovesOldPoints()
        {
            // Arrange
            var baseTime = DateTime.UtcNow;
            var series = new StreamingLineSeries(rollingWindow: TimeSpan.FromMinutes(30));
            StreamingDataEventArgs? lastRemovedEvent = null;
            series.PointsRemoved += (s, e) => lastRemovedEvent = e;

            // Act - Add points with different timestamps
            series.AppendPoint(new PointD((baseTime - TimeSpan.FromHours(1)).ToOADate(), 10)); // Old - should be removed
            series.AppendPoint(new PointD((baseTime - TimeSpan.FromMinutes(10)).ToOADate(), 20)); // Recent - should stay
            series.AppendPoint(new PointD(baseTime.ToOADate(), 30)); // Current - should stay

            // Assert
            series.PointCount.Should().Be(2); // Old point should be trimmed
            lastRemovedEvent.Should().NotBeNull();
            lastRemovedEvent!.PointsRemoved.Should().Be(1);
        }

        [Fact]
        public void AppendRealTimePoint_WithCurrentTime_AddsCorrectly()
        {
            // Arrange
            var series = new StreamingLineSeries();
            var testTime = DateTime.UtcNow;

            // Act
            series.AppendRealTimePoint(42.5, testTime);

            // Assert
            series.PointCount.Should().Be(1);
            var addedPoint = series.Data[0];
            addedPoint.Y.Should().Be(42.5);

            // Check time is approximately correct (within 1 second tolerance)
            var pointTime = DateTime.FromOADate(addedPoint.X);
            Math.Abs((pointTime - testTime).TotalSeconds).Should().BeLessThan(1.0);
        }

        [Fact]
        public void AppendRealTimePoints_MultipleBatch_AddsAllCorrectly()
        {
            // Arrange
            var series = new StreamingLineSeries();
            var baseTime = DateTime.UtcNow;
            var testData = new[]
            {
                (baseTime, 10.0),
                (baseTime.AddSeconds(1), 20.0),
                (baseTime.AddSeconds(2), 30.0)
            };

            // Act
            series.AppendRealTimePoints(testData);

            // Assert
            series.PointCount.Should().Be(3);
            series.Data.Select(p => p.Y).Should().BeEquivalentTo(new[] { 10.0, 20.0, 30.0 });
        }

        [Fact]
        public void TrimToWindow_ManualCall_RemovesOldData()
        {
            // Arrange
            var series = new StreamingLineSeries();

            // Add some points
            for (var i = 0; i < 10; i++)
            {
                series.AppendPoint(new PointD(i, i));
            }

            // Now set a smaller limit
            series.MaxPointCount = 5;

            // Act
            series.TrimToWindow();

            // Assert
            series.PointCount.Should().Be(5);
            series.Data.Should().BeEquivalentTo(new[]
            {
                new PointD(5, 5),
                new PointD(6, 6),
                new PointD(7, 7),
                new PointD(8, 8),
                new PointD(9, 9)
            });
        }

        [Fact]
        public void GetRenderData_LargeDataset_AppliesResampling()
        {
            // Arrange - Create large dataset
            var series = new StreamingLineSeries();
            var largeDataset = Enumerable.Range(0, 5000)
                .Select(i => new PointD(i, Math.Sin(i * 0.01)))
                .ToArray();

            series.AppendPoints(largeDataset);

            // Act
            var renderData = series.GetRenderData(800); // 800px viewport

            // Assert
            renderData.Count.Should().BeLessThan(5000); // Should be resampled
            renderData.Count.Should().BeGreaterThan(100); // Should have reasonable detail
            renderData[0].Should().Be(largeDataset[0]); // Should preserve endpoints
            renderData[renderData.Count - 1].Should().Be(largeDataset[largeDataset.Length - 1]);
        }

        [Fact]
        public void OldestPointAge_WithData_ReturnsCorrectAge()
        {
            // Arrange
            var series = new StreamingLineSeries();
            var oldTime = DateTime.UtcNow - TimeSpan.FromMinutes(10);
            var recentTime = DateTime.UtcNow;

            series.AppendPoint(new PointD(recentTime.ToOADate(), 20));
            series.AppendPoint(new PointD(oldTime.ToOADate(), 10));

            // Act
            var age = series.OldestPointAge;

            // Assert
            age.Should().NotBeNull();
            age!.Value.Should().BeCloseTo(TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void OldestPointAge_EmptyData_ReturnsNull()
        {
            // Arrange
            var series = new StreamingLineSeries();

            // Act
            var age = series.OldestPointAge;

            // Assert
            age.Should().BeNull();
        }

        [Fact]
        public void CreateRealTime_FactoryMethod_ConfiguresCorrectly()
        {
            // Arrange & Act
            var series = StreamingLineSeries.CreateRealTime(maxPoints: 5000, rollingWindow: TimeSpan.FromMinutes(45), title: "Test Streaming");

            // Assert
            series.MaxPointCount.Should().Be(5000);
            series.RollingWindowDuration.Should().Be(TimeSpan.FromMinutes(45));
            series.Title.Should().Be("Test Streaming");
            series.EnableAutoResampling.Should().BeTrue();
            series.AutoResampleThreshold.Should().Be(2000);
            series.StrokeThickness.Should().Be(1.5);
        }

        [Fact]
        public void HighFrequencyStreaming_Performance_HandlesLargeVolume()
        {
            // Arrange
            var series = new StreamingLineSeries(maxPointCount: 5000);
            var random = new Random(42); // Deterministic for tests
            var totalPointsAdded = 0;

            // Act - Simulate high-frequency streaming
            var startTime = DateTime.UtcNow;
            for (var i = 0; i < 10000; i++)
            {
                var value = 50 + (25 * Math.Sin(i * 0.1)) + (5 * random.NextDouble());
                series.AppendRealTimePoint(value);
                totalPointsAdded++;
            }
            var elapsed = DateTime.UtcNow - startTime;

            // Assert - Performance characteristics
            series.PointCount.Should().BeLessThanOrEqualTo(5000); // Respects max count
            series.PointCount.Should().BeGreaterThan(1000); // Should have significant data
            totalPointsAdded.Should().Be(10000); // All points were processed
            elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5)); // Should be fast

            // Verify data integrity
            series.Data.Should().NotBeEmpty();
            var lastPoint = series.Data[series.Data.Count - 1];

            // Check time is approximately current (within 1 minute tolerance)
            var expectedTime = DateTime.UtcNow.ToOADate();
            var timeDifferenceInDays = Math.Abs(lastPoint.X - expectedTime);
            timeDifferenceInDays.Should().BeLessThan(TimeSpan.FromMinutes(1).TotalDays);
        }

        [Fact]
        public void EventsRaising_AddAndRemove_FiresCorrectly()
        {
            // Arrange
            var series = new StreamingLineSeries(maxPointCount: 3);
            var addEvents = new List<StreamingDataEventArgs>();
            var removeEvents = new List<StreamingDataEventArgs>();

            series.PointsAdded += (s, e) => addEvents.Add(e);
            series.PointsRemoved += (s, e) => removeEvents.Add(e);

            // Act
            series.AppendPoint(new PointD(1, 10)); // Add event
            series.AppendPoint(new PointD(2, 20)); // Add event
            series.AppendPoint(new PointD(3, 30)); // Add event
            series.AppendPoint(new PointD(4, 40)); // Add + Remove events (exceeds limit)

            // Assert
            addEvents.Count.Should().Be(4); // All additions recorded
            removeEvents.Count.Should().BeGreaterThanOrEqualTo(1); // At least one removal

            var lastAddEvent = addEvents[addEvents.Count - 1];
            lastAddEvent.PointsAdded.Should().Be(1);
            lastAddEvent.PointCount.Should().Be(3); // Final count after trimming
        }

        [Fact]
        public void MaxPointCount_SetToNull_DisablesCountLimit()
        {
            // Arrange
            var series = new StreamingLineSeries(maxPointCount: 3);

            // Add points up to limit
            for (var i = 0; i < 5; i++)
            {
                series.AppendPoint(new PointD(i, i));
            }
            series.PointCount.Should().Be(3); // Should be limited

            // Act - Disable limit
            series.MaxPointCount = null;
            for (var i = 5; i < 10; i++)
            {
                series.AppendPoint(new PointD(i, i));
            }

            // Assert - Should now allow unlimited points
            series.PointCount.Should().Be(8); // 3 original + 5 new
        }

        [Fact]
        public void RangeCalculation_WithStreamingData_ReturnsCorrectRanges()
        {
            // Arrange
            var series = new StreamingLineSeries();
            var testData = new[]
            {
                new PointD(1, 10),
                new PointD(2, 5),
                new PointD(3, 15),
                new PointD(4, 20)
            };

            // Act
            series.AppendPoints(testData);

            // Assert
            var xRange = series.GetXRange();
            var yRange = series.GetYRange();

            xRange.Min.Should().Be(1);
            xRange.Max.Should().Be(4);
            yRange.Min.Should().Be(5);
            yRange.Max.Should().Be(20);
        }
    }
}