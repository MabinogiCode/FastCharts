using SkiaSharp;
using FastCharts.Core.Primitives;
using FastCharts.Core.Axes;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class GridLayer : IRenderLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model;
            var xr = model.XAxis.VisibleRange;
            var yr = model.YAxis.VisibleRange;
            var pr = ctx.PlotRect;
            if (xr.Size <= 0 || yr.Size <= 0)
            {
                return;
            }

            var xAxisBase = (AxisBase)model.XAxis;
            var yAxisBase = (AxisBase)model.YAxis;

            double xDataPerPx = xr.Size / System.Math.Max(1.0, pr.Width);
            double yDataPerPx = yr.Size / System.Math.Max(1.0, pr.Height);
            double approxXStepData = 80.0 * xDataPerPx;
            double approxYStepData = 50.0 * yDataPerPx;

            var xMaj = model.XAxis.Ticker.GetTicks(xr, approxXStepData);
            var yMaj = model.YAxis.Ticker.GetTicks(yr, approxYStepData);
            var xMin = (xAxisBase.ShowMinorGrid || xAxisBase.ShowMinorTicks) ? model.XAxis.Ticker.GetMinorTicks(xr, xMaj) : System.Array.Empty<double>();
            var yMin = (yAxisBase.ShowMinorGrid || yAxisBase.ShowMinorTicks) ? model.YAxis.Ticker.GetMinorTicks(yr, yMaj) : System.Array.Empty<double>();

            byte minorAlphaX = (byte)(ctx.Model.Theme.GridColor.A * xAxisBase.MinorGridOpacity);
            byte minorAlphaY = (byte)(ctx.Model.Theme.GridColor.A * yAxisBase.MinorGridOpacity);

            using var minorPaintX = new SKPaint { Color = new SKColor(ctx.Model.Theme.GridColor.R, ctx.Model.Theme.GridColor.G, ctx.Model.Theme.GridColor.B, minorAlphaX), Style = SKPaintStyle.Stroke, StrokeWidth = (float)ctx.Model.Theme.GridThickness, IsAntialias = false };
            using var minorPaintY = new SKPaint { Color = new SKColor(ctx.Model.Theme.GridColor.R, ctx.Model.Theme.GridColor.G, ctx.Model.Theme.GridColor.B, minorAlphaY), Style = SKPaintStyle.Stroke, StrokeWidth = (float)ctx.Model.Theme.GridThickness, IsAntialias = false };

            var c = ctx.Canvas;
            c.Save();
            c.ClipRect(pr);
            if (xAxisBase.ShowMinorGrid)
            {
                foreach (var t in xMin)
                {
                    float px = PixelMapper.X(t, model.XAxis, pr);
                    c.DrawLine(px, pr.Top, px, pr.Bottom, minorPaintX);
                }
            }
            if (yAxisBase.ShowMinorGrid)
            {
                foreach (var t in yMin)
                {
                    float py = PixelMapper.Y(t, model.YAxis, pr);
                    c.DrawLine(pr.Left, py, pr.Right, py, minorPaintY);
                }
            }
            foreach (var t in xMaj)
            {
                float px = PixelMapper.X(t, model.XAxis, pr);
                c.DrawLine(px, pr.Top, px, pr.Bottom, ctx.Paints.Grid);
            }
            foreach (var t in yMaj)
            {
                float py = PixelMapper.Y(t, model.YAxis, pr);
                c.DrawLine(pr.Left, py, pr.Right, py, ctx.Paints.Grid);
            }
            c.Restore();
        }
    }
}
