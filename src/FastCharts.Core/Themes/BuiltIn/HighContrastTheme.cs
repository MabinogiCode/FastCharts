using System.Collections.Generic;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Themes.BuiltIn
{
    /// <summary>
    /// High-contrast theme: black background, bright saturated series colors,
    /// thicker axes for accessibility.
    /// </summary>
    public sealed class HighContrastTheme : ITheme
    {
        private static readonly ColorRgba[] _palette =
        [
            new ColorRgba(255, 255,   0), // yellow
            new ColorRgba(  0, 255, 255), // cyan
            new ColorRgba(255,   0, 255), // magenta
            new ColorRgba(  0, 255,   0), // green
            new ColorRgba(255, 128,   0), // orange
            new ColorRgba(255, 255, 255), // white
        ];

        public ColorRgba AxisColor => new ColorRgba(255, 255, 255, 255);

        public ColorRgba GridColor => new ColorRgba(255, 255, 255, 80);

        public ColorRgba LabelColor => new ColorRgba(255, 255, 255, 255);

        public double AxisThickness => 2;

        public double GridThickness => 1;

        public double TickLength => 6;

        public double LabelTextSize => 13;

        public ColorRgba PrimarySeriesColor => new ColorRgba(255, 255, 0, 255);

        public IReadOnlyList<ColorRgba> SeriesPalette => _palette;

        public ColorRgba PlotBackgroundColor => new ColorRgba(0, 0, 0);

        public ColorRgba SurfaceBackgroundColor => new ColorRgba(0, 0, 0);
    }
}
