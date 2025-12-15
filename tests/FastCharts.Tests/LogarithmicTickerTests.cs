using System;
using System.Linq;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Primitives;
using Xunit;

namespace FastCharts.Tests
{
    public class LogarithmicTickerTests
    {
        [Fact]
        public void LogarithmicTicker_GetTicks_Base10_ReturnsExpectedTicks()
        {
            // Arrange
            var ticker = new LogarithmicTicker(10.0);
            var range = new FRange(1.0, 1000.0);

            // Act
            var ticks = ticker.GetTicks(range, 1.0);

            // Assert
            Assert.Contains(1.0, ticks);
            Assert.Contains(10.0, ticks);
            Assert.Contains(100.0, ticks);
            Assert.Contains(1000.0, ticks);
            Assert.True(ticks.Count >= 4);
        }

        [Fact]
        public void LogarithmicTicker_GetMinorTicks_Base10_ReturnsSubdivisions()
        {
            // Arrange
            var ticker = new LogarithmicTicker(10.0);
            var range = new FRange(1.0, 100.0);
            var majorTicks = new[] { 1.0, 10.0, 100.0 };

            // Act
            var minorTicks = ticker.GetMinorTicks(range, majorTicks);

            // Assert
            Assert.Contains(2.0, minorTicks);
            Assert.Contains(5.0, minorTicks);
            Assert.Contains(20.0, minorTicks);
            Assert.Contains(50.0, minorTicks);
            Assert.DoesNotContain(1.0, minorTicks); // Should not duplicate major ticks
            Assert.DoesNotContain(10.0, minorTicks);
        }

        [Fact]
        public void LogarithmicTicker_GetTicks_WithNegativeRange_ReturnsEmpty()
        {
            // Arrange
            var ticker = new LogarithmicTicker(10.0);
            var range = new FRange(-10.0, -1.0);

            // Act
            var ticks = ticker.GetTicks(range, 1.0);

            // Assert
            Assert.Empty(ticks);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        public void LogarithmicTicker_Constructor_WithInvalidBase_ThrowsArgumentException(double invalidBase)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new LogarithmicTicker(invalidBase));
        }
    }
}