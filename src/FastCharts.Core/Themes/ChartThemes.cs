using FastCharts.Core.Abstractions;
using FastCharts.Core.Themes.BuiltIn;

namespace FastCharts.Core.Themes
{
    /// <summary>
    /// One-liner access to built-in themes: <c>model.Theme = ChartThemes.Dark;</c>
    /// Switching at runtime takes effect on the next render.
    /// </summary>
    public static class ChartThemes
    {
        /// <summary>
        /// Light theme (default)
        /// </summary>
        public static ITheme Light { get; } = new LightTheme();

        /// <summary>
        /// Dark theme
        /// </summary>
        public static ITheme Dark { get; } = new DarkTheme();

        /// <summary>
        /// High-contrast theme (black background, saturated colors, thicker strokes)
        /// </summary>
        public static ITheme HighContrast { get; } = new HighContrastTheme();
    }
}
