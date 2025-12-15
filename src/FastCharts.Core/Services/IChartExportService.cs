using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FastCharts.Core.Abstractions;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Chart export service that provides convenient methods for exporting charts
    /// </summary>
    public interface IChartExportService
    {
        /// <summary>
        /// Export chart to PNG file
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="filePath">File path to save PNG</param>
        /// <param name="pixelWidth">Width in pixels</param>
        /// <param name="pixelHeight">Height in pixels</param>
        /// <param name="quality">PNG quality (0-100)</param>
        /// <param name="transparentBackground">Whether to use transparent background</param>
        void ExportToPngFile(ChartModel model, string filePath, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false);

        /// <summary>
        /// Export chart to PNG file asynchronously
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="filePath">File path to save PNG</param>
        /// <param name="pixelWidth">Width in pixels</param>
        /// <param name="pixelHeight">Height in pixels</param>
        /// <param name="quality">PNG quality (0-100)</param>
        /// <param name="transparentBackground">Whether to use transparent background</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExportToPngFileAsync(ChartModel model, string filePath, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Export chart to PNG byte array
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="pixelWidth">Width in pixels</param>
        /// <param name="pixelHeight">Height in pixels</param>
        /// <param name="quality">PNG quality (0-100)</param>
        /// <param name="transparentBackground">Whether to use transparent background</param>
        /// <returns>PNG data as byte array</returns>
        byte[] ExportToPngBytes(ChartModel model, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false);

        /// <summary>
        /// Export chart to PNG byte array asynchronously
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="pixelWidth">Width in pixels</param>
        /// <param name="pixelHeight">Height in pixels</param>
        /// <param name="quality">PNG quality (0-100)</param>
        /// <param name="transparentBackground">Whether to use transparent background</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PNG data as byte array</returns>
        Task<byte[]> ExportToPngBytesAsync(ChartModel model, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false, CancellationToken cancellationToken = default);
    }
}