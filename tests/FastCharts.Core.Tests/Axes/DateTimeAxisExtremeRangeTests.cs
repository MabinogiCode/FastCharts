using System;
using FastCharts.Core;
using FastCharts.Core.Axes;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;

using FluentAssertions;

using SkiaSharp;

using Xunit;

namespace FastCharts.Core.Tests.Axes;

/// <summary>
/// Regression tests: zooming a date axis out beyond year 1 / 9999 made the ticker overflow
/// DateTime and the whole frame failed to render.
/// </summary>
public class DateTimeAxisExtremeRangeTests
{
    [Theory]
    [InlineData(-1e7, 1e7)]
    [InlineData(-1e9, 1e9)]
    [InlineData(2.9e6, 2.96e6)]
    [InlineData(2958465.5, 2958465.99)]
    [InlineData(-700000, -600000)]
    public void RenderingFarZoomedOutDateAxisDoesNotThrow(double min, double max)
    {
        // Arrange
        using var model = new ChartModel();
        model.ReplaceXAxis(new DateTimeAxis());
        model.AddSeries(new LineSeries(new[] { new PointD(45000, 1), new PointD(45010, 2) }));
        model.XAxis.VisibleRange = new FRange(min, max);
        using var bitmap = new SKBitmap(400, 300);
        using var canvas = new SKCanvas(bitmap);

        // Act
        var act = () => new SkiaChartRenderer().Render(model, canvas, 400, 300);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void TicksOverCenturiesStayReadable()
    {
        // Arrange: about 2700 years
        var ticker = new DateTicker();

        // Act
        var ticks = ticker.GetTicks(new FRange(-1e6 / 2, 1e6 / 2), 1);

        // Assert
        ticks.Should().NotBeEmpty();
        ticks.Count.Should().BeLessThanOrEqualTo(30);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(-1e7)]
    [InlineData(3e6)]
    public void TryFromOADateRejectsOutOfRangeValues(double oaDate)
    {
        DateTimeAxis.TryFromOADate(oaDate, out _).Should().BeFalse();
    }

    [Fact]
    public void TryFromOADateConvertsValidValues()
    {
        var expected = new DateTime(2024, 3, 15, 12, 0, 0);

        DateTimeAxis.TryFromOADate(expected.ToOADate(), out var actual).Should().BeTrue();
        actual.Should().Be(expected);
    }
}
