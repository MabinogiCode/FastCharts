using System.IO;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;
using FastCharts.Rendering.Skia.Rendering;

using FluentAssertions;

using SkiaSharp;

using Xunit;

namespace FastCharts.Core.Tests.Axes;

/// <summary>
/// Regression tests: rendering used to map every axis linearly, so logarithmic axes were drawn
/// as linear ones (10 on a [1, 100] log axis landed at 9% of the height instead of 50%).
/// </summary>
public class LogAxisRenderingTests
{
    [Fact]
    public void PixelMapperMapsLogAxisLogarithmically()
    {
        // Arrange
        var axis = new LogNumericAxis();
        axis.SetVisibleRange(1, 100);
        var plot = new SKRect(0, 0, 100, 100);

        // Act
        var py = PixelMapper.Y(10.0, axis, plot);

        // Assert
        py.Should().BeApproximately(50f, 1e-3f);
    }

    [Fact]
    public void PixelMapperMapsLinearAxisLinearly()
    {
        // Arrange
        var axis = new NumericAxis { VisibleRange = new FRange(0, 200) };
        var plot = new SKRect(10, 0, 110, 100);

        // Act
        var px = PixelMapper.X(50.0, axis, plot);

        // Assert
        px.Should().BeApproximately(35f, 1e-3f);
    }

    [Fact]
    public void PixelMapperRoundTripsOnLogAxis()
    {
        // Arrange
        var axis = new LogarithmicAxis();
        axis.SetVisibleRange(1, 10000);
        var plot = new SKRect(0, 0, 400, 300);

        // Act
        var px = PixelMapper.X(316.0, axis, plot);
        var value = PixelMapper.ToDataX(px, axis, plot);

        // Assert
        value.Should().BeApproximately(316.0, 0.01);
    }

    [Fact]
    public void SwitchingToLogAxisWithSameRangeRebuildsCachedGeometry()
    {
        // Arrange: warm the line geometry cache with a linear Y axis
        using var model = new ChartModel();
        model.AddSeries(new LineSeries(Enumerable.Range(1, 100).Select(i => new PointD(i, i * i))));
        model.YAxis.VisibleRange = new FRange(1, 10000);
        var warmRenderer = new SkiaChartRenderer();
        ExportBytes(warmRenderer, model);

        // Act: same visible range, different transform
        model.SetYAxisLogarithmic();
        var fromWarmCache = ExportBytes(warmRenderer, model);
        var fromScratch = ExportBytes(new SkiaChartRenderer(), model);

        // Assert
        model.YAxis.VisibleRange.Should().Be(new FRange(1, 10000));
        fromWarmCache.Should().Equal(fromScratch, "the line cache must not reuse geometry projected with the old axis");
    }

    private static byte[] ExportBytes(SkiaChartRenderer renderer, ChartModel model)
    {
        using var stream = new MemoryStream();
        renderer.ExportPng(model, stream, 640, 400);
        return stream.ToArray();
    }
}
