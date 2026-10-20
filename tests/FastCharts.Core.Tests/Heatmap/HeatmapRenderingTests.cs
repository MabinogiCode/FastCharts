using System;
using System.IO;
using System.Text;

using FastCharts.Core.Helpers;
using FastCharts.Core.Interaction;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes;
using FastCharts.Rendering.Skia;
using FastCharts.Rendering.Skia.Rendering;

using FluentAssertions;

using SkiaSharp;

using Xunit;

namespace FastCharts.Core.Tests.Heatmap;

/// <summary>
/// P2-HEATMAP / P1-AX-COLOR rendering: cell colors, missing values, logarithmic axes, image
/// cache invalidation, color bar and the layout space it reserves.
/// </summary>
public class HeatmapRenderingTests
{
    private const int Width = 400;
    private const int Height = 300;

    [Fact]
    public void CellsAreColoredThroughTheColorAxis()
    {
        // Arrange
        using var model = CreateModel(out _);

        // Act
        using var bitmap = new SkiaChartRenderer().RenderToBitmap(model, Width, Height);

        // Assert: grayscale over [0, 3] → 0, 85, 170, 255
        CellColor(bitmap, model, 0, 0).Should().Be(Gray(0));
        CellColor(bitmap, model, 0, 1).Should().Be(Gray(85));
        CellColor(bitmap, model, 1, 0).Should().Be(Gray(170), "row 1 is the top row");
        CellColor(bitmap, model, 1, 1).Should().Be(Gray(255));
    }

    [Fact]
    public void MissingCellsLetThePlotShowThrough()
    {
        // Arrange: reference frame where every cell is missing (plot background and grid only)
        using var model = CreateModel(out var heatmap);
        using var underneath = RenderWithoutCells(model, heatmap);
        heatmap.Values[1, 0] = double.NaN;
        heatmap.Invalidate();

        // Act
        using var bitmap = new SkiaChartRenderer().RenderToBitmap(model, Width, Height);

        // Assert
        CellColor(bitmap, model, 1, 0).Should().Be(CellColor(underneath, model, 1, 0));
        CellColor(bitmap, model, 1, 1).Should().Be(Gray(255), "other cells keep their colors");
    }

    [Fact]
    public void LogarithmicAxisDrawsCellsAtLogPositions()
    {
        // Arrange: two rows over [1, 100]; on a log axis the row boundary (50.5) sits near the top
        using var model = new ChartModel();
        var heatmap = model.AddHeatmap(new double[,] { { 0, 0 }, { 1, 1 } }, new FRange(0, 2), new FRange(1, 100));
        heatmap.ColorAxis.ColorMap = ColorMaps.Grayscale;
        model.SetYAxisLogarithmic();
        model.XAxis.VisibleRange = new FRange(0, 2);
        model.YAxis.VisibleRange = new FRange(1, 100);
        var plot = PlotRect(model);
        var x = (int)PixelMapper.X(0.5, model.XAxis, plot);

        // Act
        using var bitmap = new SkiaChartRenderer().RenderToBitmap(model, Width, Height);

        // Assert: y = 30 is in the bottom row, although it is above the middle of the plot
        var y30 = (int)PixelMapper.Y(30.0, model.YAxis, plot);
        y30.Should().BeLessThan((int)plot.MidY);
        bitmap.GetPixel(x, y30).Should().Be(Gray(0));
        bitmap.GetPixel(x, (int)PixelMapper.Y(80.0, model.YAxis, plot)).Should().Be(Gray(255));
    }

    [Fact]
    public void OpacityBlendsCellsWithWhatIsUnderneath()
    {
        // Arrange
        using var model = CreateModel(out var heatmap);
        using var underneath = RenderWithoutCells(model, heatmap);
        var below = CellColor(underneath, model, 0, 0);
        heatmap.Opacity = 0.5;

        // Act
        using var bitmap = new SkiaChartRenderer().RenderToBitmap(model, Width, Height);

        // Assert: a black cell at 50% halves what is underneath
        var pixel = CellColor(bitmap, model, 0, 0);
        ((int)pixel.Red).Should().BeCloseTo(below.Red / 2, 2);
        ((int)pixel.Green).Should().BeCloseTo(below.Green / 2, 2);
        ((int)pixel.Blue).Should().BeCloseTo(below.Blue / 2, 2);
    }

    [Fact]
    public void ReusedRendererPicksUpValueAndScaleChanges()
    {
        // Arrange: warm the image cache
        using var model = CreateModel(out var heatmap);
        var renderer = new SkiaChartRenderer();
        renderer.RenderToBitmap(model, Width, Height).Dispose();

        // Act 1: in-place edit + Invalidate
        heatmap.Values[0, 0] = 3;
        heatmap.Invalidate();
        using var edited = renderer.RenderToBitmap(model, Width, Height);

        // Act 2: new color map, then fixed bounds
        heatmap.ColorAxis.ColorMap = ColorMaps.Grayscale.Reversed();
        using var reversed = renderer.RenderToBitmap(model, Width, Height);
        heatmap.ColorAxis.Minimum = 1;
        heatmap.ColorAxis.Maximum = 2;
        using var clamped = renderer.RenderToBitmap(model, Width, Height);

        // Assert
        CellColor(edited, model, 0, 0).Should().Be(Gray(255));
        CellColor(reversed, model, 0, 0).Should().Be(Gray(0));
        CellColor(clamped, model, 0, 1).Should().Be(Gray(255), "1 is now the low end of the reversed map");
        CellColor(clamped, model, 1, 1).Should().Be(Gray(0), "3 is above the range and takes the last color");
    }

