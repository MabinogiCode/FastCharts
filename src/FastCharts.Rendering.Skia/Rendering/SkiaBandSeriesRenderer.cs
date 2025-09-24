using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaBandSeriesRenderer
    {
        public static void Render(ChartModel model, SKCanvas canvas, SKRect plotRect)
        {
            var palette = model.Theme.SeriesPalette;
            int seriesIndex = 0;
            foreach (var bs in model.Series.OfType<BandSeries>())
            {
                if (bs.IsEmpty)
                {
                    seriesIndex++;
                    continue;
                }

                var c = (palette != null && seriesIndex < palette.Count) ? palette[seriesIndex] : model.Theme.PrimarySeriesColor;
                byte alpha = (byte)(System.Math.Max(0.0, System.Math.Min(1.0, bs.FillOpacity)) * c.A);
                var fillColor = new SKColor(c.R, c.G, c.B, alpha);

                using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = fillColor };
                using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)bs.StrokeThickness, Color = new SKColor(c.R, c.G, c.B, c.A) };
                using var bandPath = new SKPath();
                using var upperPath = new SKPath();
                using var lowerPath = new SKPath();
                bool started = false;

                for (int i = 0; i < bs.Data.Count; i++)
                {
                    var p = bs.Data[i];
                    float px = PixelMapper.X(p.X, model.XAxis, plotRect);
                    float pyHigh = PixelMapper.Y(p.YHigh, model.YAxis, plotRect);
                    float pyLow = PixelMapper.Y(p.YLow, model.YAxis, plotRect);
                    if (!started)
                    {
                        bandPath.MoveTo(px, pyHigh);
                        upperPath.MoveTo(px, pyHigh);
                        lowerPath.MoveTo(px, pyLow);
                        started = true;
                    }
                    else
                    {
                        bandPath.LineTo(px, pyHigh);
                        upperPath.LineTo(px, pyHigh);
                        lowerPath.LineTo(px, pyLow);
                    }
                }
                for (int i = bs.Data.Count - 1; i >= 0; i--)
                {
                    var p = bs.Data[i];
                    float px = PixelMapper.X(p.X, model.XAxis, plotRect);
                    float pyLow = PixelMapper.Y(p.YLow, model.YAxis, plotRect);
                    bandPath.LineTo(px, pyLow);
                }
                if (started)
                {
                    bandPath.Close();
                    canvas.DrawPath(bandPath, fillPaint);
                    if (bs.StrokeThickness > 0.0)
                    {
                        canvas.DrawPath(upperPath, strokePaint);
                        canvas.DrawPath(lowerPath, strokePaint);
                    }
                }
                seriesIndex++;
            }
        }
    }
}
