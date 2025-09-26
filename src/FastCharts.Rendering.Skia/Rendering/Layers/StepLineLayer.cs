using SkiaSharp;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class StepLineLayer : ISeriesSubLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model; var pr = ctx.PlotRect; var palette = model.Theme.SeriesPalette; int paletteCount = palette?.Count ?? 0;
            int stepIndex = 0;
            foreach (var s in model.Series)
            {
                if (s is not StepLineSeries sls || sls.IsEmpty || !sls.IsVisible)
                {
                    continue;
                }
                var c = (paletteCount > 0 && stepIndex < paletteCount && palette != null) ? palette[stepIndex] : model.Theme.PrimarySeriesColor;
                using var sp = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)sls.StrokeThickness, Color = new SKColor(c.R, c.G, c.B, c.A) };
                using var path = new SKPath(); bool started2 = false;
                for (int i = 0; i < sls.Data.Count; i++)
                {
                    var p = sls.Data[i];
                    float x = PixelMapper.X(p.X, model.XAxis, pr);
                    float y = PixelMapper.Y(p.Y, model.YAxis, pr);
                    if (!started2)
                    {
                        path.MoveTo(x, y);
                        started2 = true;
                        continue;
                    }
                    var prev = sls.Data[i - 1];
                    float xPrev = PixelMapper.X(prev.X, model.XAxis, pr);
                    float yPrev = PixelMapper.Y(prev.Y, model.YAxis, pr);
                    if (sls.Mode == StepMode.Before)
                    {
                        path.LineTo(x, yPrev); path.LineTo(x, y);
                    }
                    else
                    {
                        path.LineTo(xPrev, y); path.LineTo(x, y);
                    }
                }
                ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr); ctx.Canvas.DrawPath(path, sp); ctx.Canvas.Restore();
                stepIndex++;
            }
        }
    }
}
