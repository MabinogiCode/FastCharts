using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal sealed class SeriesLayer : IRenderLayer
    {
        private readonly ISeriesSubLayer[] _layers = new ISeriesSubLayer[]
        {
            new AreaBandLayer(),
            new BarLayer(),
            new StackedBarLayer(),
            new OhlcLayer(),
            new ErrorBarLayer(),
            new ScatterLayer(),
            new StepLineLayer(),
            new LineLayer()
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
