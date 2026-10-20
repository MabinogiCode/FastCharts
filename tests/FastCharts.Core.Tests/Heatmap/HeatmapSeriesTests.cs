using System;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.Heatmap;

/// <summary>
/// P2-HEATMAP: grid geometry, cell lookup, value range and change notifications.
/// </summary>
public class HeatmapSeriesTests
{
    // Row 0 is the bottom row
    private static double[,] Grid()
    {
        return new double[,]
        {
            { 0, 1, 2 },
            { 3, 4, 5 },
        };
    }

    [Fact]
    public void DefaultExtentIsOneUnitPerCell()
    {
        // Arrange / Act
        var heatmap = new HeatmapSeries(Grid());

        // Assert
        heatmap.ColumnCount.Should().Be(3);
        heatmap.RowCount.Should().Be(2);
        heatmap.XRange.Should().Be(new FRange(0, 3));
        heatmap.YRange.Should().Be(new FRange(0, 2));
        heatmap.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void RangeProviderReportsTheGridEdges()
    {
        // Arrange
        ISeriesRangeProvider heatmap = new HeatmapSeries(Grid(), new FRange(-3, 3), new FRange(10, 20));

        // Act
        var ok = heatmap.TryGetRanges(out var x, out var y);

        // Assert
        ok.Should().BeTrue();
        x.Should().Be(new FRange(-3, 3));
        y.Should().Be(new FRange(10, 20));
    }

    [Fact]
    public void EmptyGridIsEmptyButKeepsAValidExtent()
    {
        // Arrange / Act
        var heatmap = new HeatmapSeries(new double[0, 0]);

        // Assert
        heatmap.IsEmpty.Should().BeTrue();
        heatmap.XRange.Size.Should().BeGreaterThan(0);
        heatmap.TryGetValueAt(0.5, 0.5, out _).Should().BeFalse();
        heatmap.TryGetValueRange(out _).Should().BeFalse();
    }

    [Fact]
    public void ConstructorValidatesArguments()
    {
        FluentActions.Invoking(() => new HeatmapSeries(null!)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new HeatmapSeries(Grid(), new FRange(1, 1))).Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => new HeatmapSeries(Grid(), null, new FRange(0, double.NaN))).Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => new HeatmapSeries(Grid()) { ColorAxis = null! }).Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(-3.0, 10.0, 0, 0)]
    [InlineData(-0.1, 14.9, 0, 1)]
    [InlineData(2.9, 19.9, 1, 2)]
    [InlineData(3.0, 20.0, 1, 2)] // outer edges belong to the last cells
    public void TryGetCellFindsTheCellUnderAPoint(double x, double y, int expectedRow, int expectedColumn)
    {
        // Arrange: columns are 2 units wide, rows 5 units high
        var heatmap = new HeatmapSeries(Grid(), new FRange(-3, 3), new FRange(10, 20));

        // Act
        var found = heatmap.TryGetCell(x, y, out var row, out var column);

        // Assert
        found.Should().BeTrue();
        row.Should().Be(expectedRow);
        column.Should().Be(expectedColumn);
    }

    [Theory]
    [InlineData(-3.1, 15.0)]
    [InlineData(0.0, 9.9)]
    [InlineData(0.0, 20.1)]
    public void TryGetCellRejectsPointsOutsideTheGrid(double x, double y)
    {
        // Arrange
        var heatmap = new HeatmapSeries(Grid(), new FRange(-3, 3), new FRange(10, 20));

        // Act / Assert
        heatmap.TryGetCell(x, y, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void TryGetValueAtReadsTheCellAndSkipsMissingValues()
    {
        // Arrange
        var values = Grid();
        values[1, 2] = double.NaN;
        var heatmap = new HeatmapSeries(values);

        // Act / Assert
        heatmap.TryGetValueAt(1.5, 0.5, out var bottomMiddle).Should().BeTrue();
        bottomMiddle.Should().Be(1);
        heatmap.TryGetValueAt(0.5, 1.5, out var topLeft).Should().BeTrue();
        topLeft.Should().Be(3);
        heatmap.TryGetValueAt(2.5, 1.5, out _).Should().BeFalse("NaN cells are missing values");
    }

    [Fact]
    public void TryGetValueRangeIgnoresMissingValues()
    {
        // Arrange
        var values = Grid();
        values[0, 0] = double.NaN;
        values[1, 2] = double.PositiveInfinity;
        var heatmap = new HeatmapSeries(values);

        // Act
        var ok = heatmap.TryGetValueRange(out var range);

        // Assert
        ok.Should().BeTrue();
        range.Should().Be(new FRange(1, 4));
    }

    [Fact]
    public void TryGetValueRangeFailsWhenEveryCellIsMissing()
    {
        // Arrange
        var heatmap = new HeatmapSeries(new double[,] { { double.NaN, double.NaN } });

        // Act / Assert
        heatmap.TryGetValueRange(out _).Should().BeFalse();
    }

    [Fact]
    public void InPlaceEditsAreSeenAfterInvalidate()
    {
        // Arrange
        var heatmap = new HeatmapSeries(Grid());
        heatmap.TryGetValueRange(out _);
        var version = heatmap.DataVersion;
        var changes = 0;
        heatmap.Changed += (_, _) => changes++;

        // Act
        heatmap.Values[0, 0] = -10;
        heatmap.TryGetValueRange(out var cached);
        heatmap.Invalidate();
        heatmap.TryGetValueRange(out var refreshed);

        // Assert
        cached.Min.Should().Be(0, "the value range is cached until the series is invalidated");
        refreshed.Min.Should().Be(-10);
        heatmap.DataVersion.Should().BeGreaterThan(version);
        changes.Should().Be(1);
    }

    [Fact]
    public void SetValuesReplacesGridAndExtent()
    {
        // Arrange
        var heatmap = new HeatmapSeries(Grid(), new FRange(-3, 3), new FRange(10, 20));
        var version = heatmap.DataVersion;
        var changes = 0;
        heatmap.Changed += (_, _) => changes++;

        // Act
        heatmap.SetValues(new double[,] { { 7, 8 } }, yRange: new FRange(0, 1));

        // Assert
        heatmap.ColumnCount.Should().Be(2);
        heatmap.RowCount.Should().Be(1);
        heatmap.XRange.Should().Be(new FRange(-3, 3), "an omitted extent is kept");
        heatmap.YRange.Should().Be(new FRange(0, 1));
        heatmap.TryGetValueRange(out var range).Should().BeTrue();
        range.Should().Be(new FRange(7, 8));
        heatmap.DataVersion.Should().BeGreaterThan(version);
        changes.Should().Be(1);
    }

    [Fact]
    public void SetValuesValidatesArguments()
    {
        // Arrange
        var heatmap = new HeatmapSeries(Grid());

        // Act / Assert
        FluentActions.Invoking(() => heatmap.SetValues(null!)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => heatmap.SetValues(Grid(), new FRange(2, 1))).Should().Throw<ArgumentException>();
        heatmap.XRange.Should().Be(new FRange(0, 3), "a rejected update leaves the series untouched");
    }

    [Fact]
    public void ColorAxisCanBeShared()
    {
        // Arrange
        var shared = new ColorAxis { Minimum = 0, Maximum = 100 };

        // Act
        var a = new HeatmapSeries(Grid()) { ColorAxis = shared };
        var b = new HeatmapSeries(Grid()) { ColorAxis = shared };

        // Assert
        a.ColorAxis.Should().BeSameAs(b.ColorAxis);
    }

    [Fact]
    public void AddHeatmapAddsTheSeriesAndFitsTheAxes()
    {
        // Arrange
        using var model = new ChartModel();

        // Act
        var heatmap = model.AddHeatmap(Grid(), new FRange(-3, 3), new FRange(10, 20), "Field");

        // Assert
        model.Series.Should().ContainSingle().Which.Should().BeSameAs(heatmap);
        heatmap.Title.Should().Be("Field");
        model.XAxis.VisibleRange.Min.Should().BeLessThanOrEqualTo(-3);
        model.XAxis.VisibleRange.Max.Should().BeGreaterThanOrEqualTo(3);
        model.YAxis.VisibleRange.Min.Should().BeLessThanOrEqualTo(10);
        model.YAxis.VisibleRange.Max.Should().BeGreaterThanOrEqualTo(20);
    }
}
