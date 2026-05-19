using System.Collections.Generic;

using FastCharts.Rendering.Skia.Rendering;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class SeriesLayer : IRenderLayer
    {
        // Ordered sublayers (back-to-front) to preserve expected overlap semantics
        private readonly ISeriesSubLayer[] _layers =
        {
            new AreaBandLayer(),   // fills first
            new BarLayer(),        // grouped bars
            new StackedBarLayer(), // stacked bars
            new OhlcLayer(),       // candles
            new ErrorBarLayer(),   // error bars overlay
            new ScatterLayer(),    // markers
            new StepLineLayer(),   // step lines
            new LineLayer()        // plain lines on top
        };

        private readonly IReadOnlyList<ISeriesRenderer> _customRenderers;

        public SeriesLayer(IReadOnlyList<ISeriesRenderer> customRenderers)
        {
            _customRenderers = customRenderers;
        }

        public void Render(RenderContext ctx)
        {
            foreach (var l in _layers)
            {
                l.Render(ctx);
            }

            // Custom series renderers draw after (on top of) the built-in layers.
            for (var i = 0; i < _customRenderers.Count; i++)
            {
                _customRenderers[i].Render(ctx);
            }
        }
    }
}
