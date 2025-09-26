using SkiaSharp;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class LineLayer : ISeriesSubLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var palette = model.Theme.SeriesPalette;
            int paletteCount = palette?.Count ?? 0;
            int lineIndex = 0;
            foreach (var s in model.Series)
            {
                if (s is not LineSeries ls)
                {
                    continue;
                }
                // Skip derived or other series types that are rendered in their specific layers
                if (ls is AreaSeries || ls is StepLineSeries)
                {
                    continue;
                }
                if (ls.IsEmpty || !ls.IsVisible)
                {
                    continue;
                }
                var c = (paletteCount > 0 && lineIndex < paletteCount && palette != null) ? palette[lineIndex] : model.Theme.PrimarySeriesColor;
                using var sp = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)ls.StrokeThickness, Color = new SKColor(c.R, c.G, c.B, c.A) };
                using var path2 = new SKPath();
                bool startedLine = false;
                foreach (var p in ls.Data)
                {
                    float px = PixelMapper.X(p.X, model.XAxis, pr);
                    var yAxis = (ls.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;
                    float py = PixelMapper.Y(p.Y, yAxis, pr);
                    if (!startedLine)
                    {
                        path2.MoveTo(px, py);
                        startedLine = true;
                    }
                    else
                    {
                        path2.LineTo(px, py);
                    }
                }
                ctx.Canvas.Save();
                ctx.Canvas.ClipRect(pr);
                ctx.Canvas.DrawPath(path2, sp);
                ctx.Canvas.Restore();
                lineIndex++;
            }
        }
    }
}
