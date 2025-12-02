using System.Collections.Generic;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Primitives;
using Xunit;

namespace FastCharts.Core.Tests.Axes.Ticks
{
    /// <summary>
    /// Tests for CategoryTicker implementation
    /// </summary>
    public class CategoryTickerTests
    {
        [Fact]
        public void CategoryTicker_Constructor_WithCategories_InitializesCorrectly()
        {
            // Arrange
            var categories = new List<string> { "A", "B", "C" };

            // Act
            var ticker = new CategoryTicker(categories);

            // Assert
            Assert.NotNull(ticker);
        }

        [Fact]
        public void CategoryTicker_Constructor_WithNull_HandlesGracefully()
        {
            // Arrange & Act
            var ticker = new CategoryTicker((List<string>?)null);

            // Assert
            Assert.NotNull(ticker);
            var ticks = ticker.GetTicks(new FRange(0, 1), 1.0);
            Assert.Empty(ticks);
        }

        [Fact]
        public void CategoryTicker_GetTicks_WithValidRange_ReturnsCorrectTicks()
        {
            // Arrange
            var categories = new List<string> { "Jan", "Feb", "Mar", "Apr", "May" };
            var ticker = new CategoryTicker(categories);

            // Act
            var ticks = ticker.GetTicks(new FRange(0, 4), 1.0);

            // Assert
            Assert.Equal(5, ticks.Count);
            Assert.Contains(0.0, ticks);
            Assert.Contains(1.0, ticks);
            Assert.Contains(2.0, ticks);
            Assert.Contains(3.0, ticks);
            Assert.Contains(4.0, ticks);
        }

        [Fact]
        public void CategoryTicker_GetTicks_WithPartialRange_ReturnsVisibleTicks()
        {
            // Arrange
            var categories = new List<string> { "A", "B", "C", "D", "E" };
            var ticker = new CategoryTicker(categories);

            // Act
            var ticks = ticker.GetTicks(new FRange(1.5, 3.5), 1.0);

            // Assert
            Assert.Equal(2, ticks.Count);
            Assert.Contains(2.0, ticks);
            Assert.Contains(3.0, ticks);
        }

        [Fact]
        public void CategoryTicker_GetTicks_WithCustomSpacing_ReturnsCorrectPositions()
        {
            // Arrange
            var categories = new List<string> { "X", "Y", "Z" };
            var ticker = new CategoryTicker(categories, 2.0);

            // Act
            var ticks = ticker.GetTicks(new FRange(0, 4), 1.0);

            // Assert
            Assert.Equal(3, ticks.Count);
            Assert.Contains(0.0, ticks);
            Assert.Contains(2.0, ticks);
            Assert.Contains(4.0, ticks);
        }

        [Fact]
        public void CategoryTicker_GetTicks_WithEmptyCategories_ReturnsEmpty()
        {
            // Arrange
            var categories = new List<string>();
            var ticker = new CategoryTicker(categories);

            // Act
            var ticks = ticker.GetTicks(new FRange(0, 10), 1.0);

            // Assert
            Assert.Empty(ticks);
        }

        [Fact]
        public void CategoryTicker_GetTicks_WithZeroRange_ReturnsEmpty()
        {
            // Arrange
            var categories = new List<string> { "A", "B", "C" };
            var ticker = new CategoryTicker(categories);

            // Act
            var ticks = ticker.GetTicks(new FRange(5, 5), 1.0);

            // Assert
            Assert.Empty(ticks);
        }

        [Fact]
        public void CategoryTicker_GetTicks_WithNegativeRange_HandlesCorrectly()
        {
            // Arrange
            var categories = new List<string> { "A", "B", "C" };
            var ticker = new CategoryTicker(categories);

            // Act
            var ticks = ticker.GetTicks(new FRange(-2, -1), 1.0);

            // Assert
            Assert.Empty(ticks); // No categories in negative range
        }

        [Fact]
        public void CategoryTicker_GetTicks_WithRangeExceedingCategories_ReturnsOnlyValidTicks()
        {
            // Arrange
            var categories = new List<string> { "First", "Second" };
            var ticker = new CategoryTicker(categories);

            // Act
            var ticks = ticker.GetTicks(new FRange(0, 10), 1.0);

            // Assert
            Assert.Equal(2, ticks.Count);
            Assert.Contains(0.0, ticks);
            Assert.Contains(1.0, ticks);
        }

        [Fact]
        public void CategoryTicker_GetMinorTicks_ReturnsEmpty()
        {
            // Arrange
            var categories = new List<string> { "A", "B", "C" };
            var ticker = new CategoryTicker(categories);
            var majorTicks = new List<double> { 0.0, 1.0, 2.0 };

            // Act
            var minorTicks = ticker.GetMinorTicks(new FRange(0, 2), majorTicks);

            // Assert
            Assert.Empty(minorTicks);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(5.0)]
        public void CategoryTicker_Constructor_WithDifferentSpacings_HandlesCorrectly(double spacing)
        {
            // Arrange
            var categories = new List<string> { "A", "B" };

            // Act
            var ticker = new CategoryTicker(categories, spacing);
            var ticks = ticker.GetTicks(new FRange(0, spacing * 2), 1.0);

            // Assert
            Assert.Equal(2, ticks.Count);
            Assert.Contains(0.0, ticks);
            Assert.Contains(spacing, ticks);
        }

        [Fact]
        public void CategoryTicker_Constructor_WithZeroSpacing_UsesMinimumSpacing()
        {
            // Arrange
            var categories = new List<string> { "A", "B" };

            // Act
            var ticker = new CategoryTicker(categories, 0.0);
            var ticks = ticker.GetTicks(new FRange(0, 1), 1.0);

            // Assert
            // Should use minimum spacing (0.1) instead of 0
            Assert.NotEmpty(ticks);
        }

        [Fact]
        public void CategoryTicker_Constructor_WithNegativeSpacing_UsesMinimumSpacing()
        {
            // Arrange
            var categories = new List<string> { "A", "B" };

            // Act
            var ticker = new CategoryTicker(categories, -1.0);
            var ticks = ticker.GetTicks(new FRange(0, 1), 1.0);

            // Assert
            // Should use minimum spacing (0.1) instead of negative value
            Assert.NotEmpty(ticks);
        }
    }
}