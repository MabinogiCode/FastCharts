using SkiaSharp;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class AreaBandLayer : ISeriesSubLayer
    {
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
                if (s is not AreaSeries area || area.IsEmpty || !area.IsVisible)
                {
                    continue;
                }
                int idx = area.PaletteIndex ?? areaIndex;
                var chosen = (paletteCount > 0 && idx < paletteCount && palette != null) ? palette[idx] : model.Theme.PrimarySeriesColor;
                using var path = new SKPath();
                bool started = false;
                var yAxis = (area.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;
                foreach (var p in area.Data)
                {
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    float py = PixelMapper.Y(p.Y, yAxis, pr);
                    if (!started)
                    {
                        path.MoveTo(px, py);
                        started = true;
                    }
                    else
                    {
                        path.LineTo(px, py);
                    }
                }
                if (started)
                {
                    int lastIdx = area.Data.Count - 1;
                    float lastX = lastIdx >= 0 ? PixelMapper.X(area.Data[lastIdx].X, model.XAxis, pr) : 0f;
                    float baseY = PixelMapper.Y(area.Baseline, yAxis, pr);
                    path.LineTo(lastX, baseY);
                    path.Close();
                    byte alpha = (byte)(RenderMath.Clamp01(area.FillOpacity) * chosen.A);
                    using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(chosen.R, chosen.G, chosen.B, alpha) };
                    ctx.Canvas.Save();
                    ctx.Canvas.ClipRect(pr);
                    ctx.Canvas.DrawPath(path, fill);
                    ctx.Canvas.Restore();
                }
                areaIndex++;
            }
            // Band series
            int bandIndex = 0;
            foreach (var s in model.Series)
            {
                if (s is not BandSeries bs || bs.IsEmpty || !bs.IsVisible)
                {
                    continue;
                }
                var col = (paletteCount > 0 && bandIndex < paletteCount && palette != null) ? palette[bandIndex] : model.Theme.PrimarySeriesColor;
                byte alpha2 = (byte)(RenderMath.Clamp01(bs.FillOpacity) * col.A);
                using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(col.R, col.G, col.B, alpha2) };
                using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)bs.StrokeThickness, Color = new SKColor(col.R, col.G, col.B, col.A) };
                using var pathHigh = new SKPath(); using var pathLow = new SKPath(); bool startedB = false;
                var yAxis = (bs.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;
                for (int i = 0; i < bs.Data.Count; i++)
                {
                    var p = bs.Data[i];
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    float pyHigh = PixelMapper.Y(p.YHigh, yAxis, pr);
                    if (!startedB) { pathHigh.MoveTo(px, pyHigh); startedB = true; }
                    else { pathHigh.LineTo(px, pyHigh); }
                }
                for (int i = bs.Data.Count - 1; i >= 0; i--)
                {
                    var p = bs.Data[i];
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    float pyLow = PixelMapper.Y(p.YLow, yAxis, pr);
                    pathHigh.LineTo(px, pyLow);
                }
                if (startedB)
                {
                    pathHigh.Close();
                    ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr); ctx.Canvas.DrawPath(pathHigh, fillPaint); ctx.Canvas.Restore();
                }
                bandIndex++;
            }
        }
    }
}
