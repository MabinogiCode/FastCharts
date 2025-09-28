using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Rendering.Skia.Rendering; // self

using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    internal sealed class RenderContext
    {
        public ChartModel Model { get; }
        public SKCanvas Canvas { get; }
        public SKRect PlotRect { get; }
        public SkiaPaintPack Paints { get; }
        public int PixelWidth { get; }
        public int PixelHeight { get; }

        public RenderContext(ChartModel model, SKCanvas canvas, SKRect plotRect, SkiaPaintPack paints, int w, int h)
        {
            Model = model; Canvas = canvas; PlotRect = plotRect; Paints = paints; PixelWidth = w; PixelHeight = h;
        }
    }
}
