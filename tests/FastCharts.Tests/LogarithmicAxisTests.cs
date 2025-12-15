using System;
using System.Linq;
using FastCharts.Core.Axes;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Primitives;
using FastCharts.Core.Scales;
using Xunit;

namespace FastCharts.Tests
{
    public class LogarithmicAxisTests
    {
        [Fact]
        public void LogarithmicAxis_DefaultConstructor_HasValidDefaults()
        {
            // Arrange & Act
            var axis = new LogarithmicAxis();

            // Assert
            Assert.Equal(10.0, axis.LogBase);
            Assert.Equal(1.0, axis.DataRange.Min);
            Assert.Equal(10.0, axis.DataRange.Max);
            Assert.Equal(1.0, axis.VisibleRange.Min);
            Assert.Equal(10.0, axis.VisibleRange.Max);
            Assert.IsType<LogarithmicTicker>(axis.Ticker);
            Assert.NotNull(axis.NumberFormatter);
        }

        [Theory]
        [InlineData(2.0)]
        [InlineData(Math.E)]
        [InlineData(10.0)]
        public void LogarithmicAxis_SetLogBase_UpdatesBaseCorrectly(double logBase)
        {
            // Arrange
            var axis = new LogarithmicAxis();

            // Act
            axis.LogBase = logBase;

            // Assert
            Assert.Equal(logBase, axis.LogBase);
            Assert.IsType<LogarithmicTicker>(axis.Ticker);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void LogarithmicAxis_SetInvalidLogBase_ThrowsArgumentException(double invalidBase)
        {
            // Arrange
            var axis = new LogarithmicAxis();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => axis.LogBase = invalidBase);
        }

        [Fact]
        public void LogarithmicAxis_SetVisibleRange_HandlesPositiveValues()
        {
            // Arrange
            var axis = new LogarithmicAxis();

            // Act
            axis.SetVisibleRange(1.0, 100.0);

            // Assert
            Assert.Equal(1.0, axis.VisibleRange.Min);
            Assert.Equal(100.0, axis.VisibleRange.Max);
        }

        [Fact]
        public void LogarithmicAxis_SetVisibleRange_HandlesNegativeValues()
        {
            // Arrange
            var axis = new LogarithmicAxis();

            // Act
            axis.SetVisibleRange(-10.0, 100.0);

            // Assert
            Assert.True(axis.VisibleRange.Min > 0); // Should be corrected to positive
            Assert.Equal(100.0, axis.VisibleRange.Max);
        }

        [Fact]
        public void LogarithmicAxis_SetVisibleRange_HandlesSwappedValues()
        {
            // Arrange
            var axis = new LogarithmicAxis();

            // Act
            axis.SetVisibleRange(100.0, 1.0);

            // Assert
            Assert.Equal(1.0, axis.VisibleRange.Min);
            Assert.Equal(100.0, axis.VisibleRange.Max);
        }

        [Fact]
        public void LogarithmicAxis_UpdateScale_CreatesValidScale()
        {
            // Arrange
            var axis = new LogarithmicAxis();
            axis.SetVisibleRange(1.0, 100.0);

            // Act
            axis.UpdateScale(0.0, 400.0);

            // Assert
            Assert.IsType<LogarithmicScale>(axis.Scale);
            var logScale = (LogarithmicScale)axis.Scale;
            Assert.Equal(1.0, logScale.DataMin);
            Assert.Equal(100.0, logScale.DataMax);
            Assert.Equal(0.0, logScale.PixelMin);
            Assert.Equal(400.0, logScale.PixelMax);
        }
    }
}