using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class BarSeriesTests
{
    [Fact]
    public void GetWidthForUsesGlobalWidthWhenSet()
    {
        var s = new BarSeries(new[]
        {
            new BarPoint(0, 1),
            new BarPoint(1, 2),
            new BarPoint(2, 3),
        }) { Width = 0.5 };

        s.GetWidthFor(0).Should().Be(0.5);
        s.GetWidthFor(1).Should().Be(0.5);
    }

    [Fact]
    public void GetWidthForInfersFromSpacingWhenNotSet()
    {
        var s = new BarSeries(new[]
        {
            new BarPoint(0, 1),
            new BarPoint(2, 2),
            new BarPoint(5, 3),
        });
        // min dx = 2 => width ~ 1.6
        s.GetWidthFor(1).Should().BeApproximately(1.6, 1e-6);
    }

    [Fact]
    public void GetRangesExpandsXByHalfBarAndYIncludesBaseline()
    {
        var s = new BarSeries(new[]
        {
            new BarPoint(10, 2),
            new BarPoint(20, -3)
        }) { Baseline = 0 };
        var xr = s.GetXRange();
        var yr = s.GetYRange();

        xr.Min.Should().BeLessThan(10);
        xr.Max.Should().BeGreaterThan(20);
        yr.Min.Should().BeLessThanOrEqualTo(-3);
        yr.Max.Should().BeGreaterThanOrEqualTo(2);
    }
}
