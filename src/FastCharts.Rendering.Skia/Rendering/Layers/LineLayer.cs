using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers;

internal sealed class LineLayer : ISeriesSubLayer
{
    public void Render(RenderContext ctx)
    {
        var model = ctx.Model;
        var pr = ctx.PlotRect;
        var palette = model.Theme.SeriesPalette;
        var paletteCount = palette?.Count ?? 0;
        var lineIndex = 0;
        
        foreach (var s in model.Series)
        {
            if (s is not LineSeries ls)
            {
                continue;
            }
            if (ls is AreaSeries or StepLineSeries)
            {
                continue; // rendered in dedicated layers
            }
            if (ls.IsEmpty || !ls.IsVisible)
            {
                continue;
            }
            
            var c = (paletteCount > 0 && lineIndex < paletteCount && palette != null) ? palette[lineIndex] : model.Theme.PrimarySeriesColor;
            using var sp = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)ls.StrokeThickness, Color = new SKColor(c.R, c.G, c.B, c.A) };
            using var path = new SKPath();
            
            // ?? LTTB INTEGRATION: Use GetRenderData() for automatic resampling!
            // This provides optimized data based on viewport pixel width
            var plotPixelWidth = (int)pr.Width;
            var renderData = ls.GetRenderData(plotPixelWidth);
            
            var started = false;
            foreach (var p in renderData)  // ? Changed from ls.Data to renderData
            {
                var px = PixelMapper.X(p.X, model.XAxis, pr);
                var yAxis = (ls.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;
                var py = PixelMapper.Y(p.Y, yAxis, pr);
                if (!started)
                {
                    path.MoveTo(px, py);
                    started = true;
                }
                else
                {
                    path.LineTo(px, py);
                }
            }
            
            ctx.Canvas.Save();
            ctx.Canvas.ClipRect(pr);
            ctx.Canvas.DrawPath(path, sp);
            ctx.Canvas.Restore();
            lineIndex++;
        }
    }
}
