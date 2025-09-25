using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class LegendLayer : IRenderLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model;
            if (model.Legend.Items.Count == 0) return;
            var theme = model.Theme;
            var palette = theme.SeriesPalette;
            var paints = ctx.Paints;
            var c = ctx.Canvas;

            var labelColor = new SKColor(theme.LabelColor.R, theme.LabelColor.G, theme.LabelColor.B, theme.LabelColor.A);
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
            var rect = new SKRect(bx, by, bx + boxW, by + boxH);

            SKColor bg = lum > 0.55 ? new SKColor(20,20,20,210) : new SKColor(255,255,255,210);
            SKColor bd = lum > 0.55 ? new SKColor(90,90,90,230) : new SKColor(0,0,0,140);
            using var bgPaint = new SKPaint { Style = SKPaintStyle.Fill, Color = bg, IsAntialias = true };
            using var bdPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = bd, StrokeWidth = 1, IsAntialias = true };
            using var swPaint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };

            c.DrawRect(rect, bgPaint);
            c.DrawRect(rect, bdPaint);
            float y = by + padding + paints.Text.TextSize;
            foreach (var item in model.Legend.Items)
            {
                var col = SkiaChartRenderer.ResolveSeriesColorStatic(model, item.SeriesReference, palette);
                swPaint.Color = new SKColor(col.R, col.G, col.B, col.A);
                var sr = new SKRect(bx + padding, y - paints.Text.TextSize + 2, bx + padding + swatch, y - paints.Text.TextSize + 2 + swatch);
                c.DrawRect(sr, swPaint);
                c.DrawText(item.Title ?? string.Empty, sr.Right + gap, y, paints.Text);
                y += lineH;
            }
        }
    }
}
