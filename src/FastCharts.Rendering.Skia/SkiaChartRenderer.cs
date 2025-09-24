using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

using SkiaSharp;

using FastCharts.Rendering.Skia.Rendering; // PixelMapper + new component renderers

namespace FastCharts.Rendering.Skia
{
    /// <summary>
    /// Orchestrates Skia rendering by delegating each chart element to a specialized renderer.
    /// </summary>
    public sealed class SkiaChartRenderer : IRenderer<SKCanvas>
    {
        public void Render(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight)
        {
            if (model == null || canvas == null)
            {
                return;
            }

            var theme = model.Theme;

            // 1) Compute plot rect from margins (in surface coords)
            var m = model.PlotMargins;
            float left = (float)m.Left;
            float top = (float)m.Top;
            float right = (float)m.Right;
            float bottom = (float)m.Bottom;

            float plotW = (float)System.Math.Max(0, pixelWidth - (left + right));
            float plotH = (float)System.Math.Max(0, pixelHeight - (top + bottom));
            if (plotW <= 0 || plotH <= 0)
            {
                // Clear surface only and exit
                canvas.Clear(new SKColor(
                    theme.SurfaceBackgroundColor.R,
                    theme.SurfaceBackgroundColor.G,
                    theme.SurfaceBackgroundColor.B,
                    theme.SurfaceBackgroundColor.A));
                return;
            }

            var plotRect = new SKRect(left, top, left + plotW, top + plotH);

            // 2) Clear surface background
            SkiaBackgroundRenderer.RenderSurfaceBackground(model, canvas);

            // 3) Update internal scales
            model.UpdateScales((int)plotW, (int)plotH);

            // 4) Draw plot background
            SkiaBackgroundRenderer.RenderPlotBackground(model, canvas, plotRect);

            // 5) Visible range guard
            var xr = model.XAxis.VisibleRange;
            var yr = model.YAxis.VisibleRange;
            if (xr.Size <= 0 || yr.Size <= 0)
            {
                return;
            }

            // 6) Axis base lines (drawn before grid & series for layering)
            SkiaAxisBaseLinesRenderer.Render(model, canvas, plotRect);

            // 7) Tick generation (shared by grid + labels)
            double xDataPerPx = xr.Size / System.Math.Max(1.0, plotW);
            double yDataPerPx = yr.Size / System.Math.Max(1.0, plotH);
            double approxXStepData = 80.0 * xDataPerPx; // ~80 px between X ticks
            double approxYStepData = 50.0 * yDataPerPx; // ~50 px between Y ticks
            var xTicks = model.XAxis.Ticker.GetTicks(xr, approxXStepData);
            var yTicks = model.YAxis.Ticker.GetTicks(yr, approxYStepData);

            // 8) Clip + grid + series
            canvas.Save();
            canvas.ClipRect(plotRect);

            SkiaGridRenderer.Render(model, canvas, plotRect, xTicks, yTicks);
            SkiaAreaSeriesRenderer.Render(model, canvas, plotRect);
            SkiaBandSeriesRenderer.Render(model, canvas, plotRect);
            SkiaScatterSeriesRenderer.Render(model, canvas, plotRect);
            SkiaLineSeriesRenderer.Render(model, canvas, plotRect); // outlines incl. area lines

            canvas.Restore();

            // 9) Axis ticks + labels
            SkiaAxisTicksLabelsRenderer.Render(model, canvas, plotRect, xTicks, yTicks);

            // 10) Border
            SkiaBorderRenderer.Render(model, canvas, plotRect);

            // 11) Overlays (crosshair, tooltip, etc.)
            SkiaOverlayRenderer.Render(model, canvas, plotRect);
        }
    }
}
