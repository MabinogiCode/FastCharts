using System.IO;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Optional capability for exporters that support vector (SVG) output.
    /// Kept separate from <see cref="IChartExporter"/> so existing implementations stay valid.
    /// </summary>
    public interface ISvgChartExporter
    {
        /// <summary>
        /// Export chart to SVG (scalable vector graphics)
        /// </summary>
        /// <param name="model">Chart model to export</param>
        /// <param name="destination">Stream to write the SVG document to</param>
        /// <param name="pixelWidth">Nominal width in pixels (SVG viewBox)</param>
        /// <param name="pixelHeight">Nominal height in pixels (SVG viewBox)</param>
        void ExportSvg(ChartModel model, Stream destination, int pixelWidth, int pixelHeight);
    }
}
