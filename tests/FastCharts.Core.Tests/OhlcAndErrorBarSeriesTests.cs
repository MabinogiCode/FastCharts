using FastCharts.Core.Series;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests;

public class OhlcAndErrorBarSeriesTests
{
    [Fact]
    public void OhlcSeriesRangesShouldUseHighLowAndPadX()
    {
        var s = new OhlcSeries(new[]
        {
            new OhlcPoint(10, 5, 9, 4, 6),
            new OhlcPoint(20, 6, 11, 5, 7)
        });
        var xr = s.GetXRange();
        var yr = s.GetYRange();
        xr.Min.Should().BeLessThan(10);
        xr.Max.Should().BeGreaterThan(20);
        yr.Min.Should().Be(4);
        yr.Max.Should().Be(11);
    }

    [Fact]
    public void ErrorBarSeriesRangesShouldIncludeCapWidthAndErrors()
    {
        var s = new ErrorBarSeries(new[]
        {
            new ErrorBarPoint(0, 10, 2, 1),
            new ErrorBarPoint(10, 20, 3, 2)
        });
        var xr = s.GetXRange();
        var yr = s.GetYRange();
        xr.Min.Should().BeLessThan(0);
        xr.Max.Should().BeGreaterThan(10);
        yr.Min.Should().Be(10 - 1);
        yr.Max.Should().Be(20 + 3);
    }
}
