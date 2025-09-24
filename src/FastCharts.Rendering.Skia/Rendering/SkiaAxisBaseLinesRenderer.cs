using FastCharts.Core;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaAxisBaseLinesRenderer
    {
        public static void Render(ChartModel model, SKCanvas canvas, SKRect plotRect)
        {
            var theme = model.Theme;
            var axisColor = new SKColor(theme.AxisColor.R, theme.AxisColor.G, theme.AxisColor.B, theme.AxisColor.A);
            using var axisPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = axisColor,
                StrokeWidth = (float)theme.AxisThickness
            };

            float xBase = plotRect.Left;
            float yBase = plotRect.Bottom;
            float xAxisStart = plotRect.Left;
            float xAxisEnd = plotRect.Right;
            float yAxisStart = plotRect.Top;
            float yAxisEnd = plotRect.Bottom;

            canvas.DrawLine(xAxisStart, yBase, xAxisEnd, yBase, axisPaint); // X axis
            canvas.DrawLine(xBase, yAxisStart, xBase, yAxisEnd, axisPaint); // Y axis
        }
    }
}
