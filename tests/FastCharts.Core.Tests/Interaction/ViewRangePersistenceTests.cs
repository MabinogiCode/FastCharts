using FastCharts.Core;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using FastCharts.Core.Interactivity;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;

using FluentAssertions;

using SkiaSharp;

using Xunit;

namespace FastCharts.Core.Tests.Interaction;

/// <summary>
/// Regression tests: the axes' visible range is the single source of truth, so zoom/pan made
/// through any entry point (API, behaviors, linked charts) must survive the next render.
/// Previously each render copied a separate viewport back onto the axes and undid them.
/// </summary>
public class ViewRangePersistenceTests
{
    private const int Width = 400;
    private const int Height = 300;

    [Fact]
    public void ZoomAtSurvivesRender()
    {
        // Arrange
        using var model = CreateModel();
        Render(model);

        // Act
        model.ZoomAt(0.5, 0.5, 50, 50);
        Render(model);

        // Assert
        model.XAxis.VisibleRange.Min.Should().BeApproximately(25, 1e-9);
        model.XAxis.VisibleRange.Max.Should().BeApproximately(75, 1e-9);
    }

    [Fact]
    public void PanSurvivesRender()
    {
        // Arrange
        using var model = CreateModel();
        Render(model);

        // Act
        model.Pan(10, 0);
        Render(model);

        // Assert
        model.XAxis.VisibleRange.Min.Should().BeApproximately(10, 1e-9);
        model.XAxis.VisibleRange.Max.Should().BeApproximately(110, 1e-9);
    }

    [Fact]
    public void ZoomRectangleSurvivesRender()
    {
        // Arrange
        using var model = CreateModel();
        Render(model);
        var behavior = new ZoomRectBehavior();
        var shift = new PointerModifiers { Shift = true };

        // Act
        behavior.OnEvent(model, new InteractionEvent(PointerEventType.Down, PointerButton.Left, shift, 100, 100, 0, Width, Height));
        behavior.OnEvent(model, new InteractionEvent(PointerEventType.Up, PointerButton.Left, shift, 200, 200, 0, Width, Height));
        var zoomed = model.XAxis.VisibleRange;
        Render(model);

        // Assert
        zoomed.Size.Should().BeLessThan(100);
        model.XAxis.VisibleRange.Should().Be(zoomed);
    }

    [Fact]
    public void ZoomWheelSurvivesRender()
    {
        // Arrange
        using var model = CreateModel();
        Render(model);
        var behavior = new ZoomWheelBehavior();

        // Act
        behavior.OnEvent(model, new InteractionEvent(PointerEventType.Wheel, PointerButton.None, new PointerModifiers(), 200, 150, 1, Width, Height));
        var zoomed = model.XAxis.VisibleRange;
        Render(model);

        // Assert
        zoomed.Size.Should().BeLessThan(100);
        model.XAxis.VisibleRange.Should().Be(zoomed);
    }

    [Fact]
    public void ZoomRectangleMovesSecondaryAxisProportionally()
    {
        // Arrange
        using var model = CreateModel();
        model.AddSeries(new LineSeries(new[] { new PointD(0, 1000), new PointD(100, 2000) }) { YAxisIndex = 1 });
        Render(model);
        var primaryBefore = model.YAxis.VisibleRange;
        var secondaryBefore = model.YAxisSecondary!.VisibleRange;
        var behavior = new ZoomRectBehavior();
        var shift = new PointerModifiers { Shift = true };

        // Act
        behavior.OnEvent(model, new InteractionEvent(PointerEventType.Down, PointerButton.Left, shift, 100, 60, 0, Width, Height));
        behavior.OnEvent(model, new InteractionEvent(PointerEventType.Up, PointerButton.Left, shift, 200, 160, 0, Width, Height));

        // Assert: same relative window on both Y axes
        var primary = model.YAxis.VisibleRange;
        var secondary = model.YAxisSecondary.VisibleRange;
        var primaryT0 = (primary.Min - primaryBefore.Min) / primaryBefore.Size;
        var secondaryT0 = (secondary.Min - secondaryBefore.Min) / secondaryBefore.Size;
        secondaryT0.Should().BeApproximately(primaryT0, 1e-9);
        (secondary.Size / secondaryBefore.Size).Should().BeApproximately(primary.Size / primaryBefore.Size, 1e-9);
    }

    [Fact]
    public void LinkedChartsStayInSyncAcrossRenders()
    {
        // Arrange
        using var a = CreateModel();
        using var b = CreateModel();
        using var link = new ChartLinkGroup();
        link.Add(a);
        link.Add(b);
        Render(a);
        Render(b);

        // Act: user pans chart A, then both charts redraw (B last)
        a.Viewport.Pan(10, 0);
        Render(a);
        Render(b);

        // Assert
        a.XAxis.VisibleRange.Min.Should().BeApproximately(10, 1e-9);
        b.XAxis.VisibleRange.Min.Should().BeApproximately(10, 1e-9);
        b.XAxis.VisibleRange.Max.Should().BeApproximately(110, 1e-9);
    }

    [Fact]
    public void ExplicitAxisRangeIsRendered()
    {
        // Arrange
        using var model = CreateModel();

        // Act
        model.XAxis.VisibleRange = new FRange(20, 40);
        Render(model);

        // Assert
        model.Viewport.X.Should().Be(new FRange(20, 40));
        model.XAxis.VisibleRange.Should().Be(new FRange(20, 40));
    }

    private static ChartModel CreateModel()
    {
        var model = new ChartModel();
        model.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(100, 100) }));
        return model;
    }

    private static void Render(ChartModel model)
    {
        using var bitmap = new SKBitmap(Width, Height);
        using var canvas = new SKCanvas(bitmap);
        new SkiaChartRenderer().Render(model, canvas, Width, Height);
    }
}
