using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia.Helpers;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

/// <summary>
/// Tests for SeriesColorResolver to ensure the extracted static method works correctly.
/// </summary>
public class SeriesColorResolverTests
{
    private readonly ChartModel _testModel;
    private readonly ColorRgba[] _testPalette;

    public SeriesColorResolverTests()
    {
        _testModel = new ChartModel();
        _testPalette = new[]
        {
            new ColorRgba(255, 0, 0), // Red
            new ColorRgba(0, 255, 0), // Green
            new ColorRgba(0, 0, 255)  // Blue
        };
    }

    [Fact]
    public void ResolveSeriesColor_WithNullPalette_ReturnsPrimaryColor()
    {
        // Arrange
        var series = new LineSeries();

        // Act
        var result = SeriesColorResolver.ResolveSeriesColor(_testModel, series, null!);

        // Assert
        result.Should().Be(_testModel.Theme.PrimarySeriesColor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ResolveSeriesColor_WithEmptyOrInvalidPalette_ReturnsPrimaryColor(int paletteSize)
    {
        // Arrange
        var series = new LineSeries();
        var palette = paletteSize < 0 ? null! : new ColorRgba[paletteSize];

        // Act
        var result = SeriesColorResolver.ResolveSeriesColor(_testModel, series, palette);

        // Assert
        result.Should().Be(_testModel.Theme.PrimarySeriesColor);
    }

    [Theory]
    [InlineData(0, 0)] // First series gets first color
    [InlineData(1, 1)] // Second series gets second color
    [InlineData(2, 2)] // Third series gets third color
    public void ResolveSeriesColor_WithMultipleSeries_ReturnsCorrectSequentialColor(int seriesIndex, int expectedColorIndex)
    {
        // Arrange
        var series = CreateMultipleSeries();
        _testModel.Series.Clear();
        for (var i = 0; i <= seriesIndex; i++)
        {
            _testModel.AddSeries(series[i]);
        }

        // Act
        var result = SeriesColorResolver.ResolveSeriesColor(_testModel, series[seriesIndex], _testPalette);

        // Assert
        result.Should().Be(_testPalette[expectedColorIndex]);
    }

    private LineSeries[] CreateMultipleSeries()
    {
        return new[]
        {
            new LineSeries(new[] { new PointD(1, 1) }),
            new LineSeries(new[] { new PointD(2, 2) }),
            new LineSeries(new[] { new PointD(3, 3) })
        };
    }

    [Fact]
    public void ResolveSeriesColorWithPaletteIndexUsesSpecifiedIndex()
    {
        // Arrange
        var model = new ChartModel();
        var series = new LineSeries(new[] { new PointD(1, 1) })
        {
            PaletteIndex = 2 // Explicitly set to third color
        };

        model.AddSeries(series);

        // Act
        var result = SeriesColorResolver.ResolveSeriesColor(model, series, _testPalette);

        // Assert
        result.Should().Be(_testPalette[2]); // Should use third color (blue)
    }

    [Fact]
    public void ResolveSeriesColorWithScatterSeriesReturnsCorrectColor()
    {
        // Arrange
        var model = new ChartModel();
        var series = new ScatterSeries(new[] { new PointD(1, 1) });
        model.AddSeries(series);

        var palette = new[]
        {
            new ColorRgba(255, 0, 0), // Red
            new ColorRgba(0, 255, 0)  // Green
        };

        // Act
        var result = SeriesColorResolver.ResolveSeriesColor(model, series, palette);

        // Assert
        result.Should().Be(palette[0]); // Should get first color
    }

    [Fact]
    public void ResolveSeriesColorWithIndexOutOfRangeReturnsPrimaryColor()
    {
        // Arrange
        var model = new ChartModel();
        var series = new LineSeries(new[] { new PointD(1, 1) })
        {
            PaletteIndex = 10 // Index beyond palette size
        };

        var palette = new[]
        {
            new ColorRgba(255, 0, 0), // Red
            new ColorRgba(0, 255, 0)  // Green
        };

        // Act
        var result = SeriesColorResolver.ResolveSeriesColor(model, series, palette);

        // Assert
        result.Should().Be(model.Theme.PrimarySeriesColor);
    }

    [Fact]
    public void ResolveSeriesColorWithUnknownSeriesTypeReturnsPrimaryColor()
    {
        // Arrange
        var model = new ChartModel();
        var unknownObject = new object();
        var palette = new[]
        {
            new ColorRgba(255, 0, 0), // Red
            new ColorRgba(0, 255, 0)  // Green
        };

        // Act
        var result = SeriesColorResolver.ResolveSeriesColor(model, unknownObject, palette);

        // Assert
        result.Should().Be(model.Theme.PrimarySeriesColor);
    }
}