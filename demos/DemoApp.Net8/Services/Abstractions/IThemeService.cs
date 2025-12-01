using FastCharts.Core.Abstractions;
using FastCharts.Core.Themes;

namespace DemoApp.Net8.Services.Abstractions
{
    /// <summary>
    /// Service interface for managing themes
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the theme by name
        /// </summary>
        /// <param name="themeName">Name of the theme</param>
        /// <returns>Theme instance</returns>
        ITheme GetTheme(string themeName);

        /// <summary>
        /// Gets the opposite theme name
        /// </summary>
        /// <param name="currentTheme">Current theme name</param>
        /// <returns>Opposite theme name</returns>
        string GetOppositeTheme(string currentTheme);
    }
}