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
        private readonly IRenderLayer _grid = new GridLayer();
        private readonly IRenderLayer _series = new SeriesLayer();
        private readonly IRenderLayer _axesTicks = new AxesTicksLayer();
        private readonly IRenderLayer _legend = new LegendLayer();

        public void Render(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight)
        {
            if (model == null || canvas == null) return;
            var theme = model.Theme;

            var m = model.PlotMargins;
            float left = (float)m.Left;
            float top = (float)m.Top;
            float right = (float)m.Right;
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

            _grid.Render(ctx);      // grid (clipped)
            _series.Render(ctx);    // series (clipped internally)
            _axesTicks.Render(ctx); // axes + ticks + border
            _legend.Render(ctx);    // legend box
            RenderOverlay(ctx);     // crosshair / tooltip
        }

        private void RenderOverlay(RenderContext ctx)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var st = model.InteractionState;
            if (st == null) return;

            // Selection rectangle
            if (st.ShowSelectionRect)
            {
                float x1 = (float)st.SelX1, y1 = (float)st.SelY1, x2 = (float)st.SelX2, y2 = (float)st.SelY2;
                using var selFill = new SKPaint { Color = new SKColor(30, 120, 220, 40), Style = SKPaintStyle.Fill, IsAntialias = true };
                using var selStroke = new SKPaint { Color = new SKColor(30, 120, 220, 160), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
                var rr = SKRect.Create(System.Math.Min(x1, x2), System.Math.Min(y1, y2), System.Math.Abs(x2 - x1), System.Math.Abs(y2 - y1));
                // clip to plot
                ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr);
                ctx.Canvas.DrawRect(rr, selFill);
                ctx.Canvas.DrawRect(rr, selStroke);
                ctx.Canvas.Restore();
            }

            if (!st.ShowCrosshair) return;

            float cx = (float)st.PixelX;
            float cy = (float)st.PixelY;
            if (cx < pr.Left) cx = pr.Left; else if (cx > pr.Right) cx = pr.Right;
            if (cy < pr.Top) cy = pr.Top; else if (cy > pr.Bottom) cy = pr.Bottom;

            using var cross = new SKPaint { Color = new SKColor(0,0,0,80), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = false };
            using var tipBg = new SKPaint { Color = new SKColor(255,255,255,230), Style = SKPaintStyle.Fill, IsAntialias = true };
            using var tipBd = new SKPaint { Color = new SKColor(0,0,0,120), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
            using var tipTx = new SKPaint { Color = new SKColor(30,30,30,255), Style = SKPaintStyle.Fill, IsAntialias = true, TextSize = (float)model.Theme.LabelTextSize };

            ctx.Canvas.Save();
            ctx.Canvas.ClipRect(pr);
            ctx.Canvas.DrawLine(pr.Left, cy, pr.Right, cy, cross);
            ctx.Canvas.DrawLine(cx, pr.Top, cx, pr.Bottom, cross);
            ctx.Canvas.Restore();

            if (string.IsNullOrEmpty(st.TooltipText)) return;
            var lines = st.TooltipText.Split('\n');
            float maxW = 0;
            foreach (var l in lines)
            {
                float w = tipTx.MeasureText(l);
                if (w > maxW) maxW = w;
            }
            float lineH = tipTx.TextSize + 2;
            float pad = 6;
            float boxW = maxW + pad * 2;
            float boxH = lineH * lines.Length + pad * 2;
            float bx = cx + 12;
            float by = cy - boxH - 12;
            if (bx + boxW > pr.Right) bx = pr.Right - boxW - 1;
            if (by < pr.Top) by = cy + 12;
            var rect = new SKRect(bx, by, bx + boxW, by + boxH);
            ctx.Canvas.DrawRect(rect, tipBg);
            ctx.Canvas.DrawRect(rect, tipBd);
            float tx = bx + pad;
            float ty = by + pad + tipTx.TextSize;
            foreach (var l in lines)
            {
                ctx.Canvas.DrawText(l, tx, ty, tipTx);
                ty += lineH;
            }
        }

        internal static FastCharts.Core.Primitives.ColorRgba ResolveSeriesColorStatic(ChartModel model, object seriesRef, System.Collections.Generic.IReadOnlyList<FastCharts.Core.Primitives.ColorRgba> palette)
        {
            var primary = model.Theme.PrimarySeriesColor;
            if (palette == null || palette.Count == 0) return primary;
            if (seriesRef is BandSeries band)
            {
                int idx = model.Series.OfType<BandSeries>().ToList().IndexOf(band);
                if (band.PaletteIndex.HasValue) idx = band.PaletteIndex.Value;
                return idx >= 0 && idx < palette.Count ? palette[idx] : primary;
            }
            if (seriesRef is ScatterSeries sc)
            {
                int idx = model.Series.OfType<ScatterSeries>().ToList().IndexOf(sc);
                if (sc.PaletteIndex.HasValue) idx = sc.PaletteIndex.Value;
                return idx >= 0 && idx < palette.Count ? palette[idx] : primary;
            }
            if (seriesRef is AreaSeries area)
            {
                int idx = model.Series.OfType<LineSeries>().ToList().IndexOf(area);
                if (area.PaletteIndex.HasValue) idx = area.PaletteIndex.Value;
                return idx >= 0 && idx < palette.Count ? palette[idx] : primary;
            }
            if (seriesRef is LineSeries ls)
            {
                int idx = model.Series.OfType<LineSeries>().ToList().IndexOf(ls);
                if (ls.PaletteIndex.HasValue) idx = ls.PaletteIndex.Value;
                return idx >= 0 && idx < palette.Count ? palette[idx] : primary;
            }
            return primary;
        }
    }
}
