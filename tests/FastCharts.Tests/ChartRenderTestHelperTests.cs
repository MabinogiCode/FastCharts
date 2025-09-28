using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Tests.Helpers;
using FluentAssertions;
using SkiaSharp;
using System;
using System.Linq;
using Xunit;

namespace FastCharts.Tests;

public class ChartRenderTestHelperTests
{
    [Fact]
    public void Render_ShouldReturnBitmapAndModel_WithDefaultDimensions()
    {
        // Arrange
        var model = new ChartModel();

        // Act
        var (bitmap, returnedModel) = ChartRenderTestHelper.Render(model);

        // Assert
        bitmap.Should().NotBeNull();
        bitmap.Width.Should().Be(420); // Default width
        bitmap.Height.Should().Be(300); // Default height
        returnedModel.Should().BeSameAs(model);
        
        bitmap.Dispose();
    }

    [Theory]
    [InlineData(100, 50)]
    [InlineData(800, 600)]
    [InlineData(1920, 1080)]
    public void Render_ShouldReturnBitmapWithSpecifiedDimensions(int width, int height)
    {
        // Arrange
        var model = new ChartModel();

        // Act
        var (bitmap, returnedModel) = ChartRenderTestHelper.Render(model, width, height);

        // Assert
        bitmap.Should().NotBeNull();
        bitmap.Width.Should().Be(width);
        bitmap.Height.Should().Be(height);
        returnedModel.Should().BeSameAs(model);
        
        bitmap.Dispose();
    }

    [Fact]
    public void Render_ShouldProduceValidBitmap_WithChartContent()
    {
        // Arrange
        var model = new ChartModel();
        var points = new[]
        {
            new PointD(0, 0),
            new PointD(1, 1),
            new PointD(2, 0)
        };
        model.AddSeries(new LineSeries(points));

        // Act
        var (bitmap, _) = ChartRenderTestHelper.Render(model);

        // Assert
        bitmap.Should().NotBeNull();
        
        // Verify bitmap contains some non-transparent pixels (indicating content was rendered)
        var hasContent = false;
        for (var y = 0; y < bitmap.Height && !hasContent; y++)
        {
            for (var x = 0; x < bitmap.Width && !hasContent; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel.Alpha > 0)
                {
                    hasContent = true;
                }
            }
        }
        hasContent.Should().BeTrue("Rendered bitmap should contain visible content");
        
