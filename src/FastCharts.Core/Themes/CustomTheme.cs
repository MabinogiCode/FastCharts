using System.Collections.Generic;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Themes.BuiltIn;

namespace FastCharts.Core.Themes
{
    /// <summary>
    /// Mutable theme for app-specific styling. Starts from a base theme
    /// (light by default) and lets you override any aspect, including the palette:
    /// <c>model.Theme = new CustomTheme(ChartThemes.Dark) { SeriesPalette = myColors };</c>
    /// </summary>
    public sealed class CustomTheme : ITheme
    {
        /// <summary>
        /// Creates a custom theme seeded from the light theme
        /// </summary>
        public CustomTheme()
            : this(new LightTheme())
        {
        }

        /// <summary>
        /// Creates a custom theme seeded from an existing theme
        /// </summary>
        /// <param name="baseTheme">Theme providing initial values</param>
        public CustomTheme(ITheme baseTheme)
        {
            AxisColor = baseTheme.AxisColor;
            GridColor = baseTheme.GridColor;
            LabelColor = baseTheme.LabelColor;
            AxisThickness = baseTheme.AxisThickness;
            GridThickness = baseTheme.GridThickness;
            TickLength = baseTheme.TickLength;
            LabelTextSize = baseTheme.LabelTextSize;
            PrimarySeriesColor = baseTheme.PrimarySeriesColor;
            SeriesPalette = baseTheme.SeriesPalette;
            PlotBackgroundColor = baseTheme.PlotBackgroundColor;
            SurfaceBackgroundColor = baseTheme.SurfaceBackgroundColor;
        }

        public ColorRgba AxisColor { get; set; }

        public ColorRgba GridColor { get; set; }

        public ColorRgba LabelColor { get; set; }

        public double AxisThickness { get; set; }

        public double GridThickness { get; set; }

        public double TickLength { get; set; }

        public double LabelTextSize { get; set; }

        public ColorRgba PrimarySeriesColor { get; set; }

        public IReadOnlyList<ColorRgba> SeriesPalette { get; set; }

        public ColorRgba PlotBackgroundColor { get; set; }

        public ColorRgba SurfaceBackgroundColor { get; set; }
    }
}
