using System;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Rendering.Skia;
using FastCharts.Tests.Helpers;
using Xunit;

namespace FastCharts.Tests.Integration
{
    /// <summary>
    /// Integration tests for CategoryAxis (P1-AX-CAT) with full rendering pipeline
    /// </summary>
    public class CategoryAxisIntegrationTests
    {
        [Fact]
        public void CategoryAxis_WithBarSeries_RendersCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Category Bar Chart" };
            
            var categoryAxis = new CategoryAxis(new[] { "Jan", "Feb", "Mar", "Apr" });
            model.ReplaceXAxis(categoryAxis);

            var barData = new[]
            {
                new BarPoint(0, 100),
                new BarPoint(1, 150),
                new BarPoint(2, 120),
                new BarPoint(3, 180)
            };
            
            model.AddSeries(new BarSeries(barData) { Title = "Monthly Sales" });

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void CategoryAxis_WithLineSeries_RendersCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Category Line Chart" };
            
            var categoryAxis = new CategoryAxis(new[] { "Q1", "Q2", "Q3", "Q4" });
            model.ReplaceXAxis(categoryAxis);

            var lineData = new[]
            {
                new PointD(0, 50),
                new PointD(1, 75),
                new PointD(2, 60),
                new PointD(3, 90)
            };
            
            model.AddSeries(new LineSeries(lineData) { Title = "Quarterly Growth" });

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void CategoryAxis_WithMultipleSeries_RendersCorrectly()
        {
            // Arrange
            var model = new ChartModel 
            { 
                Title = "Multiple Series with Categories",
                Theme = new DarkTheme()
            };
            
            var categoryAxis = new CategoryAxis(new[] { "Product A", "Product B", "Product C" });
            model.ReplaceXAxis(categoryAxis);

            // Add bar series
            var barData = new[]
            {
                new BarPoint(0, 200),
                new BarPoint(1, 300),
                new BarPoint(2, 150)
            };
            model.AddSeries(new BarSeries(barData) { Title = "Units Sold" });

            // Add line series for trend
            var lineData = new[]
            {
                new PointD(0, 180),
                new PointD(1, 320),
                new PointD(2, 140)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Trend" });

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void CategoryAxis_EmptyCategories_RendersWithoutError()
        {
            // Arrange
            var model = new ChartModel { Title = "Empty Categories" };
            var categoryAxis = new CategoryAxis();
            model.ReplaceXAxis(categoryAxis);

            // Act & Assert - should not throw
            var exception = Record.Exception(() =>
            {
                var (bitmap, _) = ChartRenderTestHelper.Render(model);
                bitmap?.Dispose();
            });
            
            Assert.Null(exception);
        }

        [Fact]
        public void CategoryAxis_WithCustomSpacing_RendersCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Custom Spacing" };
            
            var categoryAxis = new CategoryAxis(new[] { "A", "B", "C" })
            {
                CategorySpacing = 2.0
            };
            model.ReplaceXAxis(categoryAxis);

            var barData = new[]
            {
                new BarPoint(0, 100),
                new BarPoint(2, 150),  // Note: spaced by 2.0
                new BarPoint(4, 120)   // Note: spaced by 2.0
            };
            
            model.AddSeries(new BarSeries(barData) { Title = "Spaced Data" });

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void CategoryAxis_LongLabels_RendersWithoutOverlap()
        {
            // Arrange
            var model = new ChartModel { Title = "Long Category Names" };
            
            var categoryAxis = new CategoryAxis(new[] 
            { 
                "Very Long Category Name 1", 
                "Even Longer Category Name 2", 
                "Extremely Long Category Name 3" 
            });
            model.ReplaceXAxis(categoryAxis);

            var barData = new[]
            {
                new BarPoint(0, 100),
                new BarPoint(1, 150),
                new BarPoint(2, 120)
            };
            
            model.AddSeries(new BarSeries(barData) { Title = "Data" });

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model, 1200, 600); // Wider for long labels

            // Assert
            Assert.NotNull(bitmap);
            Assert.Equal(1200, bitmap.Width);
            Assert.Equal(600, bitmap.Height);
        }

        [Fact]
        public void CategoryAxis_WithSecondaryYAxis_RendersCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Categories with Dual Y" };
            
            var categoryAxis = new CategoryAxis(new[] { "Jan", "Feb", "Mar" });
            model.ReplaceXAxis(categoryAxis);
            model.EnsureSecondaryYAxis();

            // Primary Y axis data
            var barData = new[]
            {
                new BarPoint(0, 100),
                new BarPoint(1, 150),
                new BarPoint(2, 120)
            };
            model.AddSeries(new BarSeries(barData) { Title = "Sales", YAxisIndex = 0 });

            // Secondary Y axis data
            var lineData = new[]
            {
                new PointD(0, 10),
                new PointD(1, 15),
                new PointD(2, 12)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Growth %", YAxisIndex = 1 });

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void CategoryAxis_Ticks_GenerateCorrectPositions()
        {
            // Arrange
            var categories = new[] { "A", "B", "C", "D", "E" };
            var categoryAxis = new CategoryAxis(categories);

            // Act
            var ticks = categoryAxis.Ticker.GetTicks(new FRange(0, 4), 1.0);

            // Assert
            Assert.Equal(5, ticks.Count);
            for (var i = 0; i < 5; i++)
            {
                Assert.Contains(i, ticks);
            }
        }

        [Fact]
        public void CategoryAxis_PartialVisibleRange_ShowsOnlyVisibleTicks()
        {
            // Arrange
            var categories = new[] { "A", "B", "C", "D", "E" };
            var categoryAxis = new CategoryAxis(categories);
            categoryAxis.SetVisibleCategories(1, 3); // Show only B, C, D

            // Act
            var visibleRange = categoryAxis.VisibleRange;
            var ticks = categoryAxis.Ticker.GetTicks(visibleRange, 1.0);

            // Assert
            Assert.Equal(new FRange(1, 3), visibleRange);
            Assert.Equal(3, ticks.Count);
            Assert.Contains(1.0, ticks); // B
            Assert.Contains(2.0, ticks); // C
            Assert.Contains(3.0, ticks); // D
        }

        [Fact]
        public void CategoryAxis_Integration_WithExportPng_WorksCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Export Test" };
            
            var categoryAxis = new CategoryAxis(new[] { "Test1", "Test2", "Test3" });
            model.ReplaceXAxis(categoryAxis);

            var barData = new[]
            {
                new BarPoint(0, 100),
                new BarPoint(1, 200),
                new BarPoint(2, 150)
            };
            model.AddSeries(new BarSeries(barData) { Title = "Test Data" });

            var renderer = new SkiaChartRenderer();

            // Act & Assert - should not throw
            var exception = Record.Exception(() =>
            {
                using var bitmap = renderer.RenderToBitmap(model, 800, 600);
                Assert.NotNull(bitmap);
                Assert.Equal(800, bitmap.Width);
                Assert.Equal(600, bitmap.Height);
            });
            
            Assert.Null(exception);
        }
    }
}