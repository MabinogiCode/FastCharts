using System;
using System.Linq;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Annotations;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    /// <summary>
    /// Rendering layer for chart annotations (P1-ANN-LINE)
    /// Renders after series but before overlay elements
    /// </summary>
    internal sealed class AnnotationLayer : IRenderLayer
    {
        public void Render(RenderContext ctx)
        {
            var model = ctx.Model;
            
            if (!model.Annotations.Any())
            {
                return;
            }

            // Sort annotations by Z-index for proper layering
            var visibleAnnotations = model.Annotations
                .Where(a => a.IsVisible)
                .OrderBy(a => a.ZIndex)
                .ToList();

            foreach (var annotation in visibleAnnotations)
            {
                switch (annotation)
                {
                    case AnnotationLine line:
                        RenderAnnotationLine(ctx, line);
                        break;
                    // Future annotation types will be added here
                    default:
                        // Unknown annotation type - skip silently
                        break;
                }
            }
        }

        private static void RenderAnnotationLine(RenderContext ctx, AnnotationLine line)
        {
            var pr = ctx.PlotRect;
            var canvas = ctx.Canvas;

            // Create paint for the line
            using var linePaint = CreateLinePaint(line);

            canvas.Save();
            canvas.ClipRect(pr);

            if (line.Orientation == AnnotationOrientation.Horizontal)
            {
                RenderHorizontalLine(ctx, line, linePaint);
            }
            else
            {
                RenderVerticalLine(ctx, line, linePaint);
            }

            canvas.Restore();
        }

        private static void RenderHorizontalLine(RenderContext ctx, AnnotationLine line, SKPaint linePaint)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var canvas = ctx.Canvas;

            // Map Y-value to pixel coordinates
            var yPixel = PixelMapper.Y(line.Value, model.YAxis, pr);

            // Check if line is within visible area
            if (yPixel < pr.Top || yPixel > pr.Bottom)
            {
                return;
            }

            // Draw the horizontal line
            canvas.DrawLine(pr.Left, yPixel, pr.Right, yPixel, linePaint);

            // Draw label if enabled
            if (line.ShowLabel && !string.IsNullOrWhiteSpace(line.Title))
            {
                RenderHorizontalLineLabel(ctx, line, yPixel);
            }
        }

        private static void RenderVerticalLine(RenderContext ctx, AnnotationLine line, SKPaint linePaint)
        {
            var model = ctx.Model;
            var pr = ctx.PlotRect;
            var canvas = ctx.Canvas;

            // Map X-value to pixel coordinates
            var xPixel = PixelMapper.X(line.Value, model.XAxis, pr);

            // Check if line is within visible area
            if (xPixel < pr.Left || xPixel > pr.Right)
            {
                return;
            }

            // Draw the vertical line
            canvas.DrawLine(xPixel, pr.Top, xPixel, pr.Bottom, linePaint);

            // Draw label if enabled
            if (line.ShowLabel && !string.IsNullOrWhiteSpace(line.Title))
            {
                RenderVerticalLineLabel(ctx, line, xPixel);
            }
        }

        private static void RenderHorizontalLineLabel(RenderContext ctx, AnnotationLine line, float yPixel)
        {
            var pr = ctx.PlotRect;
            var canvas = ctx.Canvas;
            var font = ctx.Paints.TextFont;

            using var textPaint = new SKPaint
            {
                Color = new SKColor(line.Color.R, line.Color.G, line.Color.B, line.Color.A),
                IsAntialias = true
            };

            using var backgroundPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 200), // Semi-transparent white background
                IsAntialias = true
            };

            var text = line.Title!;
            var textWidth = font.MeasureText(text, textPaint);
            var textHeight = font.Size;

            // Calculate label position based on LabelPosition setting
            float labelX = line.LabelPosition switch
            {
                LabelPosition.Start => pr.Left + 5,
                LabelPosition.Middle => pr.Left + (pr.Width - textWidth) / 2,
                LabelPosition.End => pr.Right - textWidth - 5,
                _ => pr.Right - textWidth - 5
            };

            var labelY = yPixel - 5; // Position above the line

            // Ensure label stays within plot bounds
            labelX = Math.Max(pr.Left + 2, Math.Min(pr.Right - textWidth - 2, labelX));

            // Draw background rectangle
            var bgRect = new SKRect(labelX - 2, labelY - textHeight, labelX + textWidth + 2, labelY + 2);
            canvas.DrawRect(bgRect, backgroundPaint);

            // Draw text
            canvas.DrawText(text, labelX, labelY - 2, SKTextAlign.Left, font, textPaint);
        }

        private static void RenderVerticalLineLabel(RenderContext ctx, AnnotationLine line, float xPixel)
        {
            var pr = ctx.PlotRect;
            var canvas = ctx.Canvas;
            var font = ctx.Paints.TextFont;

            using var textPaint = new SKPaint
            {
                Color = new SKColor(line.Color.R, line.Color.G, line.Color.B, line.Color.A),
                IsAntialias = true
            };

            using var backgroundPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 200), // Semi-transparent white background
                IsAntialias = true
            };

            var text = line.Title!;
            var textWidth = font.MeasureText(text, textPaint);
            var textHeight = font.Size;

            // Calculate label position based on LabelPosition setting
            float labelY = line.LabelPosition switch
            {
                LabelPosition.Start => pr.Top + textHeight + 5,
                LabelPosition.Middle => pr.Top + (pr.Height + textHeight) / 2,
                LabelPosition.End => pr.Bottom - 5,
                _ => pr.Bottom - 5
            };

            var labelX = xPixel + 5; // Position to the right of the line

            // Ensure label stays within plot bounds
            labelY = Math.Max(pr.Top + textHeight + 2, Math.Min(pr.Bottom - 2, labelY));
            labelX = Math.Min(pr.Right - textWidth - 2, labelX);

            // Draw background rectangle
            var bgRect = new SKRect(labelX - 2, labelY - textHeight, labelX + textWidth + 2, labelY + 2);
            canvas.DrawRect(bgRect, backgroundPaint);

            // Draw text
            canvas.DrawText(text, labelX, labelY - 2, SKTextAlign.Left, font, textPaint);
        }

        private static SKPaint CreateLinePaint(AnnotationLine line)
        {
            var paint = new SKPaint
            {
                Color = new SKColor(line.Color.R, line.Color.G, line.Color.B, line.Color.A),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (float)line.Thickness,
                IsAntialias = true
            };

            // Apply line style
            switch (line.LineStyle)
            {
                case LineStyle.Dashed:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 8, 4 }, 0);
                    break;
                case LineStyle.Dotted:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0);
                    break;
                case LineStyle.DashDot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 8, 4, 2, 4 }, 0);
                    break;
                case LineStyle.Solid:
                default:
                    // No path effect for solid lines
                    break;
            }

            return paint;
        }
    }
}