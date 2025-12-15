using System;
using System.Linq;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using Xunit;

namespace FastCharts.Core.Tests.Axes
{
    /// <summary>
    /// Tests for CategoryAxis implementation (P1-AX-CAT)
    /// </summary>
    public class CategoryAxisTests
    {
        [Fact]
        public void CategoryAxis_Constructor_WithoutCategories_InitializesEmpty()
        {
            // Arrange & Act
            var axis = new CategoryAxis();

            // Assert
            Assert.Empty(axis.Categories);
            Assert.Equal(new FRange(0, 0), axis.DataRange);
            Assert.Equal(new FRange(0, 0), axis.VisibleRange);
            Assert.Equal(1.0, axis.CategorySpacing);
        }

        [Fact]
        public void CategoryAxis_Constructor_WithCategories_InitializesCorrectly()
        {
            // Arrange
            var categories = new[] { "Jan", "Feb", "Mar", "Apr" };

            // Act
            var axis = new CategoryAxis(categories);

            // Assert
            Assert.Equal(4, axis.Categories.Count);
            Assert.Equal(categories, axis.Categories);
            Assert.Equal(new FRange(0, 3), axis.DataRange);
            Assert.Equal(new FRange(0, 3), axis.VisibleRange);
        }

        [Fact]
        public void CategoryAxis_Constructor_WithNullOrEmptyCategories_FiltersCorrectly()
        {
            // Arrange
            var categories = new string?[] { "Jan", null, "", "   ", "Feb" };

            // Act
            var axis = new CategoryAxis(categories.OfType<string>());

            // Assert
            Assert.Equal(2, axis.Categories.Count);
            Assert.Equal("Jan", axis.Categories[0]);
            Assert.Equal("Feb", axis.Categories[1]);
        }

        [Fact]
        public void CategoryAxis_AddCategory_ValidCategory_AddsSuccessfully()
        {
            // Arrange
            var axis = new CategoryAxis();

            // Act
            axis.AddCategory("Q1");
            axis.AddCategory("Q2");

            // Assert
            Assert.Equal(2, axis.Categories.Count);
            Assert.Equal("Q1", axis.Categories[0]);
            Assert.Equal("Q2", axis.Categories[1]);
            Assert.Equal(new FRange(0, 1), axis.DataRange);
        }

        [Fact]
        public void CategoryAxis_AddCategory_InvalidCategory_DoesNotAdd()
        {
            // Arrange
            var axis = new CategoryAxis();

            // Act
            axis.AddCategory("");
            axis.AddCategory("  ");
            // Note: null is now handled by nullable parameter

            // Assert
            Assert.Empty(axis.Categories);
            Assert.Equal(new FRange(0, 0), axis.DataRange);
        }

        [Fact]
        public void CategoryAxis_AddCategories_ValidCategories_AddsAll()
        {
            // Arrange
            var axis = new CategoryAxis();
            var categories = new[] { "A", "B", "C" };

            // Act
            axis.AddCategories(categories);

            // Assert
            Assert.Equal(3, axis.Categories.Count);
            Assert.Equal(categories, axis.Categories);
        }

