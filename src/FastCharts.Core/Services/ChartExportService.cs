using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FastCharts.Core.Abstractions;

namespace FastCharts.Core.Services
{
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
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

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
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

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

        /// <summary>
        /// Exports the chart to an SVG file. Requires the configured exporter to support
        /// vector output (<see cref="ISvgChartExporter"/>, e.g. SkiaChartRenderer).
        /// </summary>
        public void ExportToSvgFile(ChartModel model, string filePath, int pixelWidth, int pixelHeight)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            GetSvgExporter().ExportSvg(model, fileStream, pixelWidth, pixelHeight);
        }

        /// <summary>
        /// Exports the chart as an SVG document string.
        /// </summary>
        public string ExportToSvgString(ChartModel model, int pixelWidth, int pixelHeight)
        {
            using var memoryStream = new MemoryStream();
            GetSvgExporter().ExportSvg(model, memoryStream, pixelWidth, pixelHeight);
            return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        private ISvgChartExporter GetSvgExporter()
        {
            if (_exporter is ISvgChartExporter svgExporter)
            {
                return svgExporter;
            }

            throw new NotSupportedException($"The configured exporter ({_exporter.GetType().Name}) does not support SVG export. Use an exporter implementing {nameof(ISvgChartExporter)}, such as SkiaChartRenderer.");
        }
    }
}