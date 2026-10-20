using System;
using FastCharts.Core.Formatting;
using FastCharts.Core.Primitives;
using FastCharts.Core.Themes;

namespace FastCharts.Core.Axes
{
    /// <summary>
    /// Color scale axis (P1-AX-COLOR): maps values to colors through a <see cref="Themes.ColorMap"/>
    /// over a range that is either fixed (<see cref="Minimum"/>/<see cref="Maximum"/>) or fitted to
    /// the data. Drawn as a color bar next to the plot when <see cref="ShowColorBar"/> is set.
    /// One instance can be shared by several heatmaps so they use the same scale.
    /// </summary>
    public sealed class ColorAxis
    {
        private ColorMap _colorMap = ColorMaps.Viridis;

        /// <summary>
        /// Gradient used to color values (default <see cref="ColorMaps.Viridis"/>).
        /// </summary>
        public ColorMap ColorMap
        {
            get => _colorMap;
            set => _colorMap = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Value mapped to the first color. Null = minimum of the data.
        /// </summary>
        public double? Minimum { get; set; }

        /// <summary>
        /// Value mapped to the last color. Null = maximum of the data.
        /// </summary>
        public double? Maximum { get; set; }

        /// <summary>
        /// Optional title drawn above the color bar.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Formatter of the color bar labels.
        /// </summary>
        public INumberFormatter? NumberFormatter { get; set; } = new CompactNumberFormatter();

        /// <summary>
        /// Color of missing values (NaN, infinities). Transparent by default.
        /// </summary>
        public ColorRgba MissingValueColor { get; set; } = new ColorRgba(0, 0, 0, 0);

        /// <summary>
        /// Whether the color bar (gradient legend with ticks) is drawn next to the plot.
        /// </summary>
        public bool ShowColorBar { get; set; } = true;

        /// <summary>
        /// Range actually used for coloring: the fixed bounds when set, otherwise
        /// <paramref name="dataRange"/>. Always returns a valid, non-empty range.
        /// </summary>
        public FRange ResolveRange(FRange dataRange)
        {
            var min = Minimum ?? dataRange.Min;
            var max = Maximum ?? dataRange.Max;
            if (double.IsNaN(min) || double.IsInfinity(min) || double.IsNaN(max) || double.IsInfinity(max))
            {
                return new FRange(0, 1);
            }

            if (min > max)
            {
                (min, max) = (max, min);
            }

            if (max - min < 1e-12)
            {
                // Single value: center it so it gets the middle color
                return new FRange(min - 0.5, max + 0.5);
            }

            return new FRange(min, max);
        }

        /// <summary>
        /// Color of <paramref name="value"/> on <paramref name="range"/> (see <see cref="ResolveRange"/>).
        /// Values outside the range take the end colors; missing values take <see cref="MissingValueColor"/>.
        /// </summary>
        public ColorRgba GetColor(double value, FRange range)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return MissingValueColor;
            }

            var span = range.Max - range.Min;
            return _colorMap.Map(span > 0 ? (value - range.Min) / span : 0.5);
        }
    }
}
