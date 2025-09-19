using FastCharts.Core.Scales;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class LinearScaleTests
{
    [Fact]
    public void ToPixels_ShouldMapDataLinearly()
    {
        var s = new LinearScale(dataMin: 0, dataMax: 100, pixelMin: 0, pixelMax: 200);

        s.ToPixels(0).Should().Be(0);
        s.ToPixels(50).Should().Be(100);
        s.ToPixels(100).Should().Be(200);
    }

    [Fact]
    public void FromPixels_ShouldInvertMapping()
    {
        var s = new LinearScale(0, 100, 0, 200);

        s.FromPixels(0).Should().Be(0);
        s.FromPixels(100).Should().Be(50);
        s.FromPixels(200).Should().Be(100);
    }
}
