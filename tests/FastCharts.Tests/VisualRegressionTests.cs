using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Tests.Helpers;

using FluentAssertions;

using Xunit;

namespace FastCharts.Tests;

/// <summary>
/// Visual-regression tests. Deterministic charts are rendered and compared
/// against committed baseline images. The first run creates the baselines
/// (review and commit the generated PNGs); later runs fail if the rendered
/// output drifts beyond tolerance.
/// </summary>
public class VisualRegressionTests
{
    [Fact]
    public void LineChartMatchesBaseline()
    {
        AssertMatchesBaseline(BuildLineChart(), "line-chart");
    }

    [Fact]
    public void BarChartMatchesBaseline()
    {
        AssertMatchesBaseline(BuildBarChart(), "bar-chart");
    }

    [Fact]
    public void ScatterChartMatchesBaseline()
    {
        AssertMatchesBaseline(BuildScatterChart(), "scatter-chart");
    }

    private static void AssertMatchesBaseline(ChartModel model, string baselineName)
    {
        var (bitmap, _) = ChartRenderTestHelper.Render(model, 480, 320);
        using (bitmap)
        {
            var result = VisualRegressionHarness.Compare(bitmap, baselineName);
            result.IsMatch.Should().BeTrue(
                $"the render should match baseline '{baselineName}' " +
                $"(differing pixels: {result.DifferingFraction:P2}, " +
                $"baseline created this run: {result.BaselineCreated})");
        }
    }

    private static ChartModel BuildLineChart()
    {
        var model = new ChartModel { Title = "Line" };
        model.AddSeries(new LineSeries(new[]
        {
            new PointD(0, 10), new PointD(1, 35), new PointD(2, 22),
            new PointD(3, 48), new PointD(4, 30), new PointD(5, 55)
        })
        {
            Title = "Series",
            StrokeThickness = 2.0
        });
        return model;
    }

    private static ChartModel BuildBarChart()
    {
        var model = new ChartModel { Title = "Bars" };
        model.AddSeries(new BarSeries(new[]
        {
            new BarPoint(0, 20), new BarPoint(1, 45),
            new BarPoint(2, 30), new BarPoint(3, 55)
        })
        {
            Title = "Bars"
        });
        return model;
    }

    private static ChartModel BuildScatterChart()
    {
        var model = new ChartModel { Title = "Scatter" };
        model.AddSeries(new ScatterSeries(new[]
        {
            new PointD(1, 2), new PointD(3, 5), new PointD(5, 1),
            new PointD(7, 8), new PointD(9, 4)
        })
        {
            Title = "Points",
            MarkerSize = 6.0
        });
        return model;
    }
}
