using System;
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
        /// Single source of truth for series colors: every rendering layer, the legend and the
        /// tooltip use it. <see cref="SeriesBase.PaletteIndex"/> wins when set.
        /// Allocation-free: computes the per-group index in a single pass over the series list.
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
        /// Returns the position of the series among the series of the same color group (the
        /// series rendered by the same layer). Hidden series are counted too, so toggling a
        /// series off in the legend never shifts the colors of the others.
        /// </summary>
        private static int IndexAmongKind(ChartModel model, SeriesBase series)
        {
            var group = ColorGroup(series);
            var count = 0;
            var seriesList = model.Series;

            for (var i = 0; i < seriesList.Count; i++)
            {
                var candidate = seriesList[i];
                if (ReferenceEquals(candidate, series))
                {
                    return count;
                }

                if (ColorGroup(candidate) == group)
                {
                    count++;
                }
            }

            return -1;
        }

        /// <summary>
        /// Color group of a series: one sequence per rendering layer, so legend swatches,
        /// tooltips and the drawn series always agree.
        /// </summary>
        private static Type ColorGroup(SeriesBase series)
        {
            return series switch
            {
                AreaSeries => typeof(AreaSeries),
                StepLineSeries => typeof(StepLineSeries),
                LineSeries => typeof(LineSeries),
                ScatterSeries => typeof(ScatterSeries),
                BarSeries => typeof(BarSeries),
                _ => series.GetType()
            };
        }
    }
}
