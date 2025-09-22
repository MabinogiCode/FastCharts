using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Series;
using SkiaSharp;
using FastCharts.Rendering.Skia.Rendering; // PixelMapper

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

            // 1) Compute plot rect from margins (in surface coords)
            var m = model.PlotMargins;
            float left   = (float)m.Left;
            float top    = (float)m.Top;
            float right  = (float)m.Right;
            float bottom = (float)m.Bottom;

            float plotW = (float)System.Math.Max(0, pixelWidth  - (left + right));
            float plotH = (float)System.Math.Max(0, pixelHeight - (top  + bottom));
            if (plotW <= 0 || plotH <= 0)
            {
                canvas.Clear(new SKColor(
                    theme.SurfaceBackgroundColor.R,
                    theme.SurfaceBackgroundColor.G,
                    theme.SurfaceBackgroundColor.B,
                    theme.SurfaceBackgroundColor.A));
                return;
            }

            var plotRect = new SKRect(left, top, left + plotW, top + plotH);

            // 2) Clear surface background (outside plot)
            canvas.Clear(new SKColor(
                theme.SurfaceBackgroundColor.R,
                theme.SurfaceBackgroundColor.G,
                theme.SurfaceBackgroundColor.B,
                theme.SurfaceBackgroundColor.A));

            // 3) Sync internal scales (if used elsewhere)
            model.UpdateScales((int)plotW, (int)plotH);

            // 4) Fill plot background
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
                canvas.DrawRect(plotRect, bg);
            }

            // 5) Prepare paints
            var axisColor  = new SKColor(theme.AxisColor.R,   theme.AxisColor.G,   theme.AxisColor.B,   theme.AxisColor.A);
            var gridColor  = new SKColor(theme.GridColor.R,   theme.GridColor.G,   theme.GridColor.B,   theme.GridColor.A);
            var labelColor = new SKColor(theme.LabelColor.R,  theme.LabelColor.G,  theme.LabelColor.B,  theme.LabelColor.A);

            using (var axisPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = axisColor, StrokeWidth = (float)theme.AxisThickness })
            using (var tickPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = axisColor, StrokeWidth = (float)theme.AxisThickness })
            using (var textPaint = new SKPaint { IsAntialias = true, Color = labelColor, TextSize = (float)theme.LabelTextSize })
            using (var gridPaint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Stroke, Color = gridColor, StrokeWidth = (float)theme.GridThickness })
            {
                // 6) Axis base lines (surface coords)
                float xBase = plotRect.Left;
                float yBase = plotRect.Bottom;
                float xAxisStart = plotRect.Left;
                float xAxisEnd   = plotRect.Right;
                float yAxisStart = plotRect.Top;
                float yAxisEnd   = plotRect.Bottom;

                canvas.DrawLine(xAxisStart, yBase, xAxisEnd, yBase, axisPaint); // X axis
                canvas.DrawLine(xBase, yAxisStart, xBase, yAxisEnd, axisPaint); // Y axis

                // 7) Visible ranges (guard)
                var xr = model.XAxis.VisibleRange;
                var yr = model.YAxis.VisibleRange;
                if (xr.Size <= 0 || yr.Size <= 0) return;

                // 8) Tick generation (approx pixel spacing → data step)
                double xDataPerPx = xr.Size / System.Math.Max(1.0, plotW);
                double yDataPerPx = yr.Size / System.Math.Max(1.0, plotH);
                double approxXStepData = 80.0 * xDataPerPx; // ~80 px between X ticks
                double approxYStepData = 50.0 * yDataPerPx; // ~50 px between Y ticks

                var xTicks = model.XAxis.Ticker.GetTicks(xr, approxXStepData);
                var yTicks = model.YAxis.Ticker.GetTicks(yr, approxYStepData);

                // 9) GRID + SERIES clipped to plotRect
                canvas.Save();
                canvas.ClipRect(plotRect);

                // Grid (verticals)
                foreach (var t in xTicks)
                {
                    float px = PixelMapper.X(t, model.XAxis, plotRect);
                    canvas.DrawLine(px, plotRect.Top, px, plotRect.Bottom, gridPaint);
                }
                // Grid (horizontals)
                foreach (var t in yTicks)
                {
                    float py = PixelMapper.Y(t, model.YAxis, plotRect);
                    canvas.DrawLine(plotRect.Left, py, plotRect.Right, py, gridPaint);
                }

                // Series (LineSeries)
                var palette = model.Theme.SeriesPalette;
                int seriesIndex = 0;

                foreach (var ls in model.Series.OfType<LineSeries>())
                {
                    if (ls.IsEmpty) { seriesIndex++; continue; }

                    var c = (palette != null && seriesIndex < palette.Count)
                        ? palette[seriesIndex]
                        : model.Theme.PrimarySeriesColor;

                    using (var seriesPaint = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = (float)ls.StrokeThickness,
                        Color = new SKColor(c.R, c.G, c.B, c.A)
                    })
                    using (var path = new SKPath())
                    {
                        bool started = false;
                        foreach (var p in ls.Data)
                        {
                            float px = PixelMapper.X(p.X, model.XAxis, plotRect);
                            float py = PixelMapper.Y(p.Y, model.YAxis, plotRect);
                            if (!started) { path.MoveTo(px, py); started = true; }
                            else          { path.LineTo(px, py); }
                        }
                        canvas.DrawPath(path, seriesPaint);
                    }

                    seriesIndex++;
                }

                canvas.Restore(); // end clip

                // 10) Ticks & labels (outside clip)
                float tickLen = (float)theme.TickLength;

                // X-axis ticks + labels
                foreach (var t in xTicks)
                {
                    float px = PixelMapper.X(t, model.XAxis, plotRect);
                    canvas.DrawLine(px, yBase, px, yBase + tickLen, tickPaint);

                    var label = t.ToString(model.XAxis.LabelFormat ?? "G");
                    float wlab = textPaint.MeasureText(label);
                    canvas.DrawText(label, px - wlab / 2f, yBase + tickLen + 3 + textPaint.TextSize, textPaint);
                }

                // Y-axis ticks + labels
                foreach (var t in yTicks)
                {
                    float py = PixelMapper.Y(t, model.YAxis, plotRect);
                    canvas.DrawLine(xBase - tickLen, py, xBase, py, tickPaint);

                    var label = t.ToString(model.YAxis.LabelFormat ?? "G");
                    float wlab = textPaint.MeasureText(label);
                    canvas.DrawText(label, xBase - tickLen - 6 - wlab, py + 4, textPaint);
                }

                // 11) Plot border on top
                using (var border = new SKPaint { Color = axisColor, Style = SKPaintStyle.Stroke, StrokeWidth = 1 })
                {
                    canvas.DrawRect(plotRect, border);
                }
            }
        }
    }
}
