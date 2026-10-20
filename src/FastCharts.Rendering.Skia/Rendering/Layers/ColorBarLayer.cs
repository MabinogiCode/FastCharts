using System;
using System.Globalization;
using FastCharts.Core.Axes;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Helpers;
using FastCharts.Core.Primitives;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers;

/// <summary>
/// Draws the color bar of the chart's color axis (P1-AX-COLOR) in the strip reserved by
/// <see cref="PlotLayout.ColorBarReservedWidth"/>: the gradient from the lowest value (bottom) to
/// the highest (top), aligned with the plot, with nicely rounded ticks, labels and optional title.
/// </summary>
internal sealed class ColorBarLayer : IRenderLayer
{
    private const float BarWidth = 14f;
    private const float StripPadding = 8f;
    private const float TickLength = 4f;
    private const float MinPixelsPerTick = 40f;

    private readonly NiceTicker _ticker = new NiceTicker();

    public void Render(RenderContext ctx)
    {
        var heatmap = PlotLayout.FindColorBarSeries(ctx.Model);
        if (heatmap == null)
        {
            return;
        }

        var pr = ctx.PlotRect;
        var barLeft = ctx.PixelWidth - (float)PlotLayout.ColorBarReservedWidth + StripPadding;
        var bar = new SKRect(barLeft, pr.Top, barLeft + BarWidth, pr.Bottom);
        if (bar.Height <= 0)
        {
            return;
        }

        var axis = heatmap.ColorAxis;
        FRange range;
        lock (heatmap.SyncRoot)
        {
            heatmap.TryGetValueRange(out var dataRange);
            range = axis.ResolveRange(dataRange);
        }

        DrawGradient(ctx.Canvas, bar, axis);
        ctx.Canvas.DrawRect(bar, ctx.Paints.Border);
        DrawTicks(ctx, bar, axis, range);

        if (!string.IsNullOrEmpty(axis.Title))
        {
            ctx.Canvas.DrawText(axis.Title, bar.Left, bar.Top - 4f, SKTextAlign.Left, ctx.Paints.TextFont, ctx.Paints.Text);
        }
    }

    private static void DrawGradient(SKCanvas canvas, SKRect bar, ColorAxis axis)
    {
        var stops = axis.ColorMap.Stops;
        var colors = new SKColor[stops.Count];
        var positions = new float[stops.Count];
        for (var i = 0; i < stops.Count; i++)
        {
            colors[i] = new SKColor(stops[i].R, stops[i].G, stops[i].B, stops[i].A);
            positions[i] = (float)i / (stops.Count - 1);
        }

        // Low values at the bottom, high values at the top
        using var shader = SKShader.CreateLinearGradient(new SKPoint(0, bar.Bottom), new SKPoint(0, bar.Top), colors, positions, SKShaderTileMode.Clamp);
        using var fill = new SKPaint { Shader = shader, Style = SKPaintStyle.Fill, IsAntialias = false };
        canvas.DrawRect(bar, fill);
    }

    private void DrawTicks(RenderContext ctx, SKRect bar, ColorAxis axis, FRange range)
    {
        var approxStep = range.Size / Math.Max(1.0, bar.Height / MinPixelsPerTick);
        var ticks = _ticker.GetTicks(range, approxStep);
        var tolerance = range.Size * 1e-9;
        foreach (var t in ticks)
        {
            if (t < range.Min - tolerance || t > range.Max + tolerance)
            {
                continue;
            }

            var y = bar.Bottom - (float)((t - range.Min) / range.Size * bar.Height);
            ctx.Canvas.DrawLine(bar.Right, y, bar.Right + TickLength, y, ctx.Paints.Tick);
            var label = axis.NumberFormatter != null ? axis.NumberFormatter.Format(t) : t.ToString("G", CultureInfo.InvariantCulture);
            ctx.Canvas.DrawText(label, bar.Right + TickLength + 2f, y + 4f, SKTextAlign.Left, ctx.Paints.TextFont, ctx.Paints.Text);
        }
    }
}
