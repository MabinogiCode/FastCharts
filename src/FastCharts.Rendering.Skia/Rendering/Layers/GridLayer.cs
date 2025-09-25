using SkiaSharp;
using FastCharts.Core.Primitives;

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
            if (xr.Size <= 0 || yr.Size <= 0) return;

            double xDataPerPx = xr.Size / System.Math.Max(1.0, pr.Width);
            double yDataPerPx = yr.Size / System.Math.Max(1.0, pr.Height);
            double approxXStepData = 80.0 * xDataPerPx;
            double approxYStepData = 50.0 * yDataPerPx;

            var xTicks = model.XAxis.Ticker.GetTicks(xr, approxXStepData);
            var yTicks = model.YAxis.Ticker.GetTicks(yr, approxYStepData);

            var c = ctx.Canvas;
            c.Save();
            c.ClipRect(pr);
            foreach (var t in xTicks)
            {
                float px = PixelMapper.X(t, model.XAxis, pr);
                c.DrawLine(px, pr.Top, px, pr.Bottom, ctx.Paints.Grid);
            }
            foreach (var t in yTicks)
            {
                float py = PixelMapper.Y(t, model.YAxis, pr);
                c.DrawLine(pr.Left, py, pr.Right, py, ctx.Paints.Grid);
            }
            c.Restore();
        }
    }
}
