using System;
using FastCharts.Core.Abstractions;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal sealed class SkiaPaintPack : IDisposable
    {
        public SKPaint Axis { get; }
        public SKPaint Tick { get; }
        public SKPaint Text { get; }
        public SKFont TextFont { get; }
        public SKPaint Grid { get; }
        public SKPaint Border { get; }

        private SkiaPaintPack(SKPaint axis, SKPaint tick, SKPaint text, SKFont textFont, SKPaint grid, SKPaint border)
        {
            Axis = axis; Tick = tick; Text = text; TextFont = textFont; Grid = grid; Border = border;
        }

        public static SkiaPaintPack Create(ITheme theme)
        {
            var axisColor = new SKColor(theme.AxisColor.R, theme.AxisColor.G, theme.AxisColor.B, theme.AxisColor.A);
            var gridColor = new SKColor(theme.GridColor.R, theme.GridColor.G, theme.GridColor.B, theme.GridColor.A);
            var labelColor = new SKColor(theme.LabelColor.R, theme.LabelColor.G, theme.LabelColor.B, theme.LabelColor.A);
            var axis = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = axisColor, StrokeWidth = (float)theme.AxisThickness };
            var tick = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = axisColor, StrokeWidth = (float)theme.AxisThickness };
            var text = new SKPaint { IsAntialias = true, Color = labelColor };
            var textFont = new SKFont(null, (float)theme.LabelTextSize, 1f, 0f) { Edging = SKFontEdging.SubpixelAntialias }; // keep smoothing
            var grid = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Stroke, Color = gridColor, StrokeWidth = (float)theme.GridThickness };
            var border = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = axisColor, StrokeWidth = 1 };
            return new SkiaPaintPack(axis, tick, text, textFont, grid, border);
        }

        public void Dispose()
        {
            Axis.Dispose(); Tick.Dispose(); Text.Dispose(); Grid.Dispose(); Border.Dispose(); TextFont.Dispose();
        }
    }
}
