using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers;

internal sealed class OhlcLayer : ISeriesSubLayer
{
    public void Render(RenderContext ctx)
    {
        var model = ctx.Model;
        var pr = ctx.PlotRect;
        var palette = model.Theme.SeriesPalette;
        var paletteCount = palette?.Count ?? 0;
        foreach (var s in model.Series)
        {
            if (s is not OhlcSeries os || os.IsEmpty || !os.IsVisible)
            {
                continue;
            }
            var cUp = model.Theme.PrimarySeriesColor;
            var cDown = (paletteCount > 1 && palette != null)
                ? palette[1]
                : new FastCharts.Core.Primitives.ColorRgba((byte)(cUp.R * 0.7), (byte)(cUp.G * 0.3), (byte)(cUp.B * 0.3), cUp.A);
            var wickStroke = (float)System.Math.Max(1.0, os.WickThickness);
            ctx.Canvas.Save();
            ctx.Canvas.ClipRect(pr);
            for (var i = 0; i < os.Data.Count; i++)
            {
                var p = os.Data[i];
                var w = os.GetWidthFor(i);
                var half = w * 0.5;
                var xC = PixelMapper.X(p.X, model.XAxis, pr);
                var xL = PixelMapper.X(p.X - half * 0.7, model.XAxis, pr);
                var xR = PixelMapper.X(p.X + half * 0.7, model.XAxis, pr);
                var yAxis = (os.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;
                var yOpen = PixelMapper.Y(p.Open, yAxis, pr);
                var yHigh = PixelMapper.Y(p.High, yAxis, pr);
                var yLow = PixelMapper.Y(p.Low, yAxis, pr);
                var yClose = PixelMapper.Y(p.Close, yAxis, pr);
                var up = p.Close >= p.Open;
                var bodyColor = up ? cUp : cDown;
                var fillAlpha = (byte)(RenderMath.Clamp01(up ? os.UpFillOpacity : os.DownFillOpacity) * bodyColor.A);
                using var wick = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = wickStroke, Color = new SKColor(bodyColor.R, bodyColor.G, bodyColor.B, bodyColor.A) };
                using var bodyFill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(bodyColor.R, bodyColor.G, bodyColor.B, fillAlpha) };
                using var bodyStroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(bodyColor.R, bodyColor.G, bodyColor.B, bodyColor.A) };
                ctx.Canvas.DrawLine(xC, yHigh, xC, yLow, wick);
                var top = System.Math.Min(yOpen, yClose);
                var bot = System.Math.Max(yOpen, yClose);
                var rect = SKRect.Create(xL, top, xR - xL, bot - top);
                if (os.Filled)
                {
                    ctx.Canvas.DrawRect(rect, bodyFill);
                }
                ctx.Canvas.DrawRect(rect, bodyStroke);
            }
            ctx.Canvas.Restore();
        }
    }
}
