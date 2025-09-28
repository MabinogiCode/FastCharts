using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests;

public class ChartModelTests
{
    [Fact]
    public void ChartModelShouldStartWithDefaultAxesAndViewport()
    {
        var m = new ChartModel();
        m.Axes.Should().HaveCount(2);
        m.Series.Should().BeEmpty();
        m.Viewport.X.Size.Should().Be(1);
        m.Viewport.Y.Size.Should().Be(1);
    }

    [Fact]
    public void AutoFitShouldUpdateDataRangesAndViewport()
    {
        var m = new ChartModel();
        m.AddSeries(new LineSeries(new[]
        {
            new PointD(0, 0),
            new PointD(10, 20)
        }));

        m.XAxis.DataRange.Min.Should().Be(0);
        m.XAxis.DataRange.Max.Should().Be(10);
        m.YAxis.DataRange.Min.Should().Be(0);
        m.YAxis.DataRange.Max.Should().Be(20);

        m.Viewport.X.Min.Should().Be(0);
        m.Viewport.X.Max.Should().Be(10);
        m.Viewport.Y.Min.Should().Be(0);
        m.Viewport.Y.Max.Should().Be(20);
    }

    [Fact]
    public void UpdateScalesShouldMapX0To0AndX100ToWidth()
    {
        var m = new ChartModel();
        m.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(100, 100) }));
        m.UpdateScales(widthPx: 200, heightPx: 100);

        m.XAxis.Scale.ToPixels(0).Should().Be(0);
        m.XAxis.Scale.ToPixels(100).Should().Be(200);
    }
}
