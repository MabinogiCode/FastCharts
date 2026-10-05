using FastCharts.Core;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.Interaction.Behaviors;

public class NearestPointBehaviorTests
{
    // Default margins (48, 16, 16, 36) on a 464 x 152 surface give a 400 x 100 plot
    private const double SurfaceWidth = 464;
    private const double SurfaceHeight = 152;

    [Fact]
    public void SnapsToVisiblePointNearCursor()
    {
        // Arrange
        using var model = CreateModel(new LineSeries(new[] { new PointD(0, 0), new PointD(5, 5), new PointD(10, 10) }));

        // Act: cursor on (5, 5)
        var handled = Move(model, 48 + 200, 16 + 50);

        // Assert
        handled.Should().BeTrue();
        model.InteractionState!.ShowNearest.Should().BeTrue();
        model.InteractionState.NearestDataX.Should().Be(5);
        model.InteractionState.NearestDataY.Should().Be(5);
    }

    [Fact]
    public void IgnoresHiddenSeries()
    {
        // Arrange
        var hidden = new LineSeries(new[] { new PointD(5, 5) }) { IsVisible = false };
        using var model = CreateModel(new LineSeries(new[] { new PointD(0, 0), new PointD(10, 10) }), hidden);

        // Act: cursor exactly on the hidden point
        Move(model, 48 + 200, 16 + 50);

        // Assert
        model.InteractionState!.ShowNearest.Should().BeFalse();
    }

    [Fact]
    public void IgnoresPointsOutsideVisibleRange()
    {
        // Arrange: the only point near the right edge is far outside the visible X range
        using var model = CreateModel(new LineSeries(new[] { new PointD(0, 0), new PointD(1000, 5) }));
        model.XAxis.VisibleRange = new FRange(0, 10);
        model.YAxis.VisibleRange = new FRange(0, 10);

        // Act: cursor on the right border, where the clamped projection used to land
        Move(model, 48 + 400, 16 + 50);

        // Assert
        model.InteractionState!.ShowNearest.Should().BeFalse();
    }

    [Fact]
    public void UsesSecondaryAxisForSecondarySeries()
    {
        // Arrange
        using var model = CreateModel(
            new LineSeries(new[] { new PointD(0, 0), new PointD(10, 10) }),
            new LineSeries(new[] { new PointD(0, 1000), new PointD(5, 1500), new PointD(10, 2000) }) { YAxisIndex = 1 });

        // Act: (5, 1500) sits at mid-height on the secondary axis
        Move(model, 48 + 200, 16 + 50);

        // Assert
        model.InteractionState!.ShowNearest.Should().BeTrue();
        model.InteractionState.NearestDataY.Should().Be(1500);
        model.InteractionState.NearestYAxisIndex.Should().Be(1);
    }

    private static ChartModel CreateModel(params SeriesBase[] series)
    {
        var model = new ChartModel();
        foreach (var s in series)
        {
            model.AddSeries(s);
        }

        return model;
    }

    private static bool Move(ChartModel model, double x, double y)
    {
        var ev = new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), x, y, 0, SurfaceWidth, SurfaceHeight);
        return new NearestPointBehavior().OnEvent(model, ev);
    }
}
