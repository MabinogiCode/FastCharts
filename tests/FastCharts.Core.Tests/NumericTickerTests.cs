using System.Linq;

using FastCharts.Core.Primitives;
using FastCharts.Core.Ticks;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class NumericTickerTests
{
    [Fact]
    public void GetTicks_ShouldReturnSequenceWithinRange()
    {
        var t = new NumericTicker();
        var ticks = t.GetTicks(new FRange(0, 10), approxStep: 2);

        ticks.Should().NotBeEmpty();
        ticks.First().Should().BeLessOrEqualTo(0);
        ticks.Last().Should().BeGreaterOrEqualTo(10);
        ticks.Should().OnlyHaveUniqueItems();
        ticks.Zip(ticks.Skip(1)).All(p => p.Second > p.First).Should().BeTrue();
    }

    [Fact]
    public void GetTicks_ShouldBeEmpty_WhenRangeNotPositive()
    {
        var t = new NumericTicker();
        t.GetTicks(new FRange(5, 5), approxStep: 1).Should().BeEmpty();
        t.GetTicks(new FRange(10, 0), approxStep: 1).Should().BeEmpty();
    }
}
