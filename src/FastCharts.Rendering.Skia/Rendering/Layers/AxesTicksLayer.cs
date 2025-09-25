using SkiaSharp;
using System.Linq;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class AxesTicksLayer : IRenderLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var c = ctx.Canvas;
            var paints = ctx.Paints;
            var theme = model.Theme;

            // Axis base lines
            float xBase = pr.Left;
            float yBase = pr.Bottom;
            c.DrawLine(pr.Left, yBase, pr.Right, yBase, paints.Axis);
            c.DrawLine(xBase, pr.Top, xBase, pr.Bottom, paints.Axis);

            var xr = model.XAxis.VisibleRange;
            var yr = model.YAxis.VisibleRange;
            if (xr.Size <= 0 || yr.Size <= 0) return;

            double xDataPerPx = xr.Size / System.Math.Max(1.0, pr.Width);
            double yDataPerPx = yr.Size / System.Math.Max(1.0, pr.Height);
            double approxXStepData = 80.0 * xDataPerPx;
            double approxYStepData = 50.0 * yDataPerPx;
            var xTicks = model.XAxis.Ticker.GetTicks(xr, approxXStepData);
            var yTicks = model.YAxis.Ticker.GetTicks(yr, approxYStepData);

            float tickLen = (float)theme.TickLength;
            foreach (var t in xTicks)
            {
                float px = PixelMapper.X(t, model.XAxis, pr);
                c.DrawLine(px, yBase, px, yBase + tickLen, paints.Tick);
                var xf = model.XAxis.NumberFormatter;
                var lbl = xf != null ? xf.Format(t) : t.ToString(model.XAxis.LabelFormat ?? "G");
                float w = paints.Text.MeasureText(lbl);
                c.DrawText(lbl, px - w / 2f, yBase + tickLen + 3 + paints.Text.TextSize, paints.Text);
            }
            foreach (var t in yTicks)
            {
                float py = PixelMapper.Y(t, model.YAxis, pr);
                c.DrawLine(xBase - tickLen, py, xBase, py, paints.Tick);
                var yf = model.YAxis.NumberFormatter;
                var lbl = yf != null ? yf.Format(t) : t.ToString(model.YAxis.LabelFormat ?? "G");
                float w = paints.Text.MeasureText(lbl);
                c.DrawText(lbl, xBase - tickLen - 6 - w, py + 4, paints.Text);
            }

            // Border
            c.DrawRect(pr, paints.Border);
        }
    }
}
