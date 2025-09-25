using System.Linq;
using SkiaSharp;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class SeriesLayer : IRenderLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var palette = model.Theme.SeriesPalette;

            // AREA
            int areaIndex = 0;
            foreach (var ls in model.Series.OfType<LineSeries>())
            {
                if (ls is not AreaSeries area || area.IsEmpty) { areaIndex++; continue; }
                int idx = area.PaletteIndex ?? areaIndex;
                var c = (palette != null && idx < palette.Count) ? palette[idx] : model.Theme.PrimarySeriesColor;
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
                    byte alpha = (byte)(System.Math.Max(0, System.Math.Min(1, area.FillOpacity)) * c.A);
                    using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(c.R, c.G, c.B, alpha) };
                    ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr); ctx.Canvas.DrawPath(path, fill); ctx.Canvas.Restore();
                }
                areaIndex++;
            }

            // BAND
            int bandIndex = 0;
            foreach (var bs in model.Series.OfType<BandSeries>())
            {
                if (bs.IsEmpty) { bandIndex++; continue; }
                var c = (palette != null && bandIndex < palette.Count) ? palette[bandIndex] : model.Theme.PrimarySeriesColor;
                byte alpha = (byte)(System.Math.Max(0, System.Math.Min(1, bs.FillOpacity)) * c.A);
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

            // SCATTER
            int scatterIndex = 0;
            foreach (var ss in model.Series.OfType<ScatterSeries>())
            {
                if (ss.IsEmpty || !ss.IsVisible) { scatterIndex++; continue; }
                var c = (palette != null && scatterIndex < palette.Count) ? palette[scatterIndex] : model.Theme.PrimarySeriesColor;
                using var mp = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(c.R, c.G, c.B, c.A) };
                float size = (float)ss.MarkerSize; if (size < 1f) size = 1f; float half = size * 0.5f;
                ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr);
                foreach (var p in ss.Data)
                {
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    float py = PixelMapper.Y(p.Y, model.YAxis, pr);
                    switch (ss.MarkerShape)
                    {
                        case MarkerShape.Circle:
                            ctx.Canvas.DrawCircle(px, py, half, mp); break;
                        case MarkerShape.Square:
                            ctx.Canvas.DrawRect(new SKRect(px - half, py - half, px + half, py + half), mp); break;
                        default:
                            using (var path = new SKPath())
                            {
                                path.MoveTo(px, py - half);
                                path.LineTo(px - half, py + half);
                                path.LineTo(px + half, py + half);
                                path.Close();
                                ctx.Canvas.DrawPath(path, mp);
                            }
                            break;
                    }
                }
                ctx.Canvas.Restore();
                scatterIndex++;
            }

            // STEP-LINE (render before plain lines to respect palette indices akin to LineSeries order)
            int stepIndex = 0;
            foreach (var s in model.Series.OfType<StepLineSeries>())
            {
                if (s.IsEmpty || !s.IsVisible) { stepIndex++; continue; }
                var c = (palette != null && stepIndex < palette.Count) ? palette[stepIndex] : model.Theme.PrimarySeriesColor;
                using var sp = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)s.StrokeThickness, Color = new SKColor(c.R, c.G, c.B, c.A) };
                using var path = new SKPath(); bool started = false;
                for (int i = 0; i < s.Data.Count; i++)
                {
                    var p = s.Data[i];
                    float x = PixelMapper.X(p.X, model.XAxis, pr);
                    float y = PixelMapper.Y(p.Y, model.YAxis, pr);
                    if (!started)
                    {
                        path.MoveTo(x, y); started = true; continue;
                    }
                    var prev = s.Data[i - 1];
                    float xPrev = PixelMapper.X(prev.X, model.XAxis, pr);
                    float yPrev = PixelMapper.Y(prev.Y, model.YAxis, pr);
                    if (s.Mode == StepMode.Before)
                    {
                        // horizontal from prev to p.X, then vertical to p.Y
                        path.LineTo(x, yPrev);
                        path.LineTo(x, y);
                    }
                    else
                    {
                        // vertical from prev to p.Y, then horizontal to p.X
                        path.LineTo(xPrev, y);
                        path.LineTo(x, y);
                    }
                }
                ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr); ctx.Canvas.DrawPath(path, sp); ctx.Canvas.Restore();
                stepIndex++;
            }

            // LINES (plain)
            int lineIndex = 0;
            foreach (var ls in model.Series.OfType<LineSeries>())
            {
                if (ls is AreaSeries || ls is ScatterSeries || ls is StepLineSeries) { lineIndex++; continue; }
                if (ls.IsEmpty || !ls.IsVisible) { lineIndex++; continue; }
                var c = (palette != null && lineIndex < palette.Count) ? palette[lineIndex] : model.Theme.PrimarySeriesColor;
                using var sp = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)ls.StrokeThickness, Color = new SKColor(c.R, c.G, c.B, c.A) };
                using var path = new SKPath(); bool startedLine = false;
                foreach (var p in ls.Data)
                {
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    float py = PixelMapper.Y(p.Y, model.YAxis, pr);
                    if (!startedLine) { path.MoveTo(px, py); startedLine = true; }
                    else { path.LineTo(px, py); }
                }
                ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr); ctx.Canvas.DrawPath(path, sp); ctx.Canvas.Restore();
                lineIndex++;
            }
        }
    }
}
