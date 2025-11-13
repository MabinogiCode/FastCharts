using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Interface for exporting charts to various formats
    /// </summary>
    public interface IChartExporter
    {
        /// <summary>
        /// Export chart to PNG format
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="destination">Stream to write the PNG data to</param>
        /// <param name="pixelWidth">Width in pixels</param>
        /// <param name="pixelHeight">Height in pixels</param>
        /// <param name="quality">PNG quality (0-100)</param>
        /// <param name="transparentBackground">Whether to use transparent background</param>
        void ExportPng(ChartModel model, Stream destination, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false);

        /// <summary>
        /// Export chart to PNG format asynchronously
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="destination">Stream to write the PNG data to</param>
        /// <param name="pixelWidth">Width in pixels</param>
        /// <param name="pixelHeight">Height in pixels</param>
        /// <param name="quality">PNG quality (0-100)</param>
        /// <param name="transparentBackground">Whether to use transparent background</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExportPngAsync(ChartModel model, Stream destination, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false, CancellationToken cancellationToken = default);
    }
}