        [Fact]
        public void CategoryAxis_SetCategories_ReplacesExisting()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "Old1", "Old2" });
            var newCategories = new[] { "New1", "New2", "New3" };

            // Act
            axis.SetCategories(newCategories);

            // Assert
            Assert.Equal(3, axis.Categories.Count);
            Assert.Equal(newCategories, axis.Categories);
            Assert.Equal(new FRange(0, 2), axis.DataRange);
        }

        [Fact]
        public void CategoryAxis_ClearCategories_RemovesAll()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });

            // Act
            axis.ClearCategories();

            // Assert
            Assert.Empty(axis.Categories);
            Assert.Equal(new FRange(0, 0), axis.DataRange);
        }

        [Fact]
        public void CategoryAxis_GetCategoryPosition_ValidCategory_ReturnsCorrectPosition()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "Jan", "Feb", "Mar" });

            // Act & Assert
            Assert.Equal(0.0, axis.GetCategoryPosition("Jan"));
            Assert.Equal(1.0, axis.GetCategoryPosition("Feb"));
            Assert.Equal(2.0, axis.GetCategoryPosition("Mar"));
        }

        [Fact]
        public void CategoryAxis_GetCategoryPosition_InvalidCategory_ReturnsMinusOne()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "Jan", "Feb", "Mar" });

            // Act & Assert
            Assert.Equal(-1.0, axis.GetCategoryPosition("NotFound"));
            Assert.Equal(-1.0, axis.GetCategoryPosition(""));
        }

        [Fact]
        public void CategoryAxis_GetCategoryAt_ValidPosition_ReturnsCategory()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });

            // Act & Assert
            Assert.Equal("A", axis.GetCategoryAt(0.0));
            Assert.Equal("B", axis.GetCategoryAt(1.0));
            Assert.Equal("C", axis.GetCategoryAt(2.0));
        }

        [Fact]
        public void CategoryAxis_GetCategoryAt_InvalidPosition_ReturnsNull()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });

            // Act & Assert
            Assert.Null(axis.GetCategoryAt(-1.0));
            Assert.Null(axis.GetCategoryAt(3.0));
            Assert.Null(axis.GetCategoryAt(10.0));
        }

        [Fact]
        public void CategoryAxis_GetCategoryAt_RoundingPosition_ReturnsNearestCategory()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });

            // Act & Assert
            Assert.Equal("A", axis.GetCategoryAt(0.1));
            Assert.Equal("A", axis.GetCategoryAt(0.4));
            Assert.Equal("B", axis.GetCategoryAt(0.6));
            Assert.Equal("B", axis.GetCategoryAt(1.4));
            Assert.Equal("C", axis.GetCategoryAt(1.9));
        }

        [Fact]
        public void CategoryAxis_GetCategoryIndex_ValidPosition_ReturnsIndex()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "X", "Y", "Z" });

            // Act & Assert
            Assert.Equal(0, axis.GetCategoryIndex(0.0));
            Assert.Equal(1, axis.GetCategoryIndex(1.0));
            Assert.Equal(2, axis.GetCategoryIndex(2.0));
        }

        [Fact]
        public void CategoryAxis_GetCategoryIndex_InvalidPosition_ReturnsMinusOne()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "X", "Y", "Z" });

            // Act & Assert
            Assert.Equal(-1, axis.GetCategoryIndex(-1.0));
            Assert.Equal(-1, axis.GetCategoryIndex(3.0));
        }

        [Fact]
        public void CategoryAxis_SetVisibleCategories_ValidIndices_UpdatesVisibleRange()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C", "D", "E" });

            // Act
            axis.SetVisibleCategories(1, 3);

            // Assert
            Assert.Equal(new FRange(1.0, 3.0), axis.VisibleRange);
        }

        [Fact]
        public void CategoryAxis_SetVisibleCategories_OutOfBounds_ClampsToBounds()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });

            // Act
            axis.SetVisibleCategories(-5, 10);

            // Assert
            Assert.Equal(new FRange(0.0, 2.0), axis.VisibleRange);
        }

        [Fact]
        public void CategoryAxis_SetVisibleRange_ValidRange_UpdatesVisibleRange()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C", "D" });

            // Act
            axis.SetVisibleRange(0.5, 2.5);

            // Assert
            Assert.Equal(new FRange(0.5, 2.5), axis.VisibleRange);
        }

        [Fact]
        public void CategoryAxis_SetVisibleRange_OutOfBounds_ClampsToBounds()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });

            // Act
            axis.SetVisibleRange(-1.0, 10.0);

            // Assert
            Assert.Equal(new FRange(0.0, 2.0), axis.VisibleRange);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(10.0)]
        public void CategoryAxis_CategorySpacing_DifferentValues_UpdatesCorrectly(double spacing)
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });

            // Act
            axis.CategorySpacing = spacing;

            // Assert
            Assert.Equal(spacing, axis.CategorySpacing);
        }

        [Fact]
        public void CategoryAxis_UpdateScale_UpdatesInternalScale()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });
            axis.VisibleRange = new FRange(0, 2);

            // Act
            axis.UpdateScale(0, 100);

            // Assert
            Assert.NotNull(axis.Scale);
            // Scale should map data range 0-2 to pixel range 0-100
        }

        [Fact]
        public void CategoryAxis_Ticker_ReturnsValidTicker()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });

            // Act
            var ticker = axis.Ticker;

            // Assert
            Assert.NotNull(ticker);

            // Test ticker functionality
            var ticks = ticker.GetTicks(new FRange(0, 2), 1.0);
            Assert.Equal(3, ticks.Count);
            Assert.Contains(0.0, ticks);
            Assert.Contains(1.0, ticks);
            Assert.Contains(2.0, ticks);
        }

        [Fact]
        public void CategoryAxis_Ticker_MinorTicks_ReturnsEmpty()
        {
            // Arrange
            var axis = new CategoryAxis(new[] { "A", "B", "C" });

            // Act
            var minorTicks = axis.Ticker.GetMinorTicks(new FRange(0, 2), new[] { 0.0, 1.0, 2.0 });

            // Assert
            Assert.Empty(minorTicks);
        }
    }
}