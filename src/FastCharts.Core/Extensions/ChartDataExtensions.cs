using System.Collections.Generic;
using FastCharts.Core.Series;

namespace FastCharts.Core.Extensions
{
    /// <summary>
    /// KISS helpers: turn plain .NET data into chart series without ceremony.
    /// </summary>
    public static class ChartDataExtensions
    {
        /// <summary>
        /// Turns X/Y pairs (e.g. a <c>Dictionary&lt;double, double&gt;</c>) into a line series.
        /// Points are sorted by X automatically.
        /// </summary>
        /// <param name="data">X/Y pairs (key = X, value = Y)</param>
        /// <param name="title">Optional legend title</param>
        /// <returns>A ready-to-add line series</returns>
        public static LineSeries ToLineSeries(this IEnumerable<KeyValuePair<double, double>> data, string? title = null)
        {
            return new LineSeries(data) { Title = title };
        }

        /// <summary>
        /// Turns Y values into a line series, X being the 0-based index.
        /// </summary>
        /// <param name="values">Y values</param>
        /// <param name="title">Optional legend title</param>
        /// <returns>A ready-to-add line series</returns>
        public static LineSeries ToLineSeries(this IEnumerable<double> values, string? title = null)
        {
            return new LineSeries(values) { Title = title };
        }
    }
}
