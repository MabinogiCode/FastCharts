using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia;

public sealed class SkiaChartRenderer
{
    /// <summary>
    /// Render the model into the provided SKCanvas (pixel coordinates).
    /// </summary>
    public void Render(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight)
    {
        // Safety
        if (canvas is null) return;

        // (Re)compute scales for exact pixel size to avoid DPI drift
        model?.UpdateScales(pixelWidth, pixelHeight);

        // Clear to transparent (Border background will show through)
        canvas.Clear(SKColors.Transparent);

        if (model is null) return;

        // Draw first LineSeries only (minimal step; pipeline will expand later)
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