    [Fact]
    public void ColorBarIsDrawnInTheReservedStrip()
    {
        // Arrange
        using var model = CreateModel(out _);
        var plot = PlotRect(model);
        var barCenterX = Width - (int)PlotLayout.ColorBarReservedWidth + 15;

        // Act
        using var bitmap = new SkiaChartRenderer().RenderToBitmap(model, Width, Height);

        // Assert: high values at the top, low values at the bottom
        plot.Right.Should().BeLessThanOrEqualTo(Width - (float)PlotLayout.ColorBarReservedWidth);
        bitmap.GetPixel(barCenterX, (int)plot.Top + 3).Red.Should().BeGreaterThan(240);
        bitmap.GetPixel(barCenterX, (int)plot.Bottom - 3).Red.Should().BeLessThan(15);
    }

    [Fact]
    public void HiddenColorBarGivesTheSpaceBackToThePlot()
    {
        // Arrange
        using var model = CreateModel(out var heatmap);
        var withBar = PlotLayout.Compute(model, Width, Height);

        // Act
        heatmap.ColorAxis.ShowColorBar = false;
        var withoutBar = PlotLayout.Compute(model, Width, Height);
        heatmap.ColorAxis.ShowColorBar = true;
        heatmap.IsVisible = false;
        var hiddenSeries = PlotLayout.Compute(model, Width, Height);

        // Assert
        PlotLayout.FindColorBarSeries(model).Should().BeNull();
        (withoutBar.Width - withBar.Width).Should().Be(PlotLayout.ColorBarReservedWidth);
        hiddenSeries.Width.Should().Be(withoutBar.Width);
    }

    [Fact]
    public void LegendStaysLeftOfTheColorBar()
    {
        // Arrange
        using var model = CreateModel(out _);
        model.InteractionState = new InteractionState();

        // Act
        using var bitmap = new SkiaChartRenderer().RenderToBitmap(model, Width, Height);

        // Assert
        var hit = model.InteractionState.LegendHits.Should().ContainSingle().Subject;
        (hit.X + hit.Width).Should().BeLessThanOrEqualTo(Width - PlotLayout.ColorBarReservedWidth);
    }

    [Fact]
    public void SvgExportEmbedsTheHeatmap()
    {
        // Arrange
        using var model = CreateModel(out _);
        using var stream = new MemoryStream();

        // Act
        new SkiaChartRenderer().ExportSvg(model, stream, Width, Height);

        // Assert
        var svg = Encoding.UTF8.GetString(stream.ToArray());
        svg.Should().Contain("<image", "the cell image is embedded in the SVG");
        svg.Should().Contain("linearGradient", "the color bar is a vector gradient");
    }

    [Fact]
    public void SecondaryAxisTicksUseTheirOwnSpacing()
    {
        // Arrange: Y2 spans 100x the primary range; it used to be ticked with the primary step
        using var model = new ChartModel();
        model.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(10, 10) }));
        model.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(10, 1000) }) { YAxisIndex = 1 });
        using var stream = new MemoryStream();

        // Act
        new SkiaChartRenderer().ExportSvg(model, stream, Width, Height);

        // Assert
        var svg = Encoding.UTF8.GetString(stream.ToArray());
        var labels = svg.Split(new[] { "<text" }, StringSplitOptions.None).Length - 1;
        labels.Should().BeLessThan(60, "about 5 labels per axis are expected, not one every 2 units of Y2");
    }

    private static ChartModel CreateModel(out HeatmapSeries heatmap)
    {
        var model = new ChartModel();
        heatmap = model.AddHeatmap(new double[,] { { 0, 1 }, { 2, 3 } }, title: "Field");
        heatmap.ColorAxis.ColorMap = ColorMaps.Grayscale;
        model.XAxis.VisibleRange = new FRange(0, 2);
        model.YAxis.VisibleRange = new FRange(0, 2);
        return model;
    }

    private static SKBitmap RenderWithoutCells(ChartModel model, HeatmapSeries heatmap)
    {
        var values = heatmap.Values;
        var saved = (double[,])values.Clone();
        for (var row = 0; row < heatmap.RowCount; row++)
        {
            for (var column = 0; column < heatmap.ColumnCount; column++)
            {
                values[row, column] = double.NaN;
            }
        }

        heatmap.Invalidate();
        var bitmap = new SkiaChartRenderer().RenderToBitmap(model, Width, Height);
        Array.Copy(saved, values, values.Length);
        heatmap.Invalidate();
        return bitmap;
    }

    private static SKRect PlotRect(ChartModel model)
    {
        var area = PlotLayout.Compute(model, Width, Height);
        return new SKRect((float)area.Left, (float)area.Top, (float)area.Right, (float)area.Bottom);
    }

    private static SKColor CellColor(SKBitmap bitmap, ChartModel model, int row, int column)
    {
        // Cell centers of the 2x2 grid spanning the [0, 2] x [0, 2] view
        var plot = PlotRect(model);
        var x = plot.Left + ((column + 0.5f) / 2f * plot.Width);
        var y = plot.Bottom - ((row + 0.5f) / 2f * plot.Height);
        return bitmap.GetPixel((int)x, (int)y);
    }

    private static SKColor Gray(byte level)
    {
        return new SKColor(level, level, level);
    }
}
