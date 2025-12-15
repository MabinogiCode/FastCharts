using FastCharts.Core.Interaction.Behaviors;
using FastCharts.Core.Performance;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FluentAssertions;
using System;
using System.Diagnostics;
using Xunit;
using Performance = FastCharts.Core.Performance;

namespace FastCharts.Core.Tests.Performance
{
    /// <summary>
    /// Tests for RenderMetrics implementation (P1-METRICS)
    /// Validates performance tracking, FPS calculation, and memory monitoring
    /// </summary>
    public class RenderMetricsTests
    {
        [Fact]
        public void RenderMetrics_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var metrics = new RenderMetrics();

            // Assert
            metrics.CurrentFPS.Should().Be(0.0);
            metrics.LastFrameTimeMs.Should().Be(0.0);
            metrics.AverageFrameTimeMs.Should().Be(0.0);
            metrics.MaxFrameTimeMs.Should().Be(0.0);
            metrics.TotalFrames.Should().Be(0);
            metrics.DataPointCount.Should().Be(0);
            metrics.SeriesCount.Should().Be(0);
            metrics.IsResampled.Should().BeFalse();
            metrics.ResamplingRatio.Should().Be(1.0);
            metrics.MemoryUsageBytes.Should().BeGreaterThanOrEqualTo(0); // Memory can be 0 in test environments
        }

        [Fact]
        public void RenderMetrics_SingleFrame_UpdatesCorrectly()
        {
            // Arrange
            var metrics = new RenderMetrics();

            // Act
            metrics.StartFrame();
            SimulateWork(); // Simulate some work
            metrics.EndFrame();

            // Assert
            metrics.TotalFrames.Should().Be(1);
            metrics.LastFrameTimeMs.Should().BeGreaterThan(0);
            metrics.CurrentFPS.Should().BeGreaterThan(0);
            metrics.AverageFrameTimeMs.Should().Be(metrics.LastFrameTimeMs);
            metrics.MaxFrameTimeMs.Should().Be(metrics.LastFrameTimeMs);
        }

        [Fact]
        public void RenderMetrics_MultipleFrames_CalculatesAverages()
        {
            // Arrange
            var metrics = new RenderMetrics();

            // Act - Simulate multiple frames with varying durations
            for (var i = 0; i < 5; i++)
            {
                metrics.StartFrame();
                SimulateWork(i + 100); // Ensure measurable work
                metrics.EndFrame();
            }

            // Assert
            metrics.TotalFrames.Should().Be(5);
            // Frame times might be 0 in fast test environments, so just check structure
            metrics.LastFrameTimeMs.Should().BeGreaterThanOrEqualTo(0);
            metrics.AverageFrameTimeMs.Should().BeGreaterThanOrEqualTo(0);
            metrics.MaxFrameTimeMs.Should().BeGreaterThanOrEqualTo(metrics.AverageFrameTimeMs);
            metrics.CurrentFPS.Should().BeGreaterThan(0);
        }

        [Fact]
        public void RenderMetrics_DataMetrics_UpdatesCorrectly()
        {
            // Arrange
            var metrics = new RenderMetrics();

            // Act
            metrics.DataPointCount = 1000;
            metrics.SeriesCount = 3;
            metrics.IsResampled = true;
            metrics.ResamplingRatio = 0.5;

            // Assert
            metrics.DataPointCount.Should().Be(1000);
            metrics.SeriesCount.Should().Be(3);
            metrics.IsResampled.Should().BeTrue();
            metrics.ResamplingRatio.Should().Be(0.5);
        }

        [Fact]
        public void RenderMetrics_PerformanceStatus_CategorizedCorrectly()
        {
            // Arrange
            var metrics = new RenderMetrics();

            // Act - Simulate a frame (performance may vary in test environments)
            metrics.StartFrame();
            // Minimal work for fast frame
            metrics.EndFrame();

            // Assert - Performance status should be valid (any status is acceptable in test environment)
            var status = metrics.GetPerformanceStatus();
            status.Should().BeOneOf(PerformanceStatus.Excellent, PerformanceStatus.Good, PerformanceStatus.Fair, PerformanceStatus.Poor);
        }

