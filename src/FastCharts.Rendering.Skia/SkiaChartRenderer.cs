using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia
{
    /// <summary>
    /// Minimal Skia renderer: draws axes (ticks + labels), grid, then the first LineSeries.
    /// Uses the model's Theme for colors/sizes.
    /// </summary>
    public sealed class SkiaChartRenderer : IRenderer<SKCanvas>
    {
        // Plot margins (pixels)
        private const float Left   = 48f;
        private const float Right  = 16f;
        private const float Top    = 16f;
        private const float Bottom = 36f;

        public void Render(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight)
        {
            if (model == null || canvas == null) return;
            
            // Prepare paints from theme
            var theme = model.Theme;

            // Clear to transparent so the WPF Border's Background shows through
            canvas.Clear(new SKColor(theme.SurfaceBackgroundColor.R,
                theme.SurfaceBackgroundColor.G,
                theme.SurfaceBackgroundColor.B,
                theme.SurfaceBackgroundColor.A));

            // Compute plot area (the inner rectangle where data is drawn)
            var plotW = (int)System.Math.Max(0, pixelWidth  - (Left + Right));
            var plotH = (int)System.Math.Max(0, pixelHeight - (Top + Bottom));
            if (plotW <= 0 || plotH <= 0)
                return;

            // Keep scales exact for the *plot* size (not the full control size)
            model.UpdateScales(plotW, plotH);

            

            canvas.Save();
            canvas.Translate(Left, Top);
            using (var bg = new SKPaint
                   {
                       Style = SKPaintStyle.Fill,
                       Color = new SKColor(theme.PlotBackgroundColor.R, theme.PlotBackgroundColor.G,
                           theme.PlotBackgroundColor.B, theme.PlotBackgroundColor.A)
                   })
            {
                canvas.DrawRect(0, 0, plotW, plotH, bg);
            }
            canvas.Restore();
            
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

            // Axis baselines (in surface coordinates)
            var x0 = Left;
            var x1 = pixelWidth - Right;
            var yBase = pixelHeight - Bottom;

            var y0 = Top;
            var y1 = pixelHeight - Bottom;
            var xBase = Left;

            // Draw axis lines
            canvas.DrawLine(x0, yBase, x1, yBase, axisPaint); // X axis
            canvas.DrawLine(xBase, y0, xBase, y1, axisPaint); // Y axis

            // Compute ticks (approx. pixel step → data step)
            var xDataPerPx = model.XAxis.VisibleRange.Size / System.Math.Max(1.0, plotW);
            var yDataPerPx = model.YAxis.VisibleRange.Size / System.Math.Max(1.0, plotH);

            var approxXStepData = 80.0 * xDataPerPx; // ~80 px between x ticks
            var approxYStepData = 50.0 * yDataPerPx; // ~50 px between y ticks

            var xTicks = model.XAxis.Ticker.GetTicks(model.XAxis.VisibleRange, approxXStepData);
            var yTicks = model.YAxis.Ticker.GetTicks(model.YAxis.VisibleRange, approxYStepData);

            // Draw grid inside plot area
            canvas.Save();
            canvas.Translate(Left, Top);

            // Vertical grid lines at X ticks
            foreach (var t in xTicks)
            {
                var x = (float)model.XAxis.Scale.ToPixels(t);
                canvas.DrawLine(x + 0.5f, 0, x + 0.5f, plotH, gridPaint);
            }

            // Horizontal grid lines at Y ticks
            foreach (var t in yTicks)
            {
                var y = (float)model.YAxis.Scale.ToPixels(t);
                canvas.DrawLine(0, y + 0.5f, plotW, y + 0.5f, gridPaint);
            }

            canvas.Restore();

            // Draw ticks and labels (surface coordinates)
            var tickLen = (float)theme.TickLength;

            // X-axis ticks & labels
            foreach (var t in xTicks)
            {
                var px = (float)model.XAxis.Scale.ToPixels(t) + Left;

                // tick
                canvas.DrawLine(px, yBase, px, yBase + tickLen, tickPaint);

                // label
                var label = t.ToString(model.XAxis.LabelFormat ?? "G");
                var w = textPaint.MeasureText(label);
                // place a bit below the ticks (3px padding)
                canvas.DrawText(label, px - w / 2f, yBase + tickLen + 3 + textPaint.TextSize, textPaint);
            }

            // Y-axis ticks & labels
            foreach (var t in yTicks)
            {
                var py = (float)model.YAxis.Scale.ToPixels(t) + Top;

                // tick
                canvas.DrawLine(xBase - tickLen, py, xBase, py, tickPaint);

                // label (right-aligned to left of axis, ~6px padding; +4 for baseline tweak)
                var label = t.ToString(model.YAxis.LabelFormat ?? "G");
                var w = textPaint.MeasureText(label);
                canvas.DrawText(label, xBase - tickLen - 6 - w, py + 4, textPaint);
            }

            // Draw all LineSeries in the plot area
            int seriesIndex = 0;
            canvas.Save();
            canvas.Translate(Left, Top);

            foreach (var ls in model.Series.OfType<LineSeries>())
            {
                if (ls.IsEmpty) { seriesIndex++; continue; }

                // pick color from palette or fallback
                var palette = model.Theme.SeriesPalette;
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
                    var x = (float)model.XAxis.Scale.ToPixels(p.X);
                    var y = (float)model.YAxis.Scale.ToPixels(p.Y);
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
