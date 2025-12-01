using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Themes.BuiltIn;
using DemoApp.Net8.Constants;
using DemoApp.Net8.Services.Abstractions;

namespace DemoApp.Net8.Services
{
    /// <summary>
    /// Service implementation for managing themes
    /// </summary>
    public sealed class ThemeService : IThemeService
    {
        /// <summary>
        /// Gets the theme by name
        /// </summary>
        /// <param name="themeName">Name of the theme</param>
        /// <returns>Theme instance</returns>
        public ITheme GetTheme(string themeName)
        {
            return themeName switch
            {
                ThemeConstants.Dark => new DarkTheme(),
                ThemeConstants.Light => new LightTheme(),
                _ => throw new ArgumentException($"Unknown theme: {themeName}", nameof(themeName))
            };
        }

        /// <summary>
        /// Gets the opposite theme name
        /// </summary>
        /// <param name="currentTheme">Current theme name</param>
        /// <returns>Opposite theme name</returns>
        public string GetOppositeTheme(string currentTheme)
        {
            return currentTheme == ThemeConstants.Light 
                ? ThemeConstants.Dark 
                : ThemeConstants.Light;
        }
    }
}