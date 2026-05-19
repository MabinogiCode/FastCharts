namespace FastCharts.Rendering.Skia.Rendering
{
    /// <summary>
    /// Extension point for drawing a series type that the built-in render layers
    /// do not handle. Register an implementation with
    /// <see cref="SkiaChartRenderer.CustomSeriesRenderers"/> to support custom
    /// series without modifying the rendering assembly.
    /// </summary>
    /// <remarks>
    /// Implementations iterate <see cref="RenderContext.Model"/>'s series, select
    /// the ones they recognise and draw them onto <see cref="RenderContext.Canvas"/>.
    /// Custom renderers run after the built-in layers, so they draw on top.
    /// </remarks>
    public interface ISeriesRenderer
    {
        /// <summary>
        /// Draws the supported custom series for the current frame.
        /// </summary>
        /// <param name="context">The current render context.</param>
        void Render(RenderContext context);
    }
}
