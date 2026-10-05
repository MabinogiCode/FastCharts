using System.IO;
using System.Linq;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.Interaction.Behaviors;

/// <summary>
/// The metrics overlay used to be documented (F3/F4/F5) but was never drawn by the renderer.
/// </summary>
public class MetricsOverlayRenderingTests
{
    [Fact]
    public void OverlayIsDrawnWhenBehaviorIsPresent()
    {
        // Arrange
        using var model = CreateModel();
        var renderer = new SkiaChartRenderer();
        var without = ExportBytes(renderer, model);
        var overlay = new MetricsOverlayBehavior { ShowDetailed = false };
        model.Behaviors.Add(overlay);

        // Act
        var with = ExportBytes(renderer, model);

        // Assert
        with.Should().NotEqual(without);
        overlay.Metrics.TotalFrames.Should().BeGreaterThan(0);
        overlay.GetDisplayText().Should().Contain("100 pts");
    }

    [Fact]
    public void F3HidesTheOverlay()
    {
        // Arrange
        using var model = CreateModel();
        var renderer = new SkiaChartRenderer();
        var without = ExportBytes(renderer, model);
        var overlay = new MetricsOverlayBehavior();
        model.Behaviors.Add(overlay);

        // Act
        var handled = overlay.OnEvent(model, new InteractionEvent(PointerEventType.KeyDown, PointerButton.None, new PointerModifiers(), 0, 0, 0, 640, 400, "F3"));
        var hidden = ExportBytes(renderer, model);

        // Assert
        handled.Should().BeTrue();
        overlay.IsVisible.Should().BeFalse();
        hidden.Should().Equal(without);
    }

    [Fact]
    public void DetailedTextHasNoPlaceholderCharacters()
    {
        var overlay = new MetricsOverlayBehavior { ShowDetailed = true };

        overlay.GetDisplayText().Should().NotContain("?");
    }

    private static ChartModel CreateModel()
    {
        var model = new ChartModel();
        model.AddSeries(new LineSeries(Enumerable.Range(0, 100).Select(i => new PointD(i, i % 10))));
        return model;
    }

    private static byte[] ExportBytes(SkiaChartRenderer renderer, ChartModel model)
    {
        using var stream = new MemoryStream();
        renderer.ExportPng(model, stream, 640, 400);
        return stream.ToArray();
    }
}
