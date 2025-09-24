using FastCharts.Core;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaBorderRenderer
    {
        public static void Render(ChartModel model, SKCanvas canvas, SKRect plotRect)
        {
            var theme = model.Theme;
            var axisColor = new SKColor(theme.AxisColor.R, theme.AxisColor.G, theme.AxisColor.B, theme.AxisColor.A);
            using var border = new SKPaint { Color = axisColor, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
            canvas.DrawRect(plotRect, border);
        }
    }
}
