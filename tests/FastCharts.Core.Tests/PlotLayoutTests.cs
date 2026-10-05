using FastCharts.Core.Helpers;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests;

/// <summary>
/// Renderer and interactions share <see cref="PlotLayout"/>: with a secondary Y axis the right
/// margin is widened for its labels, and hit-testing must use the same plot rectangle.
/// </summary>
public class PlotLayoutTests
{
    [Fact]
    public void ComputeWithoutSecondaryAxisUsesPlotMargins()
    {
        var area = PlotLayout.Compute(new Margins(48, 16, 16, 36), false, 500, 300);

        area.Left.Should().Be(48);
        area.Top.Should().Be(16);
        area.Width.Should().Be(500 - 48 - 16);
        area.Height.Should().Be(300 - 16 - 36);
    }

    [Fact]
    public void ComputeWithSecondaryAxisWidensRightMargin()
    {
        var area = PlotLayout.Compute(new Margins(48, 16, 16, 36), true, 500, 300);

        area.Width.Should().Be(500 - 48 - PlotLayout.SecondaryAxisMinRightMargin);
    }

    [Fact]
    public void ComputeClampsToEmptyArea()
    {
        var area = PlotLayout.Compute(new Margins(48, 16, 16, 36), false, 20, 20);

        area.IsEmpty.Should().BeTrue();
        area.Width.Should().Be(0);
    }

    [Fact]
    public void NearestPointMatchesRenderedPositionWithSecondaryAxis()
    {
        // Arrange: point at the right end of the X range, chart with a secondary axis
        using var model = new ChartModel();
        model.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(10, 10) }));
        model.AddSeries(new LineSeries(new[] { new PointD(0, 100), new PointD(10, 200) }) { YAxisIndex = 1 });
        var area = PlotLayout.Compute(model, 464, 152);

        // Act: cursor exactly where the renderer draws (10, 10): the plot's top-right corner
        var ev = new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), area.Right, area.Top, 0, 464, 152);
        new NearestPointBehavior { MaxPixelDistance = 2 }.OnEvent(model, ev);

        // Assert
        model.InteractionState!.ShowNearest.Should().BeTrue();
        model.InteractionState.NearestDataX.Should().Be(10);
    }
}
