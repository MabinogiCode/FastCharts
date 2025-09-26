using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Legend;
using SkiaSharp;
using FastCharts.Rendering.Skia.Rendering;
using FastCharts.Rendering.Skia.Rendering.Layers;

namespace FastCharts.Rendering.Skia
{
    public sealed class SkiaChartRenderer : IRenderer<SKCanvas>
    {
        private readonly GridLayer _grid = new();
        private readonly SeriesLayer _series = new();
        private readonly AxesTicksLayer _axesTicks = new();
        private readonly LegendLayer _legend = new();

        public void Render(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight)
        {
            if (model == null || canvas == null)
            {
                return;
            }
            var theme = model.Theme;
            var m = model.PlotMargins;
            float left = (float)m.Left;
            float top = (float)m.Top;
            // Auto-extend right margin if secondary Y axis is present (reserve space for labels)
            double rightBase = m.Right;
            if (model.YAxisSecondary != null)
            {
                rightBase = System.Math.Max(rightBase, 48); // ensure sufficient space for secondary labels
            }
            float right = (float)rightBase;
            float bottom = (float)m.Bottom;
            float plotW = (float)System.Math.Max(0, pixelWidth - (left + right));
            float plotH = (float)System.Math.Max(0, pixelHeight - (top + bottom));
            var plotRect = new SKRect(left, top, left + plotW, top + plotH);
            canvas.Clear(new SKColor(theme.SurfaceBackgroundColor.R, theme.SurfaceBackgroundColor.G, theme.SurfaceBackgroundColor.B, theme.SurfaceBackgroundColor.A));
            model.UpdateScales((int)plotW, (int)plotH);
            using var bg = new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(theme.PlotBackgroundColor.R, theme.PlotBackgroundColor.G, theme.PlotBackgroundColor.B, theme.PlotBackgroundColor.A) };
            canvas.DrawRect(plotRect, bg);
            using var paints = SkiaPaintPack.Create(theme);
            var ctx = new RenderContext(model, canvas, plotRect, paints, pixelWidth, pixelHeight);
            _grid.Render(ctx);
            _series.Render(ctx);
            _axesTicks.Render(ctx);
            _legend.Render(ctx);
            RenderOverlay(ctx);
        }

        private static void RenderOverlay(RenderContext ctx)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var st = model.InteractionState;
            if (st == null)
            {
                return;
            }
            var tooltipSeries = st.TooltipSeries; // local snapshot
            if (tooltipSeries == null)
            {
                return;
            }

