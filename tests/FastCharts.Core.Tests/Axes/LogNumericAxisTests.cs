using FastCharts.Core.Axes;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Formatting;
using FastCharts.Core.Primitives;
using FastCharts.Core.Scales;
using FluentAssertions;
using System;
using Xunit;

namespace FastCharts.Core.Tests.Axes
{
    /// <summary>
    /// Tests for LogNumericAxis implementation (P1-AX-LOG)
    /// </summary>
    public class LogNumericAxisTests
    {
        [Fact]
        public void LogNumericAxis_Constructor_DefaultBase10_InitializesCorrectly()
        {
            // Arrange & Act
            var axis = new LogNumericAxis();

            // Assert
            axis.LogBase.Should().Be(10.0);
            axis.DataRange.Min.Should().Be(1.0);
            axis.DataRange.Max.Should().Be(100.0);
            axis.VisibleRange.Min.Should().Be(1.0);
            axis.VisibleRange.Max.Should().Be(100.0);
            axis.Ticker.Should().BeOfType<LogTicker>();
            axis.NumberFormatter.Should().BeOfType<ScientificNumberFormatter>();
        }

        [Fact]
        public void LogNumericAxis_Constructor_CustomBase_InitializesCorrectly()
        {
            // Arrange & Act
            var axis = new LogNumericAxis(2.0);

            // Assert
            axis.LogBase.Should().Be(2.0);
        }

        [Fact]
        public void LogNumericAxis_Constructor_InvalidBase_ThrowsException()
        {
            // Act & Assert
            Action act1 = () => new LogNumericAxis(0);
            Action act2 = () => new LogNumericAxis(-1);
            Action act3 = () => new LogNumericAxis(1);

            act1.Should().Throw<ArgumentException>();
            act2.Should().Throw<ArgumentException>();
            act3.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetVisibleRange_ValidPositiveValues_SetsCorrectly()
        {
            // Arrange
            var axis = new LogNumericAxis();

            // Act
            axis.SetVisibleRange(10, 1000);

            // Assert
            axis.VisibleRange.Min.Should().Be(10);
            axis.VisibleRange.Max.Should().Be(1000);
        }

        [Fact]
        public void SetVisibleRange_NegativeValues_ClampsToPositive()
        {
            // Arrange
            var axis = new LogNumericAxis();

            // Act
            axis.SetVisibleRange(-10, 100);

            // Assert
            axis.VisibleRange.Min.Should().BeGreaterThan(0);
            axis.VisibleRange.Max.Should().Be(100);
        }

        [Fact]
        public void SetVisibleRange_SwappedValues_AutoCorrects()
        {
            // Arrange
            var axis = new LogNumericAxis();

            // Act
            axis.SetVisibleRange(1000, 10);

            // Assert
            axis.VisibleRange.Min.Should().Be(10);
            axis.VisibleRange.Max.Should().Be(1000);
        }

        [Fact]
        public void SetVisibleRange_TooCloseValues_Expands()
        {
            // Arrange
            var axis = new LogNumericAxis();

            // Act
            axis.SetVisibleRange(100, 100.01); // Very close values

            // Assert
            axis.VisibleRange.Max.Should().BeGreaterThan(axis.VisibleRange.Min * 1.1);
        }

        [Fact]
        public void SetVisibleRangeByPowers_ValidPowers_SetsCorrectly()
        {
            // Arrange
            var axis = new LogNumericAxis(); // Base 10

            // Act
            axis.SetVisibleRangeByPowers(0, 3); // 10^0 to 10^3 = 1 to 1000

            // Assert
            axis.VisibleRange.Min.Should().BeApproximately(1, 1e-10);
            axis.VisibleRange.Max.Should().BeApproximately(1000, 1e-10);
        }

        [Fact]
        public void GetVisibleRangePowers_ReturnsCorrectPowers()
        {
            // Arrange
            var axis = new LogNumericAxis();
            axis.SetVisibleRange(1, 1000);

            // Act
            var (minPower, maxPower) = axis.GetVisibleRangePowers();

            // Assert
            minPower.Should().BeApproximately(0, 1e-10); // log10(1) = 0
            maxPower.Should().BeApproximately(3, 1e-10); // log10(1000) = 3
        }

        [Fact]
        public void UpdateScale_CreatesCorrectLogScale()
        {
            // Arrange
            var axis = new LogNumericAxis();
            axis.SetVisibleRange(1, 100);

            // Act
            axis.UpdateScale(0, 400); // 400 pixels

            // Assert
            axis.Scale.Should().BeOfType<LogScale>();

            // Test scale conversion
            var scale = (LogScale)axis.Scale;
            scale.ToPixels(1).Should().BeApproximately(0, 1e-6);
            scale.ToPixels(100).Should().BeApproximately(400, 1e-6);
            scale.ToPixels(10).Should().BeApproximately(200, 1e-6); // Middle in log space
        }

        [Fact]
        public void IsValidValue_PositiveValues_ReturnsTrue()
        {
            // Act & Assert
            LogNumericAxis.IsValidValue(1.0).Should().BeTrue();
            LogNumericAxis.IsValidValue(0.001).Should().BeTrue();
            LogNumericAxis.IsValidValue(1000000).Should().BeTrue();
        }

        [Fact]
        public void IsValidValue_InvalidValues_ReturnsFalse()
        {
            // Act & Assert
            LogNumericAxis.IsValidValue(0).Should().BeFalse();
            LogNumericAxis.IsValidValue(-1).Should().BeFalse();
            LogNumericAxis.IsValidValue(double.NaN).Should().BeFalse();
            LogNumericAxis.IsValidValue(double.PositiveInfinity).Should().BeFalse();
            LogNumericAxis.IsValidValue(double.NegativeInfinity).Should().BeFalse();
        }

        [Fact]
        public void ClampValue_InvalidValues_ClampsToMinimum()
        {
            // Act & Assert
            LogNumericAxis.ClampValue(-10).Should().BeGreaterThan(0);
            LogNumericAxis.ClampValue(0).Should().BeGreaterThan(0);
            LogNumericAxis.ClampValue(double.NaN).Should().BeGreaterThan(0);
            LogNumericAxis.ClampValue(10).Should().Be(10); // Valid value unchanged
        }
    }
}