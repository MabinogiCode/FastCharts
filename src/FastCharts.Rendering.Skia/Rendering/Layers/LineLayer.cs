using System.Collections.Generic;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers;

internal sealed class LineLayer : ISeriesSubLayer
{
    public void Render(RenderContext ctx)
    {
        var model = ctx.Model;
        var pr = ctx.PlotRect;
        var palette = model.Theme.SeriesPalette;
        var paletteCount = palette?.Count ?? 0;
        var lineIndex = 0;

        foreach (var s in model.Series)
        {
            if (s is not LineSeries ls)
            {
                continue;
            }
            if (ls is AreaSeries or StepLineSeries)
            {
                continue; // rendered in dedicated layers
            }
            if (ls.IsEmpty || !ls.IsVisible)
            {
                continue;
            }

            var c = (paletteCount > 0 && lineIndex < paletteCount && palette != null) ? palette[lineIndex] : model.Theme.PrimarySeriesColor;
            using var sp = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)ls.StrokeThickness, Color = new SKColor(c.R, c.G, c.B, c.A) };
            using var path = new SKPath();

            // LTTB integration: renderers must consume GetRenderData(), never raw Data
            var plotPixelWidth = (int)pr.Width;
            var renderData = ls.GetRenderData(plotPixelWidth);

            var yAxis = (ls.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;

            // Project data points to pixels once; reused for path + markers
            var pixels = new SKPoint[renderData.Count];
            for (var i = 0; i < renderData.Count; i++)
            {
                pixels[i] = new SKPoint(
                    PixelMapper.X(renderData[i].X, model.XAxis, pr),
                    PixelMapper.Y(renderData[i].Y, yAxis, pr));
            }

            if (ls.Smoothing == LineSmoothing.Spline && pixels.Length >= 3)
            {
                BuildSplinePath(path, pixels);
            }
            else
            {
                BuildPolylinePath(path, pixels);
            }

            ctx.Canvas.Save();
            ctx.Canvas.ClipRect(pr);
            ctx.Canvas.DrawPath(path, sp);

            if (ls.ShowMarkers)
            {
                DrawMarkers(ctx.Canvas, ls, pixels, c.R, c.G, c.B, c.A);
            }

            ctx.Canvas.Restore();
            lineIndex++;
        }
    }

    private static void BuildPolylinePath(SKPath path, IReadOnlyList<SKPoint> pixels)
    {
        for (var i = 0; i < pixels.Count; i++)
        {
            if (i == 0)
            {
                path.MoveTo(pixels[i]);
            }
            else
            {
                path.LineTo(pixels[i]);
            }
        }
    }

    /// <summary>
    /// Smooth curve through all points: Catmull-Rom spline converted to cubic Beziers.
    /// Endpoints are duplicated so the curve starts and ends exactly on the data.
    /// </summary>
    private static void BuildSplinePath(SKPath path, IReadOnlyList<SKPoint> pixels)
    {
        path.MoveTo(pixels[0]);

        var count = pixels.Count;
        for (var i = 0; i < count - 1; i++)
        {
            var p0 = pixels[i == 0 ? 0 : i - 1];
            var p1 = pixels[i];
            var p2 = pixels[i + 1];
            var p3 = pixels[i + 2 < count ? i + 2 : count - 1];

            // Catmull-Rom to Bezier control points (tension 0.5)
            var c1 = new SKPoint(p1.X + (p2.X - p0.X) / 6f, p1.Y + (p2.Y - p0.Y) / 6f);
            var c2 = new SKPoint(p2.X - (p3.X - p1.X) / 6f, p2.Y - (p3.Y - p1.Y) / 6f);

            path.CubicTo(c1, c2, p2);
        }
    }

    private static void DrawMarkers(SKCanvas canvas, LineSeries ls, IReadOnlyList<SKPoint> pixels, byte r, byte g, byte b, byte a)
    {
        var size = (float)ls.MarkerSize;
        if (size < 1f)
        {
            size = 1f;
        }

        var half = size * 0.5f;
        using var mp = new SKPaint
        {
            IsAntialias = size > 3,
            Style = SKPaintStyle.Fill,
            StrokeWidth = (float)ls.StrokeThickness,
            Color = new SKColor(r, g, b, a)
        };

        for (var i = 0; i < pixels.Count; i++)
        {
            MarkerRenderer.Draw(canvas, ls.MarkerShape, pixels[i].X, pixels[i].Y, half, mp);
        }
    }
}
