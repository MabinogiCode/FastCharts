namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Renders a ChartModel into a backend-specific surface.
    /// The surface type is generic to avoid coupling Core to any graphics library.
    /// </summary>
    public interface IRenderer<in TSurface>
    {
        void Render(ChartModel model, TSurface surface, int pixelWidth, int pixelHeight);
    }
}
