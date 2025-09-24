using FastCharts.Core;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaBackgroundRenderer
    {
        public static void RenderSurfaceBackground(ChartModel model, SKCanvas canvas)
        {
            var c = model.Theme.SurfaceBackgroundColor;
            canvas.Clear(new SKColor(c.R, c.G, c.B, c.A));
        }

        public static void RenderPlotBackground(ChartModel model, SKCanvas canvas, SKRect plotRect)
        {
            var c = model.Theme.PlotBackgroundColor;
            using var bg = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(c.R, c.G, c.B, c.A)
            };
            canvas.DrawRect(plotRect, bg);
        }
    }
}
