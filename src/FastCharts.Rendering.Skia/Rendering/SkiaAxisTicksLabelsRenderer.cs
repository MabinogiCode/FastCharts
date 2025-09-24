using System.Collections.Generic;
using FastCharts.Core;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaAxisTicksLabelsRenderer
    {
        public static void Render(ChartModel model, SKCanvas canvas, SKRect plotRect, IEnumerable<double> xTicks, IEnumerable<double> yTicks)
        {
            var theme = model.Theme;
            var axisColor = new SKColor(theme.AxisColor.R, theme.AxisColor.G, theme.AxisColor.B, theme.AxisColor.A);
            var labelColor = new SKColor(theme.LabelColor.R, theme.LabelColor.G, theme.LabelColor.B, theme.LabelColor.A);

            using var tickPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = axisColor, StrokeWidth = (float)theme.AxisThickness };
            using var textPaint = new SKPaint { IsAntialias = true, Color = labelColor, TextSize = (float)theme.LabelTextSize };

            float xBase = plotRect.Left;
            float yBase = plotRect.Bottom;
            float tickLen = (float)theme.TickLength;

            foreach (var t in xTicks)
            {
                float px = PixelMapper.X(t, model.XAxis, plotRect);
                canvas.DrawLine(px, yBase, px, yBase + tickLen, tickPaint);
                var xf = model.XAxis.NumberFormatter;
                var xLabel = xf != null ? xf.Format(t) : t.ToString(model.XAxis.LabelFormat ?? "G");
                float xWidth = textPaint.MeasureText(xLabel);
                canvas.DrawText(xLabel, px - xWidth / 2f, yBase + tickLen + 3 + textPaint.TextSize, textPaint);
            }

            foreach (var t in yTicks)
            {
                float py = PixelMapper.Y(t, model.YAxis, plotRect);
                canvas.DrawLine(xBase - tickLen, py, xBase, py, tickPaint);
                var yf = model.YAxis.NumberFormatter;
                var yLabel = yf != null ? yf.Format(t) : t.ToString(model.YAxis.LabelFormat ?? "G");
                float yWidth = textPaint.MeasureText(yLabel);
                canvas.DrawText(yLabel, xBase - tickLen - 6 - yWidth, py + 4, textPaint);
            }
        }
    }
}
