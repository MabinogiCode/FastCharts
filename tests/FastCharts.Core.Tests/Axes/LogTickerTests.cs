using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Primitives;
using FluentAssertions;
using System;
using Xunit;

namespace FastCharts.Core.Tests.Axes
{
    /// <summary>
    /// Tests for LogTicker implementation
    /// </summary>
    public class LogTickerTests
    {
        [Fact]
        public void LogTicker_Constructor_DefaultBase10_InitializesCorrectly()
        {
            // Arrange & Act
            var ticker = new LogTicker();

            // Assert
            ticker.LogBase.Should().Be(10.0);
        }

        [Fact]
        public void LogTicker_Constructor_InvalidBase_ThrowsException()
        {
            // Act & Assert
            Action act = () => new LogTicker(0);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetTicks_ValidRange_ReturnsCorrectTicks()
        {
            // Arrange
            var ticker = new LogTicker();
            var range = new FRange(1, 1000); // 10^0 to 10^3

            // Act
            var ticks = ticker.GetTicks(range, 1);

            // Assert
            ticks.Should().NotBeEmpty();
            ticks.Should().Contain(1);    // 10^0
            ticks.Should().Contain(10);   // 10^1
            ticks.Should().Contain(100);  // 10^2
            ticks.Should().Contain(1000); // 10^3
            ticks.Should().BeInAscendingOrder();
        }

        [Fact]
        public void GetTicks_LargeRange_LimitsDensity()
        {
            // Arrange
            var ticker = new LogTicker();
            var range = new FRange(1e-10, 1e10); // 20 decades

            // Act
            var ticks = ticker.GetTicks(range, 1);

            // Assert
            ticks.Should().NotBeEmpty();
            ticks.Count.Should().BeLessThanOrEqualTo(15); // Should limit density
        }

        [Fact]
        public void GetTicks_InvalidRange_ReturnsEmpty()
        {
            // Arrange
            var ticker = new LogTicker();

            // Act & Assert
            ticker.GetTicks(new FRange(-1, 10), 1).Should().BeEmpty(); // Negative min
            ticker.GetTicks(new FRange(0, 10), 1).Should().BeEmpty();  // Zero min
            ticker.GetTicks(new FRange(10, 1), 1).Should().BeEmpty();  // Inverted range
        }

        [Fact]
        public void GetMinorTicks_ValidMajorTicks_ReturnsIntermediateValues()
        {
            // Arrange
            var ticker = new LogTicker();
            var range = new FRange(1, 100);
            var majorTicks = new[] { 1.0, 10.0, 100.0 };

            // Act
            var minorTicks = ticker.GetMinorTicks(range, majorTicks);

            // Assert
            minorTicks.Should().NotBeEmpty();
            minorTicks.Should().Contain(2);  // 2 × 10^0
            minorTicks.Should().Contain(5);  // 5 × 10^0
            minorTicks.Should().Contain(20); // 2 × 10^1
            minorTicks.Should().Contain(50); // 5 × 10^1
            minorTicks.Should().NotContain(majorTicks); // Should not duplicate major ticks
            minorTicks.Should().BeInAscendingOrder();
        }

        [Fact]
        public void GetMinorTicks_Base2_ReturnsAppropriateMinors()
        {
            // Arrange
            var ticker = new LogTicker(2.0);
            var range = new FRange(1, 8);
            var majorTicks = new[] { 1.0, 2.0, 4.0, 8.0 };

            // Act
            var minorTicks = ticker.GetMinorTicks(range, majorTicks);

            // Assert
            minorTicks.Should().NotBeEmpty();
            minorTicks.Should().Contain(1.5); // Between 1 and 2
            minorTicks.Should().Contain(3.0); // Between 2 and 4
            minorTicks.Should().Contain(6.0); // Between 4 and 8
        }
    }
}