using System;

using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class LineSeriesTests
{
    [Fact]
    public void LineSeries_ShouldExposeDataAndRanges()
    {
        var points = new[]
        {
            new PointD(0, 10),
            new PointD(5, -5),
            new PointD(10, 20)
        };

        var s = new LineSeries(points) { StrokeThickness = 2 };

        s.IsEmpty.Should().BeFalse();
        s.Data.Should().HaveCount(3);
        s.StrokeThickness.Should().Be(2);

        var xr = s.GetXRange();
        var yr = s.GetYRange();

        xr.Min.Should().Be(0);
        xr.Max.Should().Be(10);
        yr.Min.Should().Be(-5);
        yr.Max.Should().Be(20);
    }

    [Fact]
    public void LineSeries_WithNoPoints_ShouldBeEmptyAndZeroRanges()
    {
        var s = new LineSeries(Array.Empty<PointD>());
        s.IsEmpty.Should().BeTrue();

        var xr = s.GetXRange();
        var yr = s.GetYRange();

        xr.Min.Should().Be(0);
        xr.Max.Should().Be(0);
        yr.Min.Should().Be(0);
        yr.Max.Should().Be(0);
    }
}
