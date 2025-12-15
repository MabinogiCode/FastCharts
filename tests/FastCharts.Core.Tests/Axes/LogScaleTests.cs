using FastCharts.Core.Primitives;
using FastCharts.Core.Scales;
using FluentAssertions;
using System;
using Xunit;

namespace FastCharts.Core.Tests.Axes
{
    /// <summary>
    /// Tests for LogScale implementation
    /// </summary>
    public class LogScaleTests
    {
        [Fact]
        public void LogScale_Constructor_ValidParameters_InitializesCorrectly()
        {
            // Arrange & Act
            var scale = new LogScale(1, 100, 0, 400, 10);

            // Assert
            scale.LogBase.Should().Be(10);
        }

        [Fact]
        public void LogScale_Constructor_InvalidParameters_ThrowsException()
        {
            // Act & Assert
            Action act1 = () => new LogScale(0, 100, 0, 400); // Zero min
            Action act2 = () => new LogScale(-1, 100, 0, 400); // Negative min
            Action act3 = () => new LogScale(100, 1, 0, 400); // Inverted range
            Action act4 = () => new LogScale(1, 100, 0, 400, 0); // Invalid base

            act1.Should().Throw<ArgumentException>();
            act2.Should().Throw<ArgumentException>();
            act3.Should().Throw<ArgumentException>();
            act4.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ToPixels_ValidValues_ReturnsCorrectPixels()
        {
            // Arrange
            var scale = new LogScale(1, 100, 0, 400, 10);

            // Act & Assert
            scale.ToPixels(1).Should().BeApproximately(0, 1e-6);
            scale.ToPixels(100).Should().BeApproximately(400, 1e-6);
            scale.ToPixels(10).Should().BeApproximately(200, 1e-6); // log10(10) is halfway between log10(1) and log10(100)
        }

        [Fact]
        public void ToPixels_InvalidValues_HandlesGracefully()
        {
            // Arrange
            var scale = new LogScale(1, 100, 0, 400, 10);

            // Act & Assert
            scale.ToPixels(double.NaN).Should().Be(double.NaN);
            scale.ToPixels(-1).Should().BeApproximately(0, 1e-6); // Clamped to min
            scale.ToPixels(0).Should().BeApproximately(0, 1e-6); // Clamped to min
        }

        [Fact]
        public void FromPixels_ValidPixels_ReturnsCorrectValues()
        {
            // Arrange
            var scale = new LogScale(1, 100, 0, 400, 10);

            // Act & Assert
            scale.FromPixels(0).Should().BeApproximately(1, 1e-6);
            scale.FromPixels(400).Should().BeApproximately(100, 1e-6);
            scale.FromPixels(200).Should().BeApproximately(10, 1e-6);
        }

        [Fact]
        public void RoundTrip_ToPixelsFromPixels_MaintainsAccuracy()
        {
            // Arrange
            var scale = new LogScale(1, 1000, 0, 600, 10);
            var testValues = new[] { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000 };

            foreach (var value in testValues)
            {
                // Act
                var pixels = scale.ToPixels(value);
                var backToValue = scale.FromPixels(pixels);

                // Assert
                backToValue.Should().BeApproximately(value, value * 1e-10);
            }
        }
    }
}