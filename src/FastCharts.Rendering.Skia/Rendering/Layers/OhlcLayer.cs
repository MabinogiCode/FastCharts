using SkiaSharp;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class OhlcLayer : ISeriesSubLayer
    {
        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model; var pr = ctx.PlotRect; var palette = model.Theme.SeriesPalette; int paletteCount = palette?.Count ?? 0;
            foreach (var s in model.Series)
            {
                if (s is not OhlcSeries os || os.IsEmpty || !os.IsVisible) continue;
                var cUp = model.Theme.PrimarySeriesColor;
                var cDown = (paletteCount > 1) ? palette[1] : new FastCharts.Core.Primitives.ColorRgba((byte)(cUp.R * 0.7), (byte)(cUp.G * 0.3), (byte)(cUp.B * 0.3), cUp.A);
                float wickStroke = (float)System.Math.Max(1.0, os.WickThickness);
                ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr);
                for (int i = 0; i < os.Data.Count; i++)
                {
                    var p = os.Data[i];
                    double w = os.GetWidthFor(i); double half = w * 0.5;
                    float xC = PixelMapper.X(p.X, model.XAxis, pr);
                    float xL = PixelMapper.X(p.X - half * 0.7, model.XAxis, pr);
                    float xR = PixelMapper.X(p.X + half * 0.7, model.XAxis, pr);
                    float yOpen = PixelMapper.Y(p.Open, model.YAxis, pr);
                    float yHigh = PixelMapper.Y(p.High, model.YAxis, pr);
                    float yLow = PixelMapper.Y(p.Low, model.YAxis, pr);
                    float yClose = PixelMapper.Y(p.Close, model.YAxis, pr);
                    bool up = p.Close >= p.Open;
                    var bodyColor = up ? cUp : cDown;
                    byte fillAlpha = (byte)(Clamp01(up ? os.UpFillOpacity : os.DownFillOpacity) * bodyColor.A);
                    using var wick = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = wickStroke, Color = new SKColor(bodyColor.R, bodyColor.G, bodyColor.B, bodyColor.A) };
                    using var bodyFill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(bodyColor.R, bodyColor.G, bodyColor.B, fillAlpha) };
                    using var bodyStroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(bodyColor.R, bodyColor.G, bodyColor.B, bodyColor.A) };
                    ctx.Canvas.DrawLine(xC, yHigh, xC, yLow, wick);
                    float top = System.Math.Min(yOpen, yClose); float bot = System.Math.Max(yOpen, yClose);
                    var rect = SKRect.Create(xL, top, xR - xL, bot - top);
                    if (os.Filled) ctx.Canvas.DrawRect(rect, bodyFill);
                    ctx.Canvas.DrawRect(rect, bodyStroke);
                }
                ctx.Canvas.Restore();
            }
        }
    }
}
