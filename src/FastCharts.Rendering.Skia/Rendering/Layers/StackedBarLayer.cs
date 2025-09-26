using SkiaSharp;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class StackedBarLayer : ISeriesSubLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var palette = model.Theme.SeriesPalette;
            int paletteCount = palette?.Count ?? 0;
            int sbarIndex = 0;
            foreach (var s in model.Series)
            {
                if (s is not StackedBarSeries sbs || sbs.IsEmpty || !sbs.IsVisible)
                {
                    continue;
                }
                int groupCount = sbs.GroupCount.GetValueOrDefault(1);
                int groupIndex = sbs.GroupIndex.GetValueOrDefault(0);
                if (groupCount < 1) { groupCount = 1; }
                if (groupIndex < 0) { groupIndex = 0; }
                if (groupIndex >= groupCount) { groupIndex = groupCount - 1; }
                const double innerGap = 0.9;
                ctx.Canvas.Save();
                ctx.Canvas.ClipRect(pr);
                for (int i = 0; i < sbs.Data.Count; i++)
                {
                    var p = sbs.Data[i];
                    double bandW = sbs.GetWidthFor(i);
                    double slotW = bandW / groupCount;
                    double effW = slotW * innerGap;
                    double groupOffset = ((groupIndex + 0.5) - (groupCount * 0.5)) * slotW;
                    float xL = PixelMapper.X(p.X + groupOffset - effW * 0.5, model.XAxis, pr);
                    float xR = PixelMapper.X(p.X + groupOffset + effW * 0.5, model.XAxis, pr);
                    double accPos = sbs.Baseline;
                    double accNeg = sbs.Baseline;
                    if (p.Values != null && p.Values.Length > 0)
                    {
                        for (int seg = 0; seg < p.Values.Length; seg++)
                        {
                            double v = p.Values[seg];
                            double yStart; double yEnd;
                            if (v >= 0)
                            {
                                yStart = accPos; yEnd = accPos + v; accPos = yEnd;
                            }
                            else
                            {
                                yStart = accNeg; yEnd = accNeg + v; accNeg = yEnd;
                            }
                            var col = (paletteCount > 0 && palette != null) ? palette[seg % paletteCount] : model.Theme.PrimarySeriesColor;
                            byte alpha = (byte)(RenderMath.Clamp01(sbs.FillOpacity) * col.A);
                            using var fillSeg = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(col.R, col.G, col.B, alpha) };
                            using var strokeSeg = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)System.Math.Max(1.0, sbs.StrokeThickness), Color = new SKColor(col.R, col.G, col.B, col.A) };
                            float y0 = PixelMapper.Y(yStart, model.YAxis, pr);
                            float y1 = PixelMapper.Y(yEnd, model.YAxis, pr);
                            var rect = SKRect.Create(System.Math.Min(xL, xR), System.Math.Min(y0, y1), System.Math.Abs(xR - xL), System.Math.Abs(y1 - y0));
                            if (rect.Width <= 0 || rect.Height <= 0)
                            {
                                continue;
                            }
                            ctx.Canvas.DrawRect(rect, fillSeg);
                            if (strokeSeg.StrokeWidth > 0.5f)
                            {
                                ctx.Canvas.DrawRect(rect, strokeSeg);
                            }
                        }
                    }
                }
                ctx.Canvas.Restore();
                sbarIndex++;
            }
        }
    }
}
