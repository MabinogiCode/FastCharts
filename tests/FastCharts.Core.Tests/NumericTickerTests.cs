using System.Linq;

using FastCharts.Core.Primitives;
using FastCharts.Core.Ticks;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests;

public class NumericTickerTests
{
    [Fact]
    public void GetTicksShouldReturnSequenceWithinRange()
    {
        var t = new NumericTicker();
        var ticks = t.GetTicks(new FRange(0, 10), approxStep: 2);
        ticks.Should().NotBeEmpty();
        ticks[0].Should().BeLessThanOrEqualTo(0);
        ticks[ticks.Count - 1].Should().BeGreaterThanOrEqualTo(10);
        ticks.Should().OnlyHaveUniqueItems();
        for (var i = 1; i < ticks.Count; i++)
        {
            (ticks[i] > ticks[i - 1]).Should().BeTrue();
        }
    }

    [Fact]
    public void GetTicksShouldBeEmptyWhenRangeNotPositive()
    {
        var t = new NumericTicker();
        t.GetTicks(new FRange(5, 5), approxStep: 1).Should().BeEmpty();
        t.GetTicks(new FRange(10, 0), approxStep: 1).Should().BeEmpty();
    }
}
