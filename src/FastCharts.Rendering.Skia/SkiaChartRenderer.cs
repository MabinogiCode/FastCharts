using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
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
            float left = (float)m.Left;
            float top = (float)m.Top;
            float right = (float)m.Right;
            float bottom = (float)m.Bottom;

            float plotW = (float)System.Math.Max(0, pixelWidth - (left + right));
            float plotH = (float)System.Math.Max(0, pixelHeight - (top + bottom));
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
            var axisColor = new SKColor(theme.AxisColor.R, theme.AxisColor.G, theme.AxisColor.B, theme.AxisColor.A);
            var gridColor = new SKColor(theme.GridColor.R, theme.GridColor.G, theme.GridColor.B, theme.GridColor.A);
            var labelColor = new SKColor(theme.LabelColor.R, theme.LabelColor.G, theme.LabelColor.B,
                theme.LabelColor.A);

            using (var axisPaint = new SKPaint
                   {
                       IsAntialias = true, Style = SKPaintStyle.Stroke, Color = axisColor,
                       StrokeWidth = (float)theme.AxisThickness
                   })
            using (var tickPaint = new SKPaint
                   {
                       IsAntialias = true, Style = SKPaintStyle.Stroke, Color = axisColor,
                       StrokeWidth = (float)theme.AxisThickness
                   })
            using (var textPaint = new SKPaint
                       { IsAntialias = true, Color = labelColor, TextSize = (float)theme.LabelTextSize })
            using (var gridPaint = new SKPaint
                   {
                       IsAntialias = false, Style = SKPaintStyle.Stroke, Color = gridColor,
                       StrokeWidth = (float)theme.GridThickness
                   })
            {
                // 6) Axis base lines (surface coords)
                float xBase = plotRect.Left;
                float yBase = plotRect.Bottom;
                float xAxisStart = plotRect.Left;
                float xAxisEnd = plotRect.Right;
                float yAxisStart = plotRect.Top;
                float yAxisEnd = plotRect.Bottom;

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

                // Series rendering (Area -> Scatter -> Plain Lines), clipped to plotRect above.

                var palette = model.Theme.SeriesPalette;
                int seriesIndex = 0;

                // --- 1) AREA (fill + outline) ---
                foreach (var ls in model.Series.OfType<LineSeries>())
                {
                    if (ls.IsEmpty)
                    {
                        seriesIndex++;
                        continue;
                    }

                    var c = (palette != null && seriesIndex < palette.Count)
                        ? palette[seriesIndex]
                        : model.Theme.PrimarySeriesColor;

                    var area = ls as AreaSeries;
                    if (area != null)
                    {
                        // Build closed path baseline -> points -> baseline
                        using (var fillPath = new SKPath())
                        {
                            bool started = false;
                            double baseline = area.Baseline;

                            double firstX = 0.0;
                            bool hasFirstX = false;

                            foreach (var p in area.Data)
                            {
                                float px = PixelMapper.X(p.X, model.XAxis, plotRect);
                                float pyBase = PixelMapper.Y(baseline, model.YAxis, plotRect);

                                if (!started)
                                {
                                    fillPath.MoveTo(px, pyBase);
                                    started = true;
                                    firstX = p.X;
                                    hasFirstX = true;
                                }

                                float py = PixelMapper.Y(p.Y, model.YAxis, plotRect);
                                fillPath.LineTo(px, py);
                            }

                            if (started && hasFirstX)
                            {
                                // Close to baseline at last X
                                var last = default(FastCharts.Core.Primitives.PointD);
                                foreach (var p in area.Data)
                                {
                                    last = p;
                                }

                                float lastPx = PixelMapper.X(last.X, model.XAxis, plotRect);
                                float basePy = PixelMapper.Y(baseline, model.YAxis, plotRect);

                                fillPath.LineTo(lastPx, basePy);
                                fillPath.Close();

                                byte alpha = (byte)(System.Math.Max(0.0, System.Math.Min(1.0, area.FillOpacity)) * c.A);
                                var fillColor = new SKColor(c.R, c.G, c.B, alpha);

                                using (var fillPaint = new SKPaint
                                       {
                                           IsAntialias = true,
                                           Style = SKPaintStyle.Fill,
                                           Color = fillColor
                                       })
                                {
                                    canvas.DrawPath(fillPath, fillPaint);
                                }
                            }
                        }
                    }

                    // Line outline for AreaSeries will be drawn in the "plain lines" pass below (shared code path).
                    seriesIndex++;
                }

                // --- 2) SCATTER (markers only, no line) ---
                seriesIndex = 0;
                foreach (var ss in model.Series.OfType<ScatterSeries>())
                {
                    if (ss.IsEmpty)
                    {
                        seriesIndex++;
                        continue;
                    }

                    var c = (palette != null && seriesIndex < palette.Count)
                        ? palette[seriesIndex]
                        : model.Theme.PrimarySeriesColor;

                    using (var markerPaint = new SKPaint
                           {
                               IsAntialias = true,
                               Style = SKPaintStyle.Fill,
                               Color = new SKColor(c.R, c.G, c.B, c.A)
                           })
                    {
                        float size = (float)ss.MarkerSize;
                        if (size < 1f)
                        {
                            size = 1f;
                        }

                        float half = size * 0.5f;

                        foreach (var p in ss.Data)
                        {
                            float px = PixelMapper.X(p.X, model.XAxis, plotRect);
                            float py = PixelMapper.Y(p.Y, model.YAxis, plotRect);

                            // Draw selected marker shape
                            if (ss.MarkerShape == FastCharts.Core.Series.MarkerShape.Circle)
                            {
                                canvas.DrawCircle(px, py, half, markerPaint);
                            }
                            else if (ss.MarkerShape == FastCharts.Core.Series.MarkerShape.Square)
                            {
                                var r = new SKRect(px - half, py - half, px + half, py + half);
                                canvas.DrawRect(r, markerPaint);
                            }
                            else // Triangle
                            {
                                using (var path = new SKPath())
                                {
                                    // Up-pointing triangle
                                    path.MoveTo(px, py - half);
                                    path.LineTo(px - half, py + half);
                                    path.LineTo(px + half, py + half);
                                    path.Close();
                                    canvas.DrawPath(path, markerPaint);
                                }
                            }
                        }
                    }

                    seriesIndex++;
                }

                // --- 3) PLAIN LINES (exclude Area & Scatter) ---
                seriesIndex = 0;
                foreach (var ls in model.Series.OfType<LineSeries>())
                {
                    if (ls is AreaSeries || ls is ScatterSeries)
                    {
                        seriesIndex++;
                        continue;
                    }

                    if (ls.IsEmpty)
                    {
                        seriesIndex++;
                        continue;
                    }

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

                            if (!started)
                            {
                                path.MoveTo(px, py);
                                started = true;
                            }
                            else
                            {
                                path.LineTo(px, py);
                            }
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

                    var xf = model.XAxis.NumberFormatter;
                    var xLabel = xf != null ? xf.Format(t) : t.ToString(model.XAxis.LabelFormat ?? "G");
                    float xWidth = textPaint.MeasureText(xLabel);
                    canvas.DrawText(xLabel, px - xWidth / 2f, yBase + tickLen + 3 + textPaint.TextSize, textPaint);
                }

                // Y-axis ticks + labels
                foreach (var t in yTicks)
                {
                    var py = PixelMapper.Y(t, model.YAxis, plotRect);
                    canvas.DrawLine(xBase - tickLen, py, xBase, py, tickPaint);

                    var yf = model.YAxis.NumberFormatter;
                    var yLabel = yf != null ? yf.Format(t) : t.ToString(model.YAxis.LabelFormat ?? "G");
                    var yWidth = textPaint.MeasureText(yLabel);
                    canvas.DrawText(yLabel, xBase - tickLen - 6 - yWidth, py + 4, textPaint);
                }

                // 11) Plot border on top
                using (var border = new SKPaint { Color = axisColor, Style = SKPaintStyle.Stroke, StrokeWidth = 1 })
                {
                    canvas.DrawRect(plotRect, border);
                }

                // 12) Overlay: crosshair + tooltip (if any)
                var st = model.InteractionState;
                if (st != null && st.ShowCrosshair)
                {
                    // Clamp cursor to plot rect
                    float cx = (float)st.PixelX;
                    float cy = (float)st.PixelY;
                    if (cx < plotRect.Left) cx = plotRect.Left;
                    else if (cx > plotRect.Right) cx = plotRect.Right;
                    if (cy < plotRect.Top) cy = plotRect.Top;
                    else if (cy > plotRect.Bottom) cy = plotRect.Bottom;

                    using (var cross = new SKPaint
                           {
                               Color = new SKColor(0, 0, 0, 80), IsAntialias = false, Style = SKPaintStyle.Stroke,
                               StrokeWidth = 1
                           })
                    using (var tipBg = new SKPaint
                           {
                               Color = new SKColor(255, 255, 255, 230), IsAntialias = true, Style = SKPaintStyle.Fill
                           })
                    using (var tipBd = new SKPaint
                           {
                               Color = new SKColor(0, 0, 0, 120), IsAntialias = true, Style = SKPaintStyle.Stroke,
                               StrokeWidth = 1
                           })
                    using (var tipTx = new SKPaint
                           {
                               Color = new SKColor(30, 30, 30, 255), IsAntialias = true,
                               TextSize = (float)model.Theme.LabelTextSize
                           })
                    {
                        // Crosshair lines (clipped to plot)
                        canvas.Save();
                        canvas.ClipRect(plotRect);
                        canvas.DrawLine(plotRect.Left, cy, plotRect.Right, cy, cross);
                        canvas.DrawLine(cx, plotRect.Top, cx, plotRect.Bottom, cross);
                        canvas.Restore();

                        // Tooltip (optional)
                        if (!string.IsNullOrEmpty(st.TooltipText))
                        {
                            var lines = st.TooltipText?.Split('\n');
                            if (lines != null && lines.Length > 0)
                            {
                                float maxW = 0;
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    float w = tipTx.MeasureText(lines[i]);
                                    if (w > maxW) maxW = w;
                                }

                                float lineH = tipTx.TextSize + 2;
                                float pad = 6;
                                float boxW = maxW + pad * 2;
                                float boxH = lineH * lines.Length + pad * 2;

                                // Position the tooltip near the cursor (inside the plot when possible)
                                float bx = cx + 12;
                                float by = cy - boxH - 12;
                                if (bx + boxW > plotRect.Right) bx = plotRect.Right - boxW - 1;
                                if (by < plotRect.Top) by = cy + 12;

                                var rect = new SKRect(bx, by, bx + boxW, by + boxH);
                                canvas.DrawRect(rect, tipBg);
                                canvas.DrawRect(rect, tipBd);

                                float tx = bx + pad;
                                float ty = by + pad + tipTx.TextSize;
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    canvas.DrawText(lines[i], tx, ty, tipTx);
                                    ty += lineH;
                                }                                
                            }
                        }
                    }
                }
            }
        }
    }
}
