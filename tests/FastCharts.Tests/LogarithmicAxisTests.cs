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

    public class LogarithmicScaleTests
    {
        [Fact]
        public void LogarithmicScale_Constructor_WithValidValues_Succeeds()
        {
            // Act
            var scale = new LogarithmicScale(1.0, 100.0, 0.0, 400.0, 10.0);

            // Assert
            Assert.Equal(1.0, scale.DataMin);
            Assert.Equal(100.0, scale.DataMax);
            Assert.Equal(0.0, scale.PixelMin);
            Assert.Equal(400.0, scale.PixelMax);
        }

        [Theory]
        [InlineData(0.0, 100.0)]
        [InlineData(-1.0, 100.0)]
        [InlineData(1.0, 0.0)]
        [InlineData(1.0, -1.0)]
        public void LogarithmicScale_Constructor_WithInvalidDataRange_ThrowsArgumentException(double dataMin, double dataMax)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new LogarithmicScale(dataMin, dataMax, 0.0, 400.0, 10.0));
        }

        [Theory]
        [InlineData(1.0, 0.0)] // 10^0 = 1
        [InlineData(10.0, 200.0)] // 10^1 = 10
        [InlineData(100.0, 400.0)] // 10^2 = 100
        public void LogarithmicScale_ToPixels_Base10_MapsCorrectly(double dataValue, double expectedPixel)
        {
            // Arrange
            var scale = new LogarithmicScale(1.0, 100.0, 0.0, 400.0, 10.0);

            // Act
            var result = scale.ToPixels(dataValue);

            // Assert
            Assert.Equal(expectedPixel, result, 1e-10);
        }

        [Theory]
        [InlineData(0.0, 1.0)] // 10^0 = 1
        [InlineData(200.0, 10.0)] // 10^1 = 10  
        [InlineData(400.0, 100.0)] // 10^2 = 100
        public void LogarithmicScale_FromPixels_Base10_MapsCorrectly(double pixelValue, double expectedData)
        {
            // Arrange
            var scale = new LogarithmicScale(1.0, 100.0, 0.0, 400.0, 10.0);

            // Act
            var result = scale.FromPixels(pixelValue);

            // Assert
            Assert.Equal(expectedData, result, 1e-10);
        }

        [Fact]
        public void LogarithmicScale_ToPixels_WithNegativeValue_ReturnsPixelMin()
        {
            // Arrange
            var scale = new LogarithmicScale(1.0, 100.0, 0.0, 400.0, 10.0);

            // Act
            var result = scale.ToPixels(-5.0);

            // Assert
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void LogarithmicScale_ClampData_ClampsToValidRange()
        {
            // Arrange
            var scale = new LogarithmicScale(1.0, 100.0, 0.0, 400.0, 10.0);

            // Act & Assert
            Assert.Equal(1.0, scale.ClampData(0.5));
            Assert.Equal(50.0, scale.ClampData(50.0));
            Assert.Equal(100.0, scale.ClampData(200.0));
        }
    }

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