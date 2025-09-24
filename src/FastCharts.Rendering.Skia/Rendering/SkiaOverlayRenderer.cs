using FastCharts.Core;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal static class SkiaOverlayRenderer
    {
        public static void Render(ChartModel model, SKCanvas canvas, SKRect plotRect)
        {
            var st = model.InteractionState;
            if (st == null || !st.ShowCrosshair)
            {
                return;
            }

            float cx = (float)st.PixelX;
            float cy = (float)st.PixelY;
            if (cx < plotRect.Left)
            {
                cx = plotRect.Left;
            }
            else if (cx > plotRect.Right)
            {
                cx = plotRect.Right;
            }
            if (cy < plotRect.Top)
            {
                cy = plotRect.Top;
            }
            else if (cy > plotRect.Bottom)
            {
                cy = plotRect.Bottom;
            }

            using var cross = new SKPaint { Color = new SKColor(0, 0, 0, 80), IsAntialias = false, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
            using var tipBg = new SKPaint { Color = new SKColor(255, 255, 255, 230), IsAntialias = true, Style = SKPaintStyle.Fill };
            using var tipBd = new SKPaint { Color = new SKColor(0, 0, 0, 120), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
            using var tipTx = new SKPaint { Color = new SKColor(30, 30, 30, 255), IsAntialias = true, TextSize = (float)model.Theme.LabelTextSize };

            canvas.Save();
            canvas.ClipRect(plotRect);
            canvas.DrawLine(plotRect.Left, cy, plotRect.Right, cy, cross);
            canvas.DrawLine(cx, plotRect.Top, cx, plotRect.Bottom, cross);
            canvas.Restore();

            if (string.IsNullOrEmpty(st.TooltipText))
            {
                return;
            }
            var lines = st.TooltipText.Split('\n');
            if (lines.Length == 0)
            {
                return;
            }
            float maxW = 0;
            foreach (var line in lines)
            {
                float w = tipTx.MeasureText(line);
                if (w > maxW)
                {
                    maxW = w;
                }
            }
            float lineH = tipTx.TextSize + 2;
            float pad = 6;
            float boxW = maxW + pad * 2;
            float boxH = lineH * lines.Length + pad * 2;
            float bx = cx + 12;
            float by = cy - boxH - 12;
            if (bx + boxW > plotRect.Right)
            {
                bx = plotRect.Right - boxW - 1;
            }
            if (by < plotRect.Top)
            {
                by = cy + 12;
            }
            var rect = new SKRect(bx, by, bx + boxW, by + boxH);
            canvas.DrawRect(rect, tipBg);
            canvas.DrawRect(rect, tipBd);
            float tx = bx + pad;
            float ty = by + pad + tipTx.TextSize;
            foreach (var line in lines)
            {
                canvas.DrawText(line, tx, ty, tipTx);
                ty += lineH;
            }
        }
    }
}
