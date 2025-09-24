using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaAreaSeriesRenderer
    {
        public static void Render(ChartModel model, SKCanvas canvas, SKRect plotRect)
        {
            var palette = model.Theme.SeriesPalette;
            int seriesIndex = 0;
            foreach (var ls in model.Series.OfType<LineSeries>())
            {
                if (ls.IsEmpty)
                {
                    seriesIndex++;
                    continue;
                }

                var area = ls as AreaSeries;
                if (area == null)
                {
                    seriesIndex++;
                    continue;
                }

                int index = ls.PaletteIndex ?? seriesIndex;
                var c = (palette != null && index < palette.Count) ? palette[index] : model.Theme.PrimarySeriesColor;

                using var fillPath = new SKPath();
                bool started = false;
                double baseline = area.Baseline;
                double firstX = 0.0;
                bool hasFirstX = false;
                foreach (var p in area.Data)
                {
                    float px = PixelMapper.X(p.X, model.XAxis, plotRect);
                    float pyBase = PixelMapper.Y(baseline, model.YAxis, plotRect);
                    if (!started)
                    {
                        fillPath.MoveTo(px, pyBase);
                        started = true;
                        firstX = p.X;
                        hasFirstX = true;
                    }
                    float py = PixelMapper.Y(p.Y, model.YAxis, plotRect);
                    fillPath.LineTo(px, py);
                }
                if (started && hasFirstX)
                {
                    var last = area.Data.Last();
                    float lastPx = PixelMapper.X(last.X, model.XAxis, plotRect);
                    float basePy = PixelMapper.Y(baseline, model.YAxis, plotRect);
                    fillPath.LineTo(lastPx, basePy);
                    fillPath.Close();

                    byte alpha = (byte)(System.Math.Max(0.0, System.Math.Min(1.0, area.FillOpacity)) * c.A);
                    var fillColor = new SKColor(c.R, c.G, c.B, alpha);
                    using var fillPaint = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill,
                        Color = fillColor
                    };
                    canvas.DrawPath(fillPath, fillPaint);
                }

                seriesIndex++;
            }
        }
    }
}
