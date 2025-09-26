using SkiaSharp;
using System.Linq;
using FastCharts.Core.Axes;
using System;
using System.Globalization;

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
            bool xIsDate = model.XAxis is DateTimeAxis;
            foreach (var t in xTicks)
            {
                float px = PixelMapper.X(t, model.XAxis, pr);
                c.DrawLine(px, yBase, px, yBase + tickLen, paints.Tick);
                string lbl;
                if (xIsDate)
                {
                    var dx = (DateTimeAxis)((object)model.XAxis);
                    var fmt = dx.DateTimeFormatter;
                    if (fmt != null)
                    {
                        double spanDays = xr.Size;
                        if (fmt is FastCharts.Core.Formatting.AdaptiveDateTimeFormatter ad)
                        {
                            ad.VisibleSpanDaysHint = spanDays;
                        }
                        lbl = fmt.Format(DateTime.FromOADate(t));
                    }
                    else
                    {
                        lbl = DateTime.FromOADate(t).ToString(dx.LabelFormat ?? "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    var xf = (model.XAxis as NumericAxis)?.NumberFormatter;
                    lbl = xf != null ? xf.Format(t) : t.ToString((model.XAxis as NumericAxis)?.LabelFormat ?? "G", CultureInfo.InvariantCulture);
                }
                float w = paints.Text.MeasureText(lbl);
                c.DrawText(lbl, px - w / 2f, yBase + tickLen + 3 + paints.Text.TextSize, paints.Text);
            }
            foreach (var t in yTicks)
            {
                float py = PixelMapper.Y(t, model.YAxis, pr);
                c.DrawLine(xBase - tickLen, py, xBase, py, paints.Tick);
                var ny = model.YAxis as NumericAxis;
                var yf = ny?.NumberFormatter;
                var lbl = yf != null ? yf.Format(t) : t.ToString(ny?.LabelFormat ?? "G", CultureInfo.InvariantCulture);
                float w = paints.Text.MeasureText(lbl);
                c.DrawText(lbl, xBase - tickLen - 6 - w, py + 4, paints.Text);
            }

            // Border
            c.DrawRect(pr, paints.Border);
        }
    }
}
