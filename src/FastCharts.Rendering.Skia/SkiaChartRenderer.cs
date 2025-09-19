using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Series;

using SkiaSharp;

namespace FastCharts.Rendering.Skia
{
    public sealed class SkiaChartRenderer : IRenderer<SKCanvas>
    {
        // Simple plot margins (px) — later we’ll make this themable
        private const float Left = 48f;
        private const float Right = 16f;
        private const float Top = 16f;
        private const float Bottom = 36f;

        public void Render(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight)
        {
            if (model == null || canvas == null) return;

            // Full-surface clear (let WPF Border show Background)
            canvas.Clear(SKColors.Transparent);

            // Plot area
            var plotW = (int)System.Math.Max(0, pixelWidth - (Left + Right));
            var plotH = (int)System.Math.Max(0, pixelHeight - (Top + Bottom));

            // Keep scales exact for plot area size (not full control size)
            model.UpdateScales(plotW, plotH);

            // Draw axes (ticks + labels) around plot area
            DrawAxes(canvas, model, pixelWidth, pixelHeight, plotW, plotH);

            // Draw first LineSeries inside plot area
            var ls = model.Series.OfType<LineSeries>().FirstOrDefault();
            if (ls is null || ls.IsEmpty) return;

            canvas.Save();
            canvas.Translate(Left, Top);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (float)ls.StrokeThickness,
                Color = new SKColor(0x33, 0x99, 0xFF)
            };

            using var path = new SKPath();
            bool started = false;

            foreach (var p in ls.Data)
            {
                var x = (float)model.XAxis.Scale.ToPixels(p.X);
                var y = (float)model.YAxis.Scale.ToPixels(p.Y);
                if (!started) { path.MoveTo(x, y); started = true; }
                else { path.LineTo(x, y); }
            }

            canvas.DrawPath(path, paint);
            canvas.Restore();
        }

        private static void DrawAxes(SKCanvas canvas, ChartModel model,
                                     int surfaceW, int surfaceH, int plotW, int plotH)
        {
            // Axis lines
            using var axisPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(0x99, 0x99, 0x99),
                StrokeWidth = 1
            };

            // X axis (bottom)
            var x0 = Left;
            var x1 = surfaceW - Right;
            var yBase = surfaceH - Bottom;
            canvas.DrawLine(x0, yBase, x1, yBase, axisPaint);

            // Y axis (left)
            var y0 = Top;
            var y1 = surfaceH - Bottom;
            var xBase = Left;
            canvas.DrawLine(xBase, y0, xBase, y1, axisPaint);

            // Ticks & labels
            using var tickPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = axisPaint.Color,
                StrokeWidth = 1
            };
            using var textPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(0x88, 0x88, 0x88),
                TextSize = 12
            };

            // X ticks (use approx 80px step)
            var approxXStepInPx = 80.0;
            var xDataPerPx = (model.XAxis.VisibleRange.Size) / System.Math.Max(1.0, plotW);
            var approxXStepData = approxXStepInPx * xDataPerPx;

            var xTicks = model.XAxis.Ticker.GetTicks(model.XAxis.VisibleRange, approxXStepData);
            foreach (var t in xTicks)
            {
                var px = (float)model.XAxis.Scale.ToPixels(t) + Left;
                // tick
                canvas.DrawLine(px, yBase, px, yBase + 5, tickPaint);

                // label
                var label = t.ToString(model.XAxis.LabelFormat ?? "G");
                var w = textPaint.MeasureText(label);
                canvas.DrawText(label, px - w / 2f, yBase + 18, textPaint);
            }

            // Y ticks (use approx 50px step)
            var approxYStepInPx = 50.0;
            var yDataPerPx = (model.YAxis.VisibleRange.Size) / System.Math.Max(1.0, plotH);
            var approxYStepData = approxYStepInPx * yDataPerPx;

            var yTicks = model.YAxis.Ticker.GetTicks(model.YAxis.VisibleRange, approxYStepData);
            foreach (var t in yTicks)
            {
                var py = (float)model.YAxis.Scale.ToPixels(t) + Top;
                // tick
                canvas.DrawLine(xBase - 5, py, xBase, py, tickPaint);

                // label (right-aligned to left margin - 6px)
                var label = t.ToString(model.YAxis.LabelFormat ?? "G");
                var w = textPaint.MeasureText(label);
                canvas.DrawText(label, xBase - 6 - w, py + 4, textPaint);
            }
        }
    }
}