            // Selection rectangle
            if (st.ShowSelectionRect)
            {
                float x1 = (float)st.SelX1, y1 = (float)st.SelY1, x2 = (float)st.SelX2, y2 = (float)st.SelY2;
                using var selFill = new SKPaint { Color = new SKColor(30, 120, 220, 40), Style = SKPaintStyle.Fill, IsAntialias = true };
                using var selStroke = new SKPaint { Color = new SKColor(30, 120, 220, 160), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
                var rr = SKRect.Create(System.Math.Min(x1, x2), System.Math.Min(y1, y2), System.Math.Abs(x2 - x1), System.Math.Abs(y2 - y1));
                ctx.Canvas.Save();
                ctx.Canvas.ClipRect(pr);
                ctx.Canvas.DrawRect(rr, selFill);
                ctx.Canvas.DrawRect(rr, selStroke);
                ctx.Canvas.Restore();
            }

            // Nearest-point highlight
            if (st.ShowNearest)
            {
                float px = PixelMapper.X(st.NearestDataX, ctx.Model.XAxis, pr);
                float py = PixelMapper.Y(st.NearestDataY, ctx.Model.YAxis, pr);
                using var npStroke = new SKPaint { Color = new SKColor(255, 80, 80, 220), Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
                using var npFill = new SKPaint { Color = new SKColor(255, 80, 80, 120), Style = SKPaintStyle.Fill, IsAntialias = true };
                ctx.Canvas.Save();
                ctx.Canvas.ClipRect(pr);
                ctx.Canvas.DrawCircle(px, py, 6, npFill);
                ctx.Canvas.DrawCircle(px, py, 6, npStroke);
                ctx.Canvas.Restore();
            }

            if (!st.ShowCrosshair)
            {
                return;
            }

            float cx = (float)st.PixelX;
            float cy = (float)st.PixelY;
            if (cx < pr.Left) { cx = pr.Left; } else if (cx > pr.Right) { cx = pr.Right; }
            if (cy < pr.Top) { cy = pr.Top; } else if (cy > pr.Bottom) { cy = pr.Bottom; }

            using var cross = new SKPaint { Color = new SKColor(0, 0, 0, 80), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = false };
            using var tipBg = new SKPaint { Color = new SKColor(255, 255, 255, 230), Style = SKPaintStyle.Fill, IsAntialias = true };
            using var tipBd = new SKPaint { Color = new SKColor(0, 0, 0, 120), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
            using var tipTx = new SKPaint { Color = new SKColor(30, 30, 30, 255), Style = SKPaintStyle.Fill, IsAntialias = true, TextSize = (float)model.Theme.LabelTextSize };
            var palette = model.Theme.SeriesPalette; // cache palette (may be null)
            ctx.Canvas.Save();
            ctx.Canvas.ClipRect(pr);
            ctx.Canvas.DrawLine(pr.Left, cy, pr.Right, cy, cross);
            ctx.Canvas.DrawLine(cx, pr.Top, cx, pr.Bottom, cross);
            ctx.Canvas.Restore();
            // Prefer aggregated multi-series tooltip
            string[] lines;
            if (tooltipSeries.Count > 0)
            {
                var header = st.TooltipText?.Split('\n').FirstOrDefault();
                var body = tooltipSeries.Select(v => v.Title + ": " + v.Y);
                lines = (header != null ? new[] { header }.Concat(body) : body).ToArray();
            }
            else if (!string.IsNullOrEmpty(st.TooltipText))
            {
                lines = (st.TooltipText ?? string.Empty).Split('\n');
            }
            else
            {
                return;
            }
            float maxW = 0;
            foreach (var l in lines)
            {
                float w = tipTx.MeasureText(l);
                if (w > maxW)
                {
                    maxW = w;
                }
            }
            float lineH = tipTx.TextSize + 2;
            float pad = 6;
            bool showSwatches = tooltipSeries.Count > 0;
            float swatchSize = showSwatches ? tipTx.TextSize * 0.6f : 0f;
            float swatchGap = showSwatches ? 4f : 0f;
            float extra = showSwatches ? (swatchSize + swatchGap) : 0f;
            float boxW = maxW + pad * 2 + extra;
            float boxH = lineH * lines.Length + pad * 2;
            float bx = cx + 12;
            float by = cy - boxH - 12;
            if (bx + boxW > pr.Right) { bx = pr.Right - boxW - 1; }
            if (by < pr.Top) { by = cy + 12; }
            var rect = new SKRect(bx, by, bx + boxW, by + boxH);
            ctx.Canvas.DrawRect(rect, tipBg);
            ctx.Canvas.DrawRect(rect, tipBd);
            float ty = by + pad + tipTx.TextSize;
            // Obtain font metrics once for vertical alignment
            tipTx.GetFontMetrics(out var fm);
            float textHeight = fm.Descent - fm.Ascent; // positive height
            for (int i = 0; i < lines.Length; i++)
            {
                float tx = bx + pad;
                if (showSwatches && i > 0 && i - 1 < tooltipSeries.Count)
                {
                    var sv = tooltipSeries[i - 1];
                    var col = model.Theme.PrimarySeriesColor;
                    if (palette != null && sv.PaletteIndex.HasValue && sv.PaletteIndex.Value >= 0 && sv.PaletteIndex.Value < palette.Count)
                    {
                        col = palette[sv.PaletteIndex.Value];
                    }
                    using var sw = new SKPaint { Color = new SKColor(col.R, col.G, col.B, col.A), Style = SKPaintStyle.Fill, IsAntialias = true };
                    float topText = ty + fm.Ascent;
                    float verticalPadding = (textHeight - swatchSize) * 0.5f;
                    float swTop = topText + verticalPadding;
                    var swRect = new SKRect(tx, swTop, tx + swatchSize, swTop + swatchSize);
                    ctx.Canvas.DrawRect(swRect, sw);
                    tx += swatchSize + swatchGap;
                }
                ctx.Canvas.DrawText(lines[i], tx, ty, tipTx);
                ty += lineH;
            }
        }

        internal static FastCharts.Core.Primitives.ColorRgba ResolveSeriesColorStatic(ChartModel model, object seriesRef, System.Collections.Generic.IReadOnlyList<FastCharts.Core.Primitives.ColorRgba> palette)
        {
            var primary = model.Theme.PrimarySeriesColor;
            if (palette == null || palette.Count == 0)
            {
                return primary;
            }
            if (seriesRef is BandSeries band)
            {
                int idx = model.Series.OfType<BandSeries>().ToList().IndexOf(band);
                if (band.PaletteIndex.HasValue) idx = band.PaletteIndex.Value;
                return (idx >= 0 && idx < palette.Count) ? palette[idx] : primary;
            }
            if (seriesRef is ScatterSeries sc)
            {
                int idx = model.Series.OfType<ScatterSeries>().ToList().IndexOf(sc);
                if (sc.PaletteIndex.HasValue) idx = sc.PaletteIndex.Value;
                return (idx >= 0 && idx < palette.Count) ? palette[idx] : primary;
            }
            if (seriesRef is AreaSeries area)
            {
                int idx = model.Series.OfType<LineSeries>().ToList().IndexOf(area);
                if (area.PaletteIndex.HasValue) idx = area.PaletteIndex.Value;
                return (idx >= 0 && idx < palette.Count) ? palette[idx] : primary;
            }
            if (seriesRef is LineSeries ls)
            {
                int idx = model.Series.OfType<LineSeries>().ToList().IndexOf(ls);
                if (ls.PaletteIndex.HasValue) idx = ls.PaletteIndex.Value;
                return (idx >= 0 && idx < palette.Count) ? palette[idx] : primary;
            }
            return primary;
        }
    }
}
