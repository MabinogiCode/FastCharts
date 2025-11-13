using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FastCharts.Core;
using FastCharts.Rendering.Skia;

namespace FastCharts.Wpf.Extensions
{
    /// <summary>
    /// Extension methods for exporting charts to various formats.
    /// </summary>
    public static class ChartExportExtensions
    {
        /// <summary>
        /// Exports the chart to PNG format.
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="quality">PNG quality (0-100)</param>
        /// <param name="transparentBackground">Use transparent background</param>
        public static void ExportToPng(this ChartModel model, string filePath, int width, int height, int quality = 100, bool transparentBackground = false)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive");
            }

            var renderer = new SkiaChartRenderer();
            using var stream = File.Create(filePath);
            renderer.ExportPng(model, stream, width, height, quality, transparentBackground);
        }

        /// <summary>
        /// Exports the chart to PNG format asynchronously.
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="quality">PNG quality (0-100)</param>
        /// <param name="transparentBackground">Use transparent background</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task ExportToPngAsync(this ChartModel model, string filePath, int width, int height, int quality = 100, bool transparentBackground = false, CancellationToken cancellationToken = default)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive");
            }

            var renderer = new SkiaChartRenderer();
            using var stream = File.Create(filePath);
            await renderer.ExportPngAsync(model, stream, width, height, quality, transparentBackground, cancellationToken);
        }

        /// <summary>
        /// Exports the chart to PNG format into a stream.
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="destination">Output stream</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="quality">PNG quality (0-100)</param>
        /// <param name="transparentBackground">Use transparent background</param>
        public static void ExportToPng(this ChartModel model, Stream destination, int width, int height, int quality = 100, bool transparentBackground = false)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive");
            }

            var renderer = new SkiaChartRenderer();
            renderer.ExportPng(model, destination, width, height, quality, transparentBackground);
        }
    }
}