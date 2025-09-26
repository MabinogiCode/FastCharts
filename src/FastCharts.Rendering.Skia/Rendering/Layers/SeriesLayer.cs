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

        public void Render(RenderContext ctx)
        {
            foreach (var l in _layers)
            {
                l.Render(ctx);
            }
        }
    }
}
