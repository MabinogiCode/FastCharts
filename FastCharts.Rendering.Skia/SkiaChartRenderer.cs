using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Series;
using FastCharts.Core.Abstractions; // NEW
using SkiaSharp;

namespace FastCharts.Rendering.Skia
{
    public sealed class SkiaChartRenderer : IRenderer<SKCanvas> // implements Core contract
    {
        public void Render(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight)
        {
            if (canvas is null || model is null) return;

            // Keep scales exact for the pixel surface
            model.UpdateScales(pixelWidth, pixelHeight);

            canvas.Clear(SKColors.Transparent);

            var ls = model.Series.OfType<LineSeries>().FirstOrDefault();
            if (ls is null || ls.IsEmpty) return;

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (float)ls.StrokeThickness,
                Color = new SKColor(0x33, 0x99, 0xFF)
            };

            using var path = new SKPath();
            bool started = false;

            foreach (var p in ls.Data)
            {
                var x = (float)model.XAxis.Scale.ToPixels(p.X);
                var y = (float)model.YAxis.Scale.ToPixels(p.Y);

                if (!started) { path.MoveTo(x, y); started = true; }
                else { path.LineTo(x, y); }
            }

            canvas.DrawPath(path, paint);
        }
    }
}
