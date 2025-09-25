using SkiaSharp;
using System.Linq;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class LegendLayer : IRenderLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model;
            if (!model.Legend.IsVisible) return;
            if (model.Legend.Items.Count == 0) return;
            var theme = model.Theme;
            var palette = theme.SeriesPalette;
            var paints = ctx.Paints;
            var c = ctx.Canvas;

            var labelColor = new SkiaSharp.SKColor(theme.LabelColor.R, theme.LabelColor.G, theme.LabelColor.B, theme.LabelColor.A);
            double lum = (0.2126 * labelColor.Red + 0.7152 * labelColor.Green + 0.0722 * labelColor.Blue) / 255.0;

            float padding = 6f, swatch = 12f, gap = 6f;
            float lineH = paints.Text.TextSize + 4f;
            float maxText = 0f;
            foreach (var item in model.Legend.Items)
            {
                float w = paints.Text.MeasureText(item.Title ?? string.Empty);
                if (w > maxText) maxText = w;
            }
            float boxW = padding * 2 + swatch + gap + maxText;
            float boxH = padding * 2 + lineH * model.Legend.Items.Count;
            float bx = ctx.PixelWidth - boxW - 8f;
            float by = 8f;
            var rect = new SkiaSharp.SKRect(bx, by, bx + boxW, by + boxH);

            SkiaSharp.SKColor bg = lum > 0.55 ? new SkiaSharp.SKColor(20,20,20,210) : new SkiaSharp.SKColor(255,255,255,210);
            SkiaSharp.SKColor bd = lum > 0.55 ? new SkiaSharp.SKColor(90,90,90,230) : new SkiaSharp.SKColor(0,0,0,140);
            using var bgPaint = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Fill, Color = bg, IsAntialias = true };
            using var bdPaint = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Stroke, Color = bd, StrokeWidth = 1, IsAntialias = true };
            using var swPaint = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Fill, IsAntialias = true };
            using var textMuted = new SkiaSharp.SKPaint { IsAntialias = true, TextSize = paints.Text.TextSize, Color = new SkiaSharp.SKColor(labelColor.Red, labelColor.Green, labelColor.Blue, (byte)(labelColor.Alpha * 0.4)) };
            using var swMuted = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Fill, IsAntialias = true };

            c.DrawRect(rect, bgPaint);
            c.DrawRect(rect, bdPaint);

            model.InteractionState ??= new FastCharts.Core.Interaction.InteractionState();
            var hits = model.InteractionState.LegendHits;
            hits.Clear();

            float y = by + padding + paints.Text.TextSize;
            foreach (var item in model.Legend.Items)
            {
                var col = SkiaChartRenderer.ResolveSeriesColorStatic(model, item.SeriesReference, palette);
                bool visible = true;
                var seriesRef = item.SeriesReference;
                var s = model.Series.FirstOrDefault(ss => object.ReferenceEquals(ss, seriesRef));
                if (s != null) visible = s.IsVisible;

                var sr = new SkiaSharp.SKRect(bx + padding, y - paints.Text.TextSize + 2, bx + padding + swatch, y - paints.Text.TextSize + 2 + swatch);
                if (visible)
                {
                    swPaint.Color = new SkiaSharp.SKColor(col.R, col.G, col.B, col.A);
                    c.DrawRect(sr, swPaint);
                    c.DrawText(item.Title ?? string.Empty, sr.Right + gap, y, paints.Text);
                }
                else
                {
                    swMuted.Color = new SkiaSharp.SKColor(col.R, col.G, col.B, (byte)(col.A * 0.3));
                    c.DrawRect(sr, swMuted);
                    using var outline = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Stroke, Color = new SkiaSharp.SKColor(120,120,120,160), StrokeWidth = 1, IsAntialias = true };
                    c.DrawRect(sr, outline);
                    c.DrawText(item.Title ?? string.Empty, sr.Right + gap, y, textMuted);
                }

                hits.Add(new FastCharts.Core.Interaction.LegendHit { X = bx, Y = y - paints.Text.TextSize, Width = boxW, Height = lineH, SeriesReference = item.SeriesReference });
                y += lineH;
            }
        }
    }
}
