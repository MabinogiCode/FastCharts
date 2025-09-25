using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class StackedBarSeriesTests
{
    [Fact]
    public void GetYRange_ShouldSumPositiveAndNegativeStacks_RelativeToBaseline()
    {
        var s = new StackedBarSeries(new[]
        {
            new StackedBarPoint(0, new[]{ 1.0, 2.0, -0.5 }),
            new StackedBarPoint(1, new[]{ 0.5, -1.5, 3.0 }),
        }) { Baseline = 0 };

        var yr = s.GetYRange();
        yr.Min.Should().BeLessThanOrEqualTo(-1.5);
        yr.Max.Should().BeGreaterThanOrEqualTo(3.0);
    }

    [Fact]
    public void GetXRange_ShouldExpandByHalfWidth()
    {
        var s = new StackedBarSeries(new[]
        {
            new StackedBarPoint(10, new[]{ 1.0, 2.0 }),
            new StackedBarPoint(20, new[]{ -1.0, 1.0 })
        });

        var xr = s.GetXRange();
        xr.Min.Should().BeLessThan(10);
        xr.Max.Should().BeGreaterThan(20);
    }
}
