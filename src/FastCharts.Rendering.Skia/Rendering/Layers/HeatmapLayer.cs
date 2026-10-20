using System;
using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers;

/// <summary>
/// Draws heatmaps (P2-HEATMAP). On linear axes the grid is colored once into an image (one pixel
/// per cell, cached per data version and color scale) and drawn scaled with nearest-neighbor
/// sampling: cells stay crisp and a frame costs a single image blit. Logarithmic axes, where cells
/// are not evenly sized on screen, fall back to one rectangle per cell.
/// </summary>
internal sealed class HeatmapLayer : ISeriesSubLayer
{
    private readonly Dictionary<HeatmapSeries, HeatmapImageCacheEntry> _cache = new();
    private readonly List<HeatmapSeries> _sweepList = new();

    public void Render(RenderContext ctx)
    {
        var model = ctx.Model;
        var pr = ctx.PlotRect;
        foreach (var s in model.Series)
        {
            if (s is not HeatmapSeries hs || hs.IsEmpty || !hs.IsVisible)
            {
                continue;
            }

            var yAxis = (hs.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;
            var alpha = (byte)(RenderMath.Clamp01(hs.Opacity) * 255);
            using var paint = new SKPaint { Color = new SKColor(255, 255, 255, alpha), IsAntialias = false };

            ctx.Canvas.Save();
            ctx.Canvas.ClipRect(pr);

            // Values may be replaced from another thread: read them under the series lock
            lock (hs.SyncRoot)
            {
                hs.TryGetValueRange(out var dataRange);
                var range = hs.ColorAxis.ResolveRange(dataRange);

                if (AxisCoordinates.IsLogarithmic(model.XAxis, out _) || AxisCoordinates.IsLogarithmic(yAxis, out _))
                {
                    DrawCells(ctx, hs, yAxis, range, alpha);
                }
                else
                {
                    var entry = GetOrBuildImage(hs, range);
                    entry.Seen = true;
                    var left = PixelMapper.X(hs.XRange.Min, model.XAxis, pr);
                    var right = PixelMapper.X(hs.XRange.Max, model.XAxis, pr);
                    var top = PixelMapper.Y(hs.YRange.Max, yAxis, pr);
                    var bottom = PixelMapper.Y(hs.YRange.Min, yAxis, pr);
                    var dest = new SKRect(Math.Min(left, right), Math.Min(top, bottom), Math.Max(left, right), Math.Max(top, bottom));
                    ctx.Canvas.DrawImage(entry.Image!, dest, new SKSamplingOptions(SKFilterMode.Nearest), paint);
                }
            }

            ctx.Canvas.Restore();
        }

        SweepUnseen();
    }

    private HeatmapImageCacheEntry GetOrBuildImage(HeatmapSeries hs, FRange range)
    {
        if (!_cache.TryGetValue(hs, out var entry))
        {
            entry = new HeatmapImageCacheEntry();
            _cache[hs] = entry;
        }

        var axis = hs.ColorAxis;
        var missing = axis.MissingValueColor;
        var upToDate =
            entry.Image != null &&
            entry.DataVersion == hs.DataVersion &&
            ReferenceEquals(entry.ColorMap, axis.ColorMap) &&
            entry.RangeMin.Equals(range.Min) &&
            entry.RangeMax.Equals(range.Max) &&
            SameColor(entry.MissingValueColor, missing);
        if (upToDate)
        {
            return entry;
        }

        var rows = hs.RowCount;
        var columns = hs.ColumnCount;
        var values = hs.Values;
        var pixels = new byte[rows * columns * 4];
        for (var row = 0; row < rows; row++)
        {
            // Image rows go top-down, grid rows bottom-up
            var offset = (rows - 1 - row) * columns * 4;
            for (var column = 0; column < columns; column++)
            {
                var color = axis.GetColor(values[row, column], range);
                pixels[offset++] = color.R;
                pixels[offset++] = color.G;
                pixels[offset++] = color.B;
                pixels[offset++] = color.A;
            }
        }

        entry.Image?.Dispose();
        entry.Image = SKImage.FromPixelCopy(new SKImageInfo(columns, rows, SKColorType.Rgba8888, SKAlphaType.Unpremul), pixels);
        entry.DataVersion = hs.DataVersion;
        entry.ColorMap = axis.ColorMap;
        entry.RangeMin = range.Min;
        entry.RangeMax = range.Max;
        entry.MissingValueColor = missing;
        return entry;
    }

    private static void DrawCells(RenderContext ctx, HeatmapSeries hs, IAxis<double> yAxis, FRange range, byte alpha)
    {
        var model = ctx.Model;
        var pr = ctx.PlotRect;
        var rows = hs.RowCount;
        var columns = hs.ColumnCount;

        // Project the cell edges once: (columns + 1) + (rows + 1) mappings instead of 4 per cell
        var xs = new float[columns + 1];
        for (var i = 0; i <= columns; i++)
        {
            xs[i] = PixelMapper.X(hs.XRange.Min + (hs.XRange.Size * i / columns), model.XAxis, pr);
        }

        var ys = new float[rows + 1];
        for (var i = 0; i <= rows; i++)
        {
            ys[i] = PixelMapper.Y(hs.YRange.Min + (hs.YRange.Size * i / rows), yAxis, pr);
        }

        using var cellPaint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = false };
        var values = hs.Values;
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var color = hs.ColorAxis.GetColor(values[row, column], range);
                if (color.A == 0)
                {
                    continue;
                }

                cellPaint.Color = new SKColor(color.R, color.G, color.B, (byte)(color.A * alpha / 255));
                var rect = new SKRect(
                    Math.Min(xs[column], xs[column + 1]),
                    Math.Min(ys[row], ys[row + 1]),
                    Math.Max(xs[column], xs[column + 1]),
                    Math.Max(ys[row], ys[row + 1]));
                ctx.Canvas.DrawRect(rect, cellPaint);
            }
        }
    }

    private void SweepUnseen()
    {
        _sweepList.Clear();
        foreach (var pair in _cache)
        {
            if (!pair.Value.Seen)
            {
                _sweepList.Add(pair.Key);
            }
            else
            {
                pair.Value.Seen = false;
            }
        }

        for (var i = 0; i < _sweepList.Count; i++)
        {
            _cache[_sweepList[i]].Dispose();
            _cache.Remove(_sweepList[i]);
        }
    }

    private static bool SameColor(ColorRgba a, ColorRgba b)
    {
        return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
    }
}
