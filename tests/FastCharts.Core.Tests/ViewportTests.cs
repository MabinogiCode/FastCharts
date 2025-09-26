using FastCharts.Core.Interactivity;
using FastCharts.Core.Primitives;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class ViewportTests
{
    [Fact]
    public void PanShouldShiftBothAxesByGivenDelta()
    {
        var vp = new Viewport(new FRange(0, 10), new FRange(0, 5));

        vp.Pan(dxData: 2, dyData: -1);

        vp.X.Min.Should().Be(2);
        vp.X.Max.Should().Be(12);
        vp.Y.Min.Should().Be(-1);
        vp.Y.Max.Should().Be(4);
    }

    [Fact]
    public void ZoomShouldContractAroundPivotWhenScaleGreaterThan1()
    {
        var vp = new Viewport(new FRange(0, 10), new FRange(0, 10));
        var pivot = new PointD(5, 5);

        vp.Zoom(scaleX: 2, scaleY: 2, pivotData: pivot);

        vp.X.Min.Should().Be(2.5);
        vp.X.Max.Should().Be(7.5);
        vp.Y.Min.Should().Be(2.5);
        vp.Y.Max.Should().Be(7.5);
    }

    [Fact]
    public void ZoomShouldExpandAroundPivotWhenScaleBetween0And1()
    {
        var vp = new Viewport(new FRange(0, 10), new FRange(0, 10));
        var pivot = new PointD(5, 5);

        vp.Zoom(scaleX: 0.5, scaleY: 0.5, pivotData: pivot);

        vp.X.Min.Should().Be(-5);
        vp.X.Max.Should().Be(15);
        vp.Y.Min.Should().Be(-5);
        vp.Y.Max.Should().Be(15);
    }
}
