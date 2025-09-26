using SkiaSharp;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class AreaBandLayer : ISeriesSubLayer
    {
        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var palette = model.Theme.SeriesPalette;
            int paletteCount = palette?.Count ?? 0;

            int areaIndex = 0;
            // First pass: area series
            foreach (var s in model.Series)
            {
                if (s is not AreaSeries area || area.IsEmpty || !area.IsVisible) continue;
                int idx = area.PaletteIndex ?? areaIndex;
                var c = (paletteCount > 0 && idx < paletteCount) ? palette[idx] : model.Theme.PrimarySeriesColor;
                using var path = new SKPath();
                bool started = false;
                foreach (var p in area.Data)
                {
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    float py = PixelMapper.Y(p.Y, model.YAxis, pr);
                    if (!started) { path.MoveTo(px, py); started = true; }
                    else { path.LineTo(px, py); }
                }
                if (started)
                {
                    int lastIdx = area.Data.Count - 1;
                    float lastX = lastIdx >= 0 ? PixelMapper.X(area.Data[lastIdx].X, model.XAxis, pr) : 0f;
                    float baseY = PixelMapper.Y(area.Baseline, model.YAxis, pr);
                    path.LineTo(lastX, baseY);
                    path.Close();
                    byte alpha = (byte)(Clamp01(area.FillOpacity) * c.A);
                    using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(c.R, c.G, c.B, alpha) };
                    ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr); ctx.Canvas.DrawPath(path, fill); ctx.Canvas.Restore();
                }
                areaIndex++;
            }

            // Band series
            int bandIndex = 0;
            foreach (var s in model.Series)
            {
                if (s is not BandSeries bs || bs.IsEmpty || !bs.IsVisible) continue;
                var c = (paletteCount > 0 && bandIndex < paletteCount) ? palette[bandIndex] : model.Theme.PrimarySeriesColor;
                byte alpha = (byte)(Clamp01(bs.FillOpacity) * c.A);
                using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(c.R, c.G, c.B, alpha) };
                using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)bs.StrokeThickness, Color = new SKColor(c.R, c.G, c.B, c.A) };
                using var bandPath = new SKPath();
                bool started = false;
                for (int i = 0; i < bs.Data.Count; i++)
                {
                    var p = bs.Data[i];
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    float pyHigh = PixelMapper.Y(p.YHigh, model.YAxis, pr);
                    if (!started) { bandPath.MoveTo(px, pyHigh); started = true; }
                    else { bandPath.LineTo(px, pyHigh); }
                }
                for (int i = bs.Data.Count - 1; i >= 0; i--)
                {
                    var p = bs.Data[i];
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    float pyLow = PixelMapper.Y(p.YLow, model.YAxis, pr);
                    bandPath.LineTo(px, pyLow);
                }
                if (started)
                {
                    bandPath.Close();
                    ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr); ctx.Canvas.DrawPath(bandPath, fillPaint); ctx.Canvas.Restore();
                }
                bandIndex++;
            }
        }
    }
}
