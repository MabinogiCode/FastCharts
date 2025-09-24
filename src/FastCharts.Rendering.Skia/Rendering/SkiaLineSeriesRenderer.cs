using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaLineSeriesRenderer
    {
        public static void Render(ChartModel model, SKCanvas canvas, SKRect plotRect)
        {
            var palette = model.Theme.SeriesPalette;
            int seriesIndex = 0;
            foreach (var ls in model.Series.OfType<LineSeries>())
            {
                if (ls is AreaSeries || ls is ScatterSeries)
                {
                    seriesIndex++;
                    continue;
                }
                if (ls.IsEmpty)
                {
                    seriesIndex++;
                    continue;
                }
                var c = (palette != null && seriesIndex < palette.Count) ? palette[seriesIndex] : model.Theme.PrimarySeriesColor;
                using var seriesPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = (float)ls.StrokeThickness,
                    Color = new SKColor(c.R, c.G, c.B, c.A)
                };
                using var path = new SKPath();
                bool started = false;
                foreach (var p in ls.Data)
                {
                    float px = PixelMapper.X(p.X, model.XAxis, plotRect);
                    float py = PixelMapper.Y(p.Y, model.YAxis, plotRect);
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
                canvas.DrawPath(path, seriesPaint);
                seriesIndex++;
            }
        }
    }
}
