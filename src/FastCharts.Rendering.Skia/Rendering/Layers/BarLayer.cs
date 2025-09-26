using SkiaSharp;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class BarLayer : ISeriesSubLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model; var pr = ctx.PlotRect; var palette = model.Theme.SeriesPalette; int paletteCount = palette?.Count ?? 0;
            int barIndex = 0;
            foreach (var s in model.Series)
            {
                if (s is not BarSeries bs || bs.IsEmpty || !bs.IsVisible) continue;
                var c = (paletteCount > 0 && barIndex < paletteCount) ? palette[barIndex] : model.Theme.PrimarySeriesColor;
                byte alpha = (byte)(RenderMath.Clamp01(bs.FillOpacity) * c.A);
                using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(c.R, c.G, c.B, alpha) };
                using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)System.Math.Max(1.0, bs.StrokeThickness), Color = new SKColor(c.R, c.G, c.B, c.A) };
                int groupCount = bs.GroupCount.GetValueOrDefault(1);
                int groupIndex2 = bs.GroupIndex.GetValueOrDefault(0);
                if (groupCount < 1) groupCount = 1;
                if (groupIndex2 < 0) groupIndex2 = 0;
                if (groupIndex2 >= groupCount) groupIndex2 = groupCount - 1;
                const double innerGap = 0.9;
                ctx.Canvas.Save(); ctx.Canvas.ClipRect(pr);
                for (int i = 0; i < bs.Data.Count; i++)
                {
                    var p = bs.Data[i];
                    double bandW = bs.GetWidthFor(i);
                    double slotW = bandW / groupCount;
                    double effW = slotW * innerGap;
                    double groupOffsetFromCenter = ((groupIndex2 + 0.5) - (groupCount * 0.5)) * slotW;
                    float xL = PixelMapper.X(p.X + groupOffsetFromCenter - effW * 0.5, model.XAxis, pr);
                    float xR = PixelMapper.X(p.X + groupOffsetFromCenter + effW * 0.5, model.XAxis, pr);
                    float y0 = PixelMapper.Y(bs.Baseline, model.YAxis, pr);
                    float y1 = PixelMapper.Y(p.Y, model.YAxis, pr);
                    var rect = SKRect.Create(System.Math.Min(xL, xR), System.Math.Min(y0, y1), System.Math.Abs(xR - xL), System.Math.Abs(y1 - y0));
                    if (rect.Width <= 0 || rect.Height <= 0) continue;
                    ctx.Canvas.DrawRect(rect, fillPaint);
                    if (strokePaint.StrokeWidth > 0.5f) ctx.Canvas.DrawRect(rect, strokePaint);
                }
                ctx.Canvas.Restore();
                barIndex++;
            }
        }
    }
}
