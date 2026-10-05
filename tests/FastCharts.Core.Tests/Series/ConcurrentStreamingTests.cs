using System;
using System.Threading;
using System.Threading.Tasks;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;

using FluentAssertions;

using SkiaSharp;

using Xunit;

namespace FastCharts.Core.Tests.Series;

/// <summary>
/// Producers may append on a background thread while the UI renders and handles the mouse:
/// series mutations and reads are serialized on <see cref="SeriesBase.SyncRoot"/>.
/// </summary>
public class ConcurrentStreamingTests
{
    [Fact]
    public async Task AppendingFromBackgroundThreadWhileRenderingDoesNotThrow()
    {
        // Arrange
        using var model = new ChartModel();
        var streaming = new StreamingLineSeries(maxPointCount: 3000);
        var plain = new LineSeries();
        model.AddSeries(streaming);
        model.AddSeries(plain);
        using var stop = new CancellationTokenSource();
        var producer = Task.Run(() =>
        {
            var i = 0;
            while (!stop.IsCancellationRequested)
            {
                streaming.AppendPoint(new PointD(i, Math.Sin(i * 0.01)));
                plain.AddPoint(new PointD(i, Math.Cos(i * 0.01)));
                i++;
            }
        });

        var renderer = new SkiaChartRenderer();
        var nearest = new NearestPointBehavior();
        var tooltip = new MultiSeriesTooltipBehavior();
        using var bitmap = new SKBitmap(400, 300);
        using var canvas = new SKCanvas(bitmap);

        // Act: render and hit-test while the producer runs
        var act = () =>
        {
            for (var frame = 0; frame < 200; frame++)
            {
                model.AutoFitDataRange();
                renderer.Render(model, canvas, 400, 300);
                model.InteractionState ??= new InteractionState();
                model.InteractionState.DataX = model.XAxis.VisibleRange.Min + (model.XAxis.VisibleRange.Size / 2);
                var move = new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), 200, 150, 0, 400, 300);
                nearest.OnEvent(model, move);
                tooltip.OnEvent(model, move);
            }
        };

        // Assert
        try
        {
            act.Should().NotThrow();
        }
        finally
        {
            stop.Cancel();
            await producer;
        }

        streaming.PointCount.Should().BeLessThanOrEqualTo(3000);
    }
}
