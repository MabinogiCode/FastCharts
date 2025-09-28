using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers;

internal sealed class StackedBarLayer : ISeriesSubLayer
{
    public void Render(RenderContext ctx)
    {
        var model = ctx.Model;
        var pr = ctx.PlotRect;
        var palette = model.Theme.SeriesPalette;
        var paletteCount = palette?.Count ?? 0;
        var sbarIndex = 0;
        foreach (var s in model.Series)
        {
            if (s is not StackedBarSeries sbs || sbs.IsEmpty || !sbs.IsVisible)
            {
                continue;
            }
            var groupCount = sbs.GroupCount.GetValueOrDefault(1);
            var groupIndex = sbs.GroupIndex.GetValueOrDefault(0);
            if (groupCount < 1) { groupCount = 1; }
            if (groupIndex < 0) { groupIndex = 0; }
            if (groupIndex >= groupCount) { groupIndex = groupCount - 1; }
            const double innerGap = 0.9;
            ctx.Canvas.Save();
            ctx.Canvas.ClipRect(pr);
            for (var i = 0; i < sbs.Data.Count; i++)
            {
                var p = sbs.Data[i];
                var bandW = sbs.GetWidthFor(i);
                var slotW = bandW / groupCount;
                var effW = slotW * innerGap;
                var groupOffset = ((groupIndex + 0.5) - (groupCount * 0.5)) * slotW;
                var xL = PixelMapper.X(p.X + groupOffset - effW * 0.5, model.XAxis, pr);
                var xR = PixelMapper.X(p.X + groupOffset + effW * 0.5, model.XAxis, pr);
                var accPos = sbs.Baseline;
                var accNeg = sbs.Baseline;
                var yAxis = (sbs.YAxisIndex == 1 && model.YAxisSecondary != null) ? model.YAxisSecondary : model.YAxis;
                if (p.Values != null && p.Values.Length > 0)
                {
                    for (var seg = 0; seg < p.Values.Length; seg++)
                    {
                        var v = p.Values[seg];
                        double yStart, yEnd;
                        if (v >= 0)
                        {
                            yStart = accPos; yEnd = accPos + v; accPos = yEnd;
                        }
                        else
                        {
                            yStart = accNeg; yEnd = accNeg + v; accNeg = yEnd;
                        }
                        var col = (paletteCount > 0 && palette != null) ? palette[seg % paletteCount] : model.Theme.PrimarySeriesColor;
                        var alpha = (byte)(RenderMath.Clamp01(sbs.FillOpacity) * col.A);
                        using var fillSeg = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(col.R, col.G, col.B, alpha) };
                        using var strokeSeg = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)System.Math.Max(1.0, sbs.StrokeThickness), Color = new SKColor(col.R, col.G, col.B, col.A) };
                        var y0 = PixelMapper.Y(yStart, yAxis, pr);
                        var y1 = PixelMapper.Y(yEnd, yAxis, pr);
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
