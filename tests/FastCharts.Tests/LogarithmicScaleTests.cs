using System;
using FastCharts.Core.Scales;
using Xunit;

namespace FastCharts.Tests
{
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
}