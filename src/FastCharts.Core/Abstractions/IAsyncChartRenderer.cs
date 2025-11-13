using System.Threading;
using System.Threading.Tasks;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Extended interface for renderers that support asynchronous operations
    /// </summary>
    /// <typeparam name="TSurface">The type of rendering surface</typeparam>
    public interface IAsyncChartRenderer<in TSurface> : IChartRenderer<TSurface>
    {
        /// <summary>
        /// Asynchronously render the chart model onto a provided surface.
        /// </summary>
        /// <param name="model">The chart model to render</param>
        /// <param name="surface">The rendering surface</param>
        /// <param name="pixelWidth">Width in pixels</param>
        /// <param name="pixelHeight">Height in pixels</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RenderAsync(ChartModel model, TSurface surface, int pixelWidth, int pixelHeight, CancellationToken cancellationToken = default);
    }
}