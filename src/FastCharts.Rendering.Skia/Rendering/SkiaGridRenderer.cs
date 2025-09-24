using FastCharts.Core;
using SkiaSharp;
using System.Collections.Generic;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaGridRenderer
    {
        public static void Render(ChartModel model, SKCanvas canvas, SKRect plotRect, IEnumerable<double> xTicks, IEnumerable<double> yTicks)
        {
            var theme = model.Theme;
            var gridColor = new SKColor(theme.GridColor.R, theme.GridColor.G, theme.GridColor.B, theme.GridColor.A);
            using var gridPaint = new SKPaint
            {
                IsAntialias = false,
                Style = SKPaintStyle.Stroke,
                Color = gridColor,
                StrokeWidth = (float)theme.GridThickness
            };

            foreach (var t in xTicks)
            {
                float px = PixelMapper.X(t, model.XAxis, plotRect);
                canvas.DrawLine(px, plotRect.Top, px, plotRect.Bottom, gridPaint);
            }

            foreach (var t in yTicks)
            {
                float py = PixelMapper.Y(t, model.YAxis, plotRect);
                canvas.DrawLine(plotRect.Left, py, plotRect.Right, py, gridPaint);
            }
        }
    }
}
