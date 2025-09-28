using FastCharts.Core.Series;

using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class ErrorBarLayer : ISeriesSubLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model; var pr = ctx.PlotRect; var palette = model.Theme.SeriesPalette; int paletteCount = palette?.Count ?? 0;
            int errIndex = 0;
            foreach (var s in model.Series)
            {
                if (s is not ErrorBarSeries es || es.IsEmpty || !es.IsVisible) 
                {
                    continue;
                }
                var c = (paletteCount > 0 && errIndex < paletteCount && palette != null) ? palette[errIndex] : model.Theme.PrimarySeriesColor;
                using var pen = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)System.Math.Max(1.0, es.StrokeThickness), Color = new SKColor(c.R, c.G, c.B, c.A) };
                double cap = es.GetCapWidth() * 0.5;
                ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr);
                foreach (var p in es.Data)
                {
                    double neg = p.NegativeError ?? p.PositiveError;
                    float xC = PixelMapper.X(p.X, model.XAxis, pr);
                    var yAxis = (es.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;
                    float yTop = PixelMapper.Y(p.Y + p.PositiveError, yAxis, pr);
                    float yBot = PixelMapper.Y(p.Y - neg, yAxis, pr);
                    float xL = PixelMapper.X(p.X - cap, model.XAxis, pr);
                    float xR = PixelMapper.X(p.X + cap, model.XAxis, pr);
                    ctx.Canvas.DrawLine(xC, yTop, xC, yBot, pen);
                    ctx.Canvas.DrawLine(xL, yTop, xR, yTop, pen);
                    ctx.Canvas.DrawLine(xL, yBot, xR, yBot, pen);
                }
                ctx.Canvas.Restore();
                errIndex++;
            }
        }
    }
}