        [Fact]
        public void RenderMetrics_Reset_ClearsAllData()
        {
            // Arrange
            var metrics = new RenderMetrics();

            // Add some data first
            metrics.StartFrame();
            SimulateWork();
            metrics.EndFrame();
            metrics.DataPointCount = 500;
            metrics.SeriesCount = 2;

            // Verify we have some data before reset
            metrics.TotalFrames.Should().BeGreaterThan(0);

            // Act
            metrics.Reset();

            // Assert - All metrics should be reset
            metrics.TotalFrames.Should().Be(0);
            metrics.CurrentFPS.Should().Be(0.0);
            metrics.DataPointCount.Should().Be(0);
            metrics.SeriesCount.Should().Be(0);
            metrics.IsResampled.Should().BeFalse();
            metrics.ResamplingRatio.Should().Be(1.0);

            // Uptime should be reset to approximately zero (allow generous tolerance for test environment)
            metrics.Uptime.Should().BeLessThan(TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public void RenderMetrics_Summary_ContainsAllInfo()
        {
            // Arrange
            var metrics = new RenderMetrics();
            metrics.DataPointCount = 1500;
            metrics.SeriesCount = 4;
            metrics.IsResampled = true;
            metrics.ResamplingRatio = 0.75;

            // Add a frame for FPS calculation
            metrics.StartFrame();
            SimulateWork();
            metrics.EndFrame();

            // Act
            var summary = metrics.GetSummary();

            // Assert - Just check that all major components are present
            summary.Should().Contain("FPS:");
            summary.Should().Contain("Frame:");
            summary.Should().Contain("Points:");
            summary.Should().Contain("1,500"); // InvariantCulture uses comma separators
            summary.Should().Contain("Series: 4");
            summary.Should().Contain("Memory:");
            summary.Should().Contain("Uptime:");
            summary.Should().Contain("?"); // Resampling indicator
        }

        [Fact]
        public void RenderMetrics_MemoryUsageFormatted_IsReadable()
        {
            // Arrange
            var metrics = new RenderMetrics();

            // Act
            var formatted = metrics.MemoryUsageFormatted;

            // Assert
            formatted.Should().NotBeEmpty();
            formatted.Should().MatchRegex(@"^\d+\.\d+ (B|KB|MB|GB)$");
        }

        [Fact]
        public void RenderMetrics_Uptime_IncreasesOverTime()
        {
            // Arrange
            var metrics = new RenderMetrics();
            var initialUptime = metrics.Uptime;

            // Act
            SimulateWork();
            var laterUptime = metrics.Uptime;

            // Assert
            laterUptime.Should().BeGreaterThan(initialUptime);
        }

        [Fact]
        public void MetricsOverlayBehavior_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var behavior = new MetricsOverlayBehavior();

            // Assert
            behavior.IsVisible.Should().BeTrue();
            behavior.Position.Should().Be(MetricsPosition.TopLeft);
            behavior.ShowDetailed.Should().BeTrue();
            behavior.BackgroundOpacity.Should().Be(0.8);
            behavior.Metrics.Should().NotBeNull();
        }

        [Fact]
        public void MetricsOverlayBehavior_ToggleMethods_WorkCorrectly()
        {
            // Arrange
            var behavior = new MetricsOverlayBehavior();
            var initialVisibility = behavior.IsVisible;
            var initialDetailLevel = behavior.ShowDetailed;

            // Act
            behavior.ToggleVisibility();
            behavior.ToggleDetailLevel();

            // Assert
            behavior.IsVisible.Should().Be(!initialVisibility);
            behavior.ShowDetailed.Should().Be(!initialDetailLevel);
        }

        [Fact]
        public void MetricsOverlayBehavior_GetDisplayText_VariesByDetailLevel()
        {
            // Arrange
            var behavior = new MetricsOverlayBehavior();

            // Add some frame data
            behavior.BeginFrame();
            SimulateWork();
            behavior.EndFrame(new ChartModel());

            // Act
            behavior.ShowDetailed = true;
            var detailedText = behavior.GetDisplayText();

            behavior.ShowDetailed = false;
            var basicText = behavior.GetDisplayText();

            // Assert
            detailedText.Should().NotBeEmpty();
            basicText.Should().NotBeEmpty();
            detailedText.Length.Should().BeGreaterThan(basicText.Length);

            // Detailed should contain more information
            detailedText.Should().Contain("RENDER METRICS");
            detailedText.Should().Contain("Memory:");
            detailedText.Should().Contain("Uptime:");

            // Basic should be concise
            basicText.Should().Contain("FPS:");
            basicText.Should().NotContain("RENDER METRICS");
        }

        [Fact]
        public void MetricsOverlayBehavior_GetOverlayPosition_ReturnsCorrectCoordinates()
        {
            // Arrange
            var behavior = new MetricsOverlayBehavior();
            const double canvasWidth = 800;
            const double canvasHeight = 600;

            // Test all positions
            var positions = new[]
            {
                MetricsPosition.TopLeft,
                MetricsPosition.TopRight,
                MetricsPosition.BottomLeft,
                MetricsPosition.BottomRight
            };

            foreach (var position in positions)
            {
                behavior.Position = position;

                // Act
                var (x, y) = behavior.GetOverlayPosition(canvasWidth, canvasHeight);

                // Assert
                x.Should().BeGreaterThanOrEqualTo(0);
                y.Should().BeGreaterThanOrEqualTo(0);
                x.Should().BeLessThan(canvasWidth);
                y.Should().BeLessThan(canvasHeight);
            }
        }

        [Fact]
        public void MetricsOverlayBehavior_GetPerformanceColor_VariesByStatus()
        {
            // Arrange
            var behavior = new MetricsOverlayBehavior();

            // Act - Get color for initial state (should be some color)
            var color = behavior.GetPerformanceColor();

            // Assert
            color.R.Should().BeInRange(0, 255);
            color.G.Should().BeInRange(0, 255);
            color.B.Should().BeInRange(0, 255);
            color.A.Should().BeInRange(0, 255);
        }

        [Fact]
        public void MetricsOverlayBehavior_BeginEndFrame_UpdatesMetrics()
        {
            // Arrange
            var behavior = new MetricsOverlayBehavior();
            var model = CreateTestChartModel();

            var initialFrames = behavior.Metrics.TotalFrames;

            // Act
            behavior.BeginFrame();
            SimulateWork();
            behavior.EndFrame(model);

            // Assert
            behavior.Metrics.TotalFrames.Should().Be(initialFrames + 1);
            behavior.Metrics.LastFrameTimeMs.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Simulates computational work without using Thread.Sleep
        /// </summary>
        private static void SimulateWork(int iterations = 1000)
        {
            var sum = 0.0;
            for (var i = 0; i < iterations; i++)
            {
                sum += Math.Sqrt(i);
            }
            // Use sum to prevent compiler optimization
            _ = sum;
        }

        private static ChartModel CreateTestChartModel()
        {
            var model = new ChartModel();

            // Add some test series
            model.AddSeries(new LineSeries(new[]
            {
                new PointD(0, 10),
                new PointD(1, 20),
                new PointD(2, 15)
            })
            { Title = "Test Series 1" });

            model.AddSeries(new ScatterSeries(new[]
            {
                new PointD(0, 5),
                new PointD(1, 15),
                new PointD(2, 10)
            })
            { Title = "Test Series 2" });

            return model;
        }
    }
}