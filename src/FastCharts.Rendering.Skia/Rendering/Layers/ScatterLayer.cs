using SkiaSharp;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class ScatterLayer : ISeriesSubLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model; var pr = ctx.PlotRect; var palette = model.Theme.SeriesPalette; int paletteCount = palette?.Count ?? 0;
            int scatterIndex = 0;
            foreach (var s in model.Series)
            {
                if (s is not ScatterSeries ss || ss.IsEmpty || !ss.IsVisible) continue;
                var c = (paletteCount > 0 && scatterIndex < paletteCount && palette != null) ? palette[scatterIndex] : model.Theme.PrimarySeriesColor;
                float size = (float)ss.MarkerSize; if (size < 1f) size = 1f; float half = size * 0.5f;
                using var mp = new SKPaint { IsAntialias = size <= 3 ? false : true, Style = SKPaintStyle.Fill, Color = new SKColor(c.R, c.G, c.B, c.A) };
                ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr);
                foreach (var p in ss.Data)
                {
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    var yAxis = (ss.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;
                    float py = PixelMapper.Y(p.Y, yAxis, pr);
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
        }
    }
}