        bitmap.Dispose();
    }

    [Fact]
    public void CountDifferingPixels_ShouldReturnZero_ForIdenticalBitmaps()
    {
        // Arrange
        using var bitmap1 = new SKBitmap(10, 10);
        using var bitmap2 = new SKBitmap(10, 10);
        
        // Fill both with same color
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                bitmap1.SetPixel(x, y, SKColors.Red);
                bitmap2.SetPixel(x, y, SKColors.Red);
            }
        }

        // Act
        var result = ChartRenderTestHelper.CountDifferingPixels(bitmap1, bitmap2);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CountDifferingPixels_ShouldReturnCorrectCount_ForDifferentBitmaps()
    {
        // Arrange
        using var bitmap1 = new SKBitmap(10, 10);
        using var bitmap2 = new SKBitmap(10, 10);
        
        // Fill bitmap1 with red, bitmap2 with blue
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                bitmap1.SetPixel(x, y, SKColors.Red);
                bitmap2.SetPixel(x, y, SKColors.Blue);
            }
        }

        // Act
        var result = ChartRenderTestHelper.CountDifferingPixels(bitmap1, bitmap2);

        // Assert
        result.Should().Be(100); // All 100 pixels differ (10x10)
    }

    [Fact]
    public void CountDifferingPixels_ShouldHandleDifferentSizedBitmaps()
    {
        // Arrange
        using var bitmap1 = new SKBitmap(10, 10); // 100 pixels
        using var bitmap2 = new SKBitmap(5, 8);   // 40 pixels
        
        // Fill with different colors
        for (var y = 0; y < bitmap1.Height; y++)
        {
            for (var x = 0; x < bitmap1.Width; x++)
            {
                bitmap1.SetPixel(x, y, SKColors.Red);
            }
        }
        
        for (var y = 0; y < bitmap2.Height; y++)
        {
            for (var x = 0; x < bitmap2.Width; x++)
            {
                bitmap2.SetPixel(x, y, SKColors.Blue);
            }
        }

        // Act
        var result = ChartRenderTestHelper.CountDifferingPixels(bitmap1, bitmap2);

        // Assert
        result.Should().Be(40); // Should compare only the overlapping area (5x8 = 40)
    }

    [Fact]
    public void CountDifferingPixels_ShouldCountPartialDifferences()
    {
        // Arrange
        using var bitmap1 = new SKBitmap(10, 10);
        using var bitmap2 = new SKBitmap(10, 10);
        
        // Fill both with red initially
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                bitmap1.SetPixel(x, y, SKColors.Red);
                bitmap2.SetPixel(x, y, SKColors.Red);
            }
        }
        
        // Change a few pixels in bitmap2
        bitmap2.SetPixel(0, 0, SKColors.Blue);
        bitmap2.SetPixel(5, 5, SKColors.Green);
        bitmap2.SetPixel(9, 9, SKColors.Yellow);

        // Act
        var result = ChartRenderTestHelper.CountDifferingPixels(bitmap1, bitmap2);

        // Assert
        result.Should().Be(3); // Only 3 pixels differ
    }

    [Fact]
    public void SetVisibleRange_ShouldSetBothAxesCorrectly()
    {
        // Arrange
        var model = new ChartModel();

        // Act
        ChartRenderTestHelper.SetVisibleRange(model, 10.0, 20.0, 30.0, 40.0);

        // Assert
        model.XAxis.VisibleRange.Min.Should().Be(10.0);
        model.XAxis.VisibleRange.Max.Should().Be(20.0);
        model.YAxis.VisibleRange.Min.Should().Be(30.0);
        model.YAxis.VisibleRange.Max.Should().Be(40.0);
    }

    [Fact]
    public void SetVisibleRange_ShouldHandleNegativeValues()
    {
        // Arrange
        var model = new ChartModel();

        // Act
        ChartRenderTestHelper.SetVisibleRange(model, -100.0, -50.0, -25.0, 75.0);

        // Assert
        model.XAxis.VisibleRange.Min.Should().Be(-100.0);
        model.XAxis.VisibleRange.Max.Should().Be(-50.0);
        model.YAxis.VisibleRange.Min.Should().Be(-25.0);
        model.YAxis.VisibleRange.Max.Should().Be(75.0);
    }

    [Fact]
    public void SetVisibleRange_ShouldHandleZeroAndDecimalValues()
    {
        // Arrange
        var model = new ChartModel();

        // Act
        ChartRenderTestHelper.SetVisibleRange(model, 0.0, 1.5, -0.5, 2.75);

        // Assert
        model.XAxis.VisibleRange.Min.Should().Be(0.0);
        model.XAxis.VisibleRange.Max.Should().Be(1.5);
        model.YAxis.VisibleRange.Min.Should().Be(-0.5);
        model.YAxis.VisibleRange.Max.Should().Be(2.75);
    }

    [Fact]
    public void IntegrationTest_RenderAndCompare_ShouldProduceDifferentResults()
    {
        // Arrange
        var emptyModel = new ChartModel();
        ChartRenderTestHelper.SetVisibleRange(emptyModel, 0, 10, 0, 10);
        
        var modelWithData = new ChartModel();
        ChartRenderTestHelper.SetVisibleRange(modelWithData, 0, 10, 0, 10);
        var points = Enumerable.Range(0, 10)
            .Select(i => new PointD(i, Math.Sin(i * 0.5) * 5 + 5))
            .ToArray();
        modelWithData.AddSeries(new LineSeries(points));

        // Act
        var (emptyBitmap, _) = ChartRenderTestHelper.Render(emptyModel);
        var (dataBitmap, _) = ChartRenderTestHelper.Render(modelWithData);
        var differences = ChartRenderTestHelper.CountDifferingPixels(emptyBitmap, dataBitmap);

        // Assert
        differences.Should().BeGreaterThan(0, "Charts with and without data should render differently");
        
        emptyBitmap.Dispose();
        dataBitmap.Dispose();
    }

    [Fact]
    public void Render_ShouldNotThrow_WithComplexChartModel()
    {
        // Arrange
        var model = new ChartModel();
        model.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(10, 10) }));
        model.AddSeries(new ScatterSeries(new[] { new PointD(5, 5), new PointD(7, 3) }) { MarkerSize = 8 });
        ChartRenderTestHelper.SetVisibleRange(model, -1, 11, -1, 11);

        // Act & Assert
        var renderAction = () => ChartRenderTestHelper.Render(model, 200, 150);
        renderAction.Should().NotThrow();
        
        var (bitmap, _) = renderAction.Invoke();
        bitmap.Should().NotBeNull();
        bitmap.Dispose();
    }
}