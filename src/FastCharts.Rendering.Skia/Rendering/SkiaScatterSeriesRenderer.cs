using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaScatterSeriesRenderer
    {
        public static void Render(ChartModel model, SKCanvas canvas, SKRect plotRect)
        {
            var palette = model.Theme.SeriesPalette;
            int seriesIndex = 0;
            foreach (var ss in model.Series.OfType<ScatterSeries>())
            {
                if (ss.IsEmpty)
                {
                    seriesIndex++;
                    continue;
                }

                var c = (palette != null && seriesIndex < palette.Count) ? palette[seriesIndex] : model.Theme.PrimarySeriesColor;
                using var markerPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(c.R, c.G, c.B, c.A) };
                float size = (float)ss.MarkerSize;
                if (size < 1f)
                {
                    size = 1f;
                }
                float half = size * 0.5f;
                foreach (var p in ss.Data)
                {
                    float px = PixelMapper.X(p.X, model.XAxis, plotRect);
                    float py = PixelMapper.Y(p.Y, model.YAxis, plotRect);
                    if (ss.MarkerShape == MarkerShape.Circle)
                    {
                        canvas.DrawCircle(px, py, half, markerPaint);
                    }
                    else if (ss.MarkerShape == MarkerShape.Square)
                    {
                        canvas.DrawRect(new SKRect(px - half, py - half, px + half, py + half), markerPaint);
                    }
                    else
                    {
                        using var path = new SKPath();
                        path.MoveTo(px, py - half);
                        path.LineTo(px - half, py + half);
                        path.LineTo(px + half, py + half);
                        path.Close();
                        canvas.DrawPath(path, markerPaint);
                    }
                }
                seriesIndex++;
            }
        }
    }
}
