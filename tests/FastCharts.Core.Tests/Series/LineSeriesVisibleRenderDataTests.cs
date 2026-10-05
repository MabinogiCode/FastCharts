using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.Series;

/// <summary>
/// Viewport-aware decimation: only the visible X window is resampled, so zooming into a
/// large series reveals every point instead of a coarse whole-series decimation.
/// </summary>
public class LineSeriesVisibleRenderDataTests
{
    [Fact]
    public void ZoomedWindowReturnsEveryVisiblePointPlusNeighbors()
    {
        // Arrange: 100k sorted points, X = index
        var series = new LineSeries(Enumerable.Range(0, 100_000).Select(i => new PointD(i, i % 7)));

        // Act: 200 points visible
        var data = series.GetRenderData(800, new FRange(1000, 1199));

        // Assert: [999, 1200] — one extra point on each side so lines reach the plot edges
        data.Should().HaveCount(202);
        data[0].X.Should().Be(999);
        data[data.Count - 1].X.Should().Be(1200);
    }

    [Fact]
    public void WideVisibleWindowIsStillDecimated()
    {
        // Arrange
        var series = new LineSeries(Enumerable.Range(0, 100_000).Select(i => new PointD(i, i % 7)));

        // Act
        var data = series.GetRenderData(800, new FRange(0, 50_000));

        // Assert: resampled, and only over the visible half
        data.Count.Should().BeLessThanOrEqualTo(1600);
        data[data.Count - 1].X.Should().BeLessThanOrEqualTo(50_001);
    }

    [Fact]
    public void UnsortedDataFallsBackToWholeSeries()
    {
        // Arrange: descending X
        var series = new LineSeries(Enumerable.Range(0, 10_000).Select(i => new PointD(10_000 - i, i)));

        // Act
        var data = series.GetRenderData(800, new FRange(100, 200));

        // Assert: decimated over everything (binary search impossible)
        data.Max(p => p.X).Should().Be(10_000);
        data.Min(p => p.X).Should().Be(1);
    }

    [Fact]
    public void SameWindowIsServedFromCache()
    {
        // Arrange
        var series = new LineSeries(Enumerable.Range(0, 100_000).Select(i => new PointD(i, i)));

        // Act
        var first = series.GetRenderData(800, new FRange(0, 60_000));
        var second = series.GetRenderData(800, new FRange(0, 60_000));

        // Assert
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void OutOfOrderAppendDisablesBinarySearch()
    {
        // Arrange
        var series = new LineSeries();
        series.AddPoints(new[] { new PointD(0, 0), new PointD(1, 1), new PointD(2, 2) });
        series.TryGetIndexRange(0.5, 1.5, out var start, out var end).Should().BeTrue();
        start.Should().Be(1);
        end.Should().Be(2);

        // Act
        series.AddPoint(new PointD(-5, 0));

        // Assert
        series.TryGetIndexRange(0.5, 1.5, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void ClearRestoresBinarySearch()
    {
        // Arrange
        var series = new LineSeries(new[] { new PointD(3, 0), new PointD(1, 0) });
        series.TryGetIndexRange(0, 10, out _, out _).Should().BeFalse();

        // Act
        series.Clear();
        series.AddPoints(new[] { new PointD(0, 0), new PointD(5, 0) });

        // Assert
        series.TryGetIndexRange(0, 10, out var start, out var end).Should().BeTrue();
        (end - start).Should().Be(2);
    }

    [Fact]
    public void InvalidateCacheRechecksOrderingAfterDirectEdits()
    {
        // Arrange
        var series = new LineSeries(new[] { new PointD(0, 0), new PointD(1, 0), new PointD(2, 0) });
        series.TryGetIndexRange(0, 2, out _, out _).Should().BeTrue();

        // Act: direct edit breaking the order, then the documented InvalidateCache call
        series.Data[0] = new PointD(10, 0);
        series.InvalidateCache();

        // Assert
        series.TryGetIndexRange(0, 2, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void GetXRangeOnSortedDataUsesEndpoints()
    {
        var series = new LineSeries(new[] { new PointD(-3, 5), new PointD(0, 1), new PointD(8, 2) });

        var range = series.GetXRange();

        range.Min.Should().Be(-3);
        range.Max.Should().Be(8);
    }
}
