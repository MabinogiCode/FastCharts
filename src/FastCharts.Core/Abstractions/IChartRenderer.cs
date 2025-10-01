using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// ? NEW: Interface for chart renderers to enable dependency injection and extensibility
    /// Follows Microsoft .NET guidelines for async API design
    /// </summary>
    /// <typeparam name="TSurface">The type of rendering surface (e.g., SKCanvas, Graphics, etc.)</typeparam>
    public interface IChartRenderer<in TSurface>
    {
        /// <summary>
        /// Render the chart model onto a provided surface of given pixel size.
        /// </summary>
        /// <param name="model">The chart model to render</param>
        /// <param name="surface">The rendering surface</param>
        /// <param name="pixelWidth">Width in pixels</param>
        /// <param name="pixelHeight">Height in pixels</param>
        void Render(ChartModel model, TSurface surface, int pixelWidth, int pixelHeight);
    }

    /// <summary>
    /// ? NEW: Extended interface for renderers that support asynchronous operations
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

    /// <summary>
    /// ? NEW: Interface for renderers that support export operations
    /// </summary>
    public interface IChartExporter
    {
        /// <summary>
        /// Export chart as PNG to a stream.
        /// </summary>
        void ExportPng(ChartModel model, Stream destination, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false);

        /// <summary>
        /// Asynchronously export chart as PNG to a stream.
        /// </summary>
        Task ExportPngAsync(ChartModel model, Stream destination, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false, CancellationToken cancellationToken = default);
    }
}