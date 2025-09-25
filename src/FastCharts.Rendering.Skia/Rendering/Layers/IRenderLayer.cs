using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal interface IRenderLayer
    {
        void Render(RenderContext ctx);
    }
}
