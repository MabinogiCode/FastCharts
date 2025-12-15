using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Helpers;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia.Helpers;
using FastCharts.Rendering.Skia.Rendering;
using FastCharts.Rendering.Skia.Rendering.Layers;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastCharts.Rendering.Skia
{
    public sealed class SkiaChartRenderer : IAsyncChartRenderer<SKCanvas>, IChartExporter, IRenderer<SKCanvas>
    {
        private readonly GridLayer _grid = new();
        private readonly SeriesLayer _series = new();
        private readonly AnnotationLayer _annotations = new();
        private readonly AxesTicksLayer _axesTicks = new();
        private readonly LegendLayer _legend = new();

        public void Render(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Chart model cannot be null");
            }
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas), "Canvas cannot be null");
            }
            if (pixelWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), pixelWidth, "Pixel width must be positive");
            }
            if (pixelHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pixelHeight), pixelHeight, "Pixel height must be positive");
            }

            var originalXRange = model.XAxis.VisibleRange;
            var originalYRange = model.YAxis.VisibleRange;
            var xRange = originalXRange;
            var yRange = originalYRange;

            if (!ValidationHelper.IsValidRange(xRange))
            {
                var tol = 1e-12;
                if (Math.Abs(xRange.Min - xRange.Max) < tol && ValidationHelper.IsFinite(xRange.Min))
                {
                    xRange = new FRange(xRange.Min, xRange.Min + 1);
                }
                else
                {
                    throw new InvalidOperationException($"X-axis visible range is invalid: [{xRange.Min}, {xRange.Max}]");
                }
            }
            if (!ValidationHelper.IsValidRange(yRange))
            {
                var tol = 1e-12;
                if (Math.Abs(yRange.Min - yRange.Max) < tol && ValidationHelper.IsFinite(yRange.Min))
                {
                    yRange = new FRange(yRange.Min, yRange.Min + 1);
                }
                else
                {
                    throw new InvalidOperationException($"Y-axis visible range is invalid: [{yRange.Min}, {yRange.Max}]");
                }
            }

            var theme = model.Theme;
            var m = model.PlotMargins;
            var left = (float)m.Left;
            var top = (float)m.Top;
            var right = (float)RenderingHelper.CalculateEffectiveRightMargin(m.Right, model.YAxisSecondary != null);
            var bottom = (float)m.Bottom;
            var plotW = Math.Max(0, pixelWidth - (left + right));
            var plotH = Math.Max(0, pixelHeight - (top + bottom));
            var plotRect = new SKRect(left, top, left + plotW, top + plotH);
            canvas.Clear(new SKColor(theme.SurfaceBackgroundColor.R, theme.SurfaceBackgroundColor.G, theme.SurfaceBackgroundColor.B, theme.SurfaceBackgroundColor.A));

            const double tolerance = 1e-10;
            var needsAdjustment = (Math.Abs(xRange.Min - originalXRange.Min) > tolerance || Math.Abs(xRange.Max - originalXRange.Max) > tolerance) ||
                                  (Math.Abs(yRange.Min - originalYRange.Min) > tolerance || Math.Abs(yRange.Max - originalYRange.Max) > tolerance);
            if (needsAdjustment)
            {
                model.XAxis.VisibleRange = xRange;
                model.YAxis.VisibleRange = yRange;
            }

            model.UpdateScales((int)plotW, (int)plotH);

            using (var bg = new SKPaint { Style = SKPaintStyle.Fill, Color = new SKColor(theme.PlotBackgroundColor.R, theme.PlotBackgroundColor.G, theme.PlotBackgroundColor.B, theme.PlotBackgroundColor.A) })
            {
                canvas.DrawRect(plotRect, bg);
            }
            using var paints = SkiaPaintPack.Create(theme);
            var ctx = new RenderContext(model, canvas, plotRect, paints, pixelWidth, pixelHeight);
            _grid.Render(ctx);
            _series.Render(ctx);
            _annotations.Render(ctx);
            _axesTicks.Render(ctx);
            _legend.Render(ctx);
            RenderOverlay(ctx);

            if (needsAdjustment)
            {
                model.XAxis.VisibleRange = originalXRange;
                model.YAxis.VisibleRange = originalYRange;
            }
        }

        public SKBitmap RenderToBitmap(ChartModel model, int pixelWidth, int pixelHeight, bool transparentBackground = false)
        {
            var info = new SKImageInfo(pixelWidth, pixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
            var bmp = new SKBitmap(info);
            using (var canvas = new SKCanvas(bmp))
            {
                if (transparentBackground)
                {
                    canvas.Clear(SKColors.Transparent);
                }
                Render(model, canvas, pixelWidth, pixelHeight);
                canvas.Flush();
            }
            return bmp;
        }

        public void ExportPng(ChartModel model, Stream destination, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (destination == null || !destination.CanWrite)
            {
                return;
            }
            using var bmp = RenderToBitmap(model, pixelWidth, pixelHeight, transparentBackground);
            using var img = SKImage.FromPixels(bmp.PeekPixels());
            using var data = img.Encode(SKEncodedImageFormat.Png, quality);
            data.SaveTo(destination);
            destination.Flush();
        }

        public async Task ExportPngAsync(ChartModel model, Stream destination, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false, CancellationToken cancellationToken = default)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (!destination.CanWrite)
            {
                throw new ArgumentException("Destination stream must be writable", nameof(destination));
            }
            cancellationToken.ThrowIfCancellationRequested();
            var imageData = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var bmp = RenderToBitmap(model, pixelWidth, pixelHeight, transparentBackground);
                using var img = SKImage.FromPixels(bmp.PeekPixels());
                return img.Encode(SKEncodedImageFormat.Png, quality);
            }, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            using (imageData)
            {
                using var stream = imageData.AsStream();
#if NET8_0_OR_GREATER
                await stream.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
                await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
#else
                cancellationToken.ThrowIfCancellationRequested();
                await stream.CopyToAsync(destination).ConfigureAwait(false);
                await destination.FlushAsync().ConfigureAwait(false);
#endif
            }
        }

        public async Task<SKBitmap> RenderToBitmapAsync(ChartModel model, int pixelWidth, int pixelHeight, bool transparentBackground = false, CancellationToken cancellationToken = default)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return RenderToBitmap(model, pixelWidth, pixelHeight, transparentBackground);
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task RenderAsync(ChartModel model, SKCanvas canvas, int pixelWidth, int pixelHeight, CancellationToken cancellationToken = default)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Render(model, canvas, pixelWidth, pixelHeight);
            }, cancellationToken).ConfigureAwait(false);
        }

        private static void RenderOverlay(RenderContext ctx)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var st = model.InteractionState;
            if (st == null)
            {
                return;
            }
            var tooltipSeries = st.TooltipSeries;
            if (tooltipSeries == null)
            {
                return;
            }
            if (st.ShowSelectionRect)
            {
                var x1 = (float)st.SelX1;
                var y1 = (float)st.SelY1;
                var x2 = (float)st.SelX2;
                var y2 = (float)st.SelY2;
                using var selFill = new SKPaint { Color = new SKColor(30, 120, 220, 40), Style = SKPaintStyle.Fill, IsAntialias = true };
                using var selStroke = new SKPaint { Color = new SKColor(30, 120, 220, 160), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
                var rr = SKRect.Create(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                ctx.Canvas.Save();
                ctx.Canvas.ClipRect(pr);
                ctx.Canvas.DrawRect(rr, selFill);
                ctx.Canvas.DrawRect(rr, selStroke);
                ctx.Canvas.Restore();
            }
            if (st.ShowNearest)
            {
                var px = PixelMapper.X(st.NearestDataX, ctx.Model.XAxis, pr);
                var py = PixelMapper.Y(st.NearestDataY, ctx.Model.YAxis, pr);
                using var npStroke = new SKPaint { Color = new SKColor(255, 80, 80, 220), Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
                using var npFill = new SKPaint { Color = new SKColor(255, 80, 80, 120), Style = SKPaintStyle.Fill, IsAntialias = true };
                ctx.Canvas.Save();
                ctx.Canvas.ClipRect(pr);
                ctx.Canvas.DrawCircle(px, py, 6, npFill);
                ctx.Canvas.DrawCircle(px, py, 6, npStroke);
                ctx.Canvas.Restore();
            }
            if (!st.ShowCrosshair)
            {
                return;
            }
            var cx = (float)st.PixelX;
            var cy = (float)st.PixelY;
            if (cx < pr.Left)
            {
                cx = pr.Left;
            }
            else if (cx > pr.Right)
            {
                cx = pr.Right;
            }
            if (cy < pr.Top)
            {
                cy = pr.Top;
            }
            else if (cy > pr.Bottom)
            {
                cy = pr.Bottom;
            }
            using var cross = new SKPaint { Color = new SKColor(0, 0, 0, 80), Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
            using var tipBg = new SKPaint { Color = new SKColor(255, 255, 255, 230), Style = SKPaintStyle.Fill, IsAntialias = true };
            using var tipBd = new SKPaint { Color = new SKColor(0, 0, 0, 120), Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
            using var tipTxPaint = new SKPaint { Color = new SKColor(30, 30, 30, 255), Style = SKPaintStyle.Fill, IsAntialias = true };
            using var tipFont = new SKFont(null, (float)model.Theme.LabelTextSize);
            var palette = model.Theme.SeriesPalette;
            ctx.Canvas.Save();
            ctx.Canvas.ClipRect(pr);
            ctx.Canvas.DrawLine(pr.Left, cy, pr.Right, cy, cross);
            ctx.Canvas.DrawLine(cx, pr.Top, cx, pr.Bottom, cross);
            ctx.Canvas.Restore();
            string[] lines;
            if (tooltipSeries.Count > 0)
            {
                var header = st.TooltipText?.Split('\n').FirstOrDefault();
                var body = tooltipSeries.Select(v => v.Title + ": " + v.Y);
                lines = (header != null ? new[] { header }.Concat(body) : body).ToArray();
            }
            else if (!string.IsNullOrEmpty(st.TooltipText))
            {
                lines = (st.TooltipText ?? string.Empty).Split('\n');
            }
            else
            {
                return;
            }
            var maxW = 0f;
            foreach (var l in lines)
            {
                var w = tipFont.MeasureText(l, tipTxPaint);
                if (w > maxW)
                {
                    maxW = w;
                }
            }
            var lineH = tipFont.Size + 2;
            var pad = 6f;
            var showSwatches = tooltipSeries.Count > 0;
            var swatchSize = showSwatches ? tipFont.Size * 0.6f : 0f;
            var swatchGap = showSwatches ? 4f : 0f;
            var extra = showSwatches ? (swatchSize + swatchGap) : 0f;
            var boxW = maxW + pad * 2 + extra;
            var boxH = lineH * lines.Length + pad * 2;
            var bx = cx + 12;
            var by = cy - boxH - 12;
            if (bx + boxW > pr.Right)
            {
                bx = pr.Right - boxW - 1;
            }
            if (by < pr.Top)
            {
                by = cy + 12;
            }
            var rect = new SKRect(bx, by, bx + boxW, by + boxH);
            ctx.Canvas.DrawRect(rect, tipBg);
            ctx.Canvas.DrawRect(rect, tipBd);
            var ty = by + pad + tipFont.Size;
            tipFont.GetFontMetrics(out var metrics);
            var textHeight = metrics.Descent - metrics.Ascent;
            for (var i = 0; i < lines.Length; i++)
            {
                var tx = bx + pad;
                if (showSwatches && i > 0 && i - 1 < tooltipSeries.Count)
                {
                    var sv = tooltipSeries[i - 1];
                    var col = SeriesColorResolver.ResolveSeriesColor(model, sv, palette);
                    using var sw = new SKPaint { Color = new SKColor(col.R, col.G, col.B, col.A), Style = SKPaintStyle.Fill, IsAntialias = true };
                    var topText = ty + metrics.Ascent;
                    var verticalPadding = (textHeight - swatchSize) * 0.5f;
                    var swTop = topText + verticalPadding;
                    var swRect = new SKRect(tx, swTop, tx + swatchSize, swTop + swatchSize);
                    ctx.Canvas.DrawRect(swRect, sw);
                    tx += swatchSize + swatchGap;
                }
                ctx.Canvas.DrawText(lines[i], tx, ty, SKTextAlign.Left, tipFont, tipTxPaint);
                ty += lineH;
            }
        }
    }
}
