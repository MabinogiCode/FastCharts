using System.Collections.Generic;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Rendering.Skia.Helpers
{
    /// <summary>
    /// Helper class for resolving series colors from chart models and palettes.
    /// </summary>
    internal static class SeriesColorResolver
    {
        /// <summary>
        /// Resolves the color for a series based on chart model, series reference, and palette.
        /// Allocation-free: computes the per-type index in a single pass over the series list.
        /// </summary>
        /// <param name="model">The chart model containing series and theme.</param>
        /// <param name="seriesRef">The series reference to resolve color for.</param>
        /// <param name="palette">The color palette to use.</param>
        /// <returns>The resolved color for the series.</returns>
        public static ColorRgba ResolveSeriesColor(ChartModel model, object seriesRef, IReadOnlyList<ColorRgba> palette)
        {
            var primary = model.Theme.PrimarySeriesColor;
            if (palette == null || palette.Count == 0)
            {
                return primary;
            }

            if (seriesRef is not SeriesBase series)
            {
                return primary;
            }

            var idx = series.PaletteIndex ?? IndexAmongKind(model, series);
            return (idx >= 0 && idx < palette.Count) ? palette[idx] : primary;
        }

        /// <summary>
        /// Returns the position of the series among the series of the same color-group
        /// (BandSeries, ScatterSeries, or LineSeries family), matching legacy palette assignment.
        /// </summary>
        private static int IndexAmongKind(ChartModel model, SeriesBase series)
        {
            var count = 0;
            var seriesList = model.Series;

            for (var i = 0; i < seriesList.Count; i++)
            {
                var candidate = seriesList[i];
                var sameKind = series switch
                {
                    BandSeries => candidate is BandSeries,
                    ScatterSeries => candidate is ScatterSeries,
                    LineSeries => candidate is LineSeries,
                    _ => false
                };

                if (!sameKind)
                {
                    continue;
                }

                if (ReferenceEquals(candidate, series))
                {
                    return count;
                }

                count++;
            }

            return -1;
        }
    }
}
