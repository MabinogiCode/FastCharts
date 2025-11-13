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

    /// <summary>
    /// Default implementation of chart export service
    /// </summary>
    public class ChartExportService : IChartExportService
    {
        private readonly IChartExporter _exporter;

        public ChartExportService(IChartExporter exporter)
        {
            _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
        }

        public void ExportToPngFile(ChartModel model, string filePath, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            _exporter.ExportPng(model, fileStream, pixelWidth, pixelHeight, quality, transparentBackground);
        }

        public async Task ExportToPngFileAsync(ChartModel model, string filePath, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await _exporter.ExportPngAsync(model, fileStream, pixelWidth, pixelHeight, quality, transparentBackground, cancellationToken).ConfigureAwait(false);
        }

        public byte[] ExportToPngBytes(ChartModel model, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false)
        {
            using var memoryStream = new MemoryStream();
            _exporter.ExportPng(model, memoryStream, pixelWidth, pixelHeight, quality, transparentBackground);
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExportToPngBytesAsync(ChartModel model, int pixelWidth, int pixelHeight, int quality = 100, bool transparentBackground = false, CancellationToken cancellationToken = default)
        {
            using var memoryStream = new MemoryStream();
            await _exporter.ExportPngAsync(model, memoryStream, pixelWidth, pixelHeight, quality, transparentBackground, cancellationToken).ConfigureAwait(false);
            return memoryStream.ToArray();
        }
    }
}