using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia
{
    /// <summary>
    /// Skia renderer: draws surface/plot backgrounds (from theme), grid, axes (ticks+labels),
    /// then all LineSeries using the theme palette. Pixel mapping is derived from VisibleRange.
    /// </summary>
    public sealed class SkiaChartRenderer : IRenderer<SKCanvas>
    {
        public void Render(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight)
        {
            if (model == null || canvas == null) return;

            var theme = model.Theme;

            // Margins come from the model (centralized layout).
            var m = model.PlotMargins;
            float left   = (float)m.Left;
            float top    = (float)m.Top;
            float right  = (float)m.Right;
            float bottom = (float)m.Bottom;

            // Fill the entire surface outside the plot (margins).
            canvas.Clear(new SKColor(
                theme.SurfaceBackgroundColor.R,
                theme.SurfaceBackgroundColor.G,
                theme.SurfaceBackgroundColor.B,
                theme.SurfaceBackgroundColor.A));

            // Compute plot area size.
            var plotW = (int)System.Math.Max(0, pixelWidth  - (left + right));
            var plotH = (int)System.Math.Max(0, pixelHeight - (top  + bottom));
            if (plotW <= 0 || plotH <= 0)
                return;

            // Keep internal scale math in sync with the plot size (if used elsewhere).
            model.UpdateScales(plotW, plotH);

            // Fill the plot background.
            canvas.Save();
            canvas.Translate(left, top);
            using (var bg = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(
                    theme.PlotBackgroundColor.R,
                    theme.PlotBackgroundColor.G,
                    theme.PlotBackgroundColor.B,
                    theme.PlotBackgroundColor.A)
            })
            {
                canvas.DrawRect(0, 0, plotW, plotH, bg);
            }
            canvas.Restore();

            // Prepare paints from theme.
            using var axisPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(theme.AxisColor.R, theme.AxisColor.G, theme.AxisColor.B, theme.AxisColor.A),
                StrokeWidth = (float)theme.AxisThickness
            };
            using var tickPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = axisPaint.Color,
                StrokeWidth = (float)theme.AxisThickness
            };
            using var textPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(theme.LabelColor.R, theme.LabelColor.G, theme.LabelColor.B, theme.LabelColor.A),
                TextSize = (float)theme.LabelTextSize
            };
            using var gridPaint = new SKPaint
            {
                IsAntialias = false,
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(theme.GridColor.R, theme.GridColor.G, theme.GridColor.B, theme.GridColor.A),
                StrokeWidth = (float)theme.GridThickness
            };

            // Axis base lines (surface coords).
            float xBase = left;
            float yBase = pixelHeight - bottom;
            float xAxisStart = left;
            float xAxisEnd   = pixelWidth - right;
            float yAxisStart = top;
            float yAxisEnd   = pixelHeight - bottom;

            canvas.DrawLine(xAxisStart, yBase, xAxisEnd, yBase, axisPaint); // X axis
            canvas.DrawLine(xBase, yAxisStart, xBase, yAxisEnd, axisPaint); // Y axis

            // Visible ranges drive all pixel mapping.
            var xr = model.XAxis.VisibleRange;
            var yr = model.YAxis.VisibleRange;
            if (xr.Size <= 0 || yr.Size <= 0) return;

            // Approximate tick steps in data units based on desired pixel spacing.
            double xDataPerPx = xr.Size / System.Math.Max(1.0, plotW);
            double yDataPerPx = yr.Size / System.Math.Max(1.0, plotH);
            double approxXStepData = 80.0 * xDataPerPx; // ~80 px between x ticks
            double approxYStepData = 50.0 * yDataPerPx; // ~50 px between y ticks

            var xTicks = model.XAxis.Ticker.GetTicks(xr, approxXStepData);
            var yTicks = model.YAxis.Ticker.GetTicks(yr, approxYStepData);

            // Local mapping functions: data -> pixels in plot coordinates (origin top-left).
            float XToPx(double x) => (float)((x - xr.Min) * (plotW / xr.Size));
            float YToPx(double y) => (float)(plotH - (y - yr.Min) * (plotH / yr.Size)); // invert Y

            // Grid inside plot area.
            canvas.Save();
            canvas.Translate(left, top);
            foreach (var t in xTicks)
            {
                var x = XToPx(t);
                canvas.DrawLine(x + 0.5f, 0, x + 0.5f, plotH, gridPaint);
            }
            foreach (var t in yTicks)
            {
                var y = YToPx(t);
                canvas.DrawLine(0, y + 0.5f, plotW, y + 0.5f, gridPaint);
            }
            canvas.Restore();

            // Ticks & labels (surface coords).
            float tickLen = (float)theme.TickLength;

            // X-axis ticks/labels
            foreach (var t in xTicks)
            {
                var px = XToPx(t) + left;
                canvas.DrawLine(px, yBase, px, yBase + tickLen, tickPaint);

                var label = t.ToString(model.XAxis.LabelFormat ?? "G");
                var wlab = textPaint.MeasureText(label);
                canvas.DrawText(label, px - wlab / 2f, yBase + tickLen + 3 + textPaint.TextSize, textPaint);
            }

            // Y-axis ticks/labels
            foreach (var t in yTicks)
            {
                var py = YToPx(t) + top;
                canvas.DrawLine(xBase - tickLen, py, xBase, py, tickPaint);

                var label = t.ToString(model.YAxis.LabelFormat ?? "G");
                var wlab = textPaint.MeasureText(label);
                canvas.DrawText(label, xBase - tickLen - 6 - wlab, py + 4, textPaint);
            }

            // Draw all LineSeries in the plot area with palette colors.
            var palette = model.Theme.SeriesPalette;
            int seriesIndex = 0;

            canvas.Save();
            canvas.Translate(left, top);

            foreach (var ls in model.Series.OfType<LineSeries>())
            {
                if (ls.IsEmpty) { seriesIndex++; continue; }

                // Pick color from palette or fallback to PrimarySeriesColor.
                var c = (palette != null && seriesIndex < palette.Count)
                    ? palette[seriesIndex]
                    : model.Theme.PrimarySeriesColor;

                using var seriesPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = (float)ls.StrokeThickness,
                    Color = new SKColor(c.R, c.G, c.B, c.A)
                };

                using var path = new SKPath();
                bool started = false;

                foreach (var p in ls.Data)
                {
                    var x = XToPx(p.X);
                    var y = YToPx(p.Y);
                    if (!started) { path.MoveTo(x, y); started = true; }
                    else          { path.LineTo(x, y); }
                }

                canvas.DrawPath(path, seriesPaint);
                seriesIndex++;
            }

            canvas.Restore();
        }
    }
}
