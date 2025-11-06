using System.Collections.Generic;
using System.Linq;
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

            if (seriesRef is BandSeries band)
            {
                var idx = model.Series.OfType<BandSeries>().ToList().IndexOf(band);
                if (band.PaletteIndex.HasValue)
                {
                    idx = band.PaletteIndex.Value;
                }
                return (idx >= 0 && idx < palette.Count) ? palette[idx] : primary;
            }

            if (seriesRef is ScatterSeries sc)
            {
                var idx = model.Series.OfType<ScatterSeries>().ToList().IndexOf(sc);
                if (sc.PaletteIndex.HasValue)
                {
                    idx = sc.PaletteIndex.Value;
                }
                return (idx >= 0 && idx < palette.Count) ? palette[idx] : primary;
            }

            if (seriesRef is AreaSeries area)
            {
                var idx = model.Series.OfType<LineSeries>().ToList().IndexOf(area);
                if (area.PaletteIndex.HasValue)
                {
                    idx = area.PaletteIndex.Value;
                }
                return (idx >= 0 && idx < palette.Count) ? palette[idx] : primary;
            }

            if (seriesRef is LineSeries ls)
            {
                var idx = model.Series.OfType<LineSeries>().ToList().IndexOf(ls);
                if (ls.PaletteIndex.HasValue)
                {
                    idx = ls.PaletteIndex.Value;
                }
                return (idx >= 0 && idx < palette.Count) ? palette[idx] : primary;
            }

            return primary;
        }
    }
}