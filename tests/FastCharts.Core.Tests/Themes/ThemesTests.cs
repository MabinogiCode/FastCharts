using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Themes;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.Theming
{
    /// <summary>
    /// Tests for dynamic theme switching and custom palettes (v1.2)
    /// </summary>
    public class ThemesTests
    {
        [Fact]
        public void BuiltInThemes_AreAvailableAsSingletons()
        {
            ChartThemes.Light.Should().NotBeNull();
            ChartThemes.Dark.Should().NotBeNull();
            ChartThemes.HighContrast.Should().NotBeNull();
            ChartThemes.Light.Should().BeSameAs(ChartThemes.Light);
        }

        [Fact]
        public void ChartModel_ThemeCanBeSwitchedAtRuntime()
        {
            using var model = new ChartModel();

            model.Theme = ChartThemes.Dark;
            model.Theme.Should().BeSameAs(ChartThemes.Dark);

            model.Theme = ChartThemes.HighContrast;
            model.Theme.Should().BeSameAs(ChartThemes.HighContrast);
        }

        [Fact]
        public void CustomTheme_StartsFromBaseThemeValues()
        {
            var custom = new CustomTheme(ChartThemes.Dark);

            custom.PlotBackgroundColor.Should().Be(ChartThemes.Dark.PlotBackgroundColor);
            custom.SeriesPalette.Should().BeEquivalentTo(ChartThemes.Dark.SeriesPalette);
        }

        [Fact]
        public void CustomTheme_PaletteAndColorsAreOverridable()
        {
            var palette = new[] { new ColorRgba(1, 2, 3), new ColorRgba(4, 5, 6) };
            var custom = new CustomTheme
            {
                SeriesPalette = palette,
                PlotBackgroundColor = new ColorRgba(10, 10, 10)
            };

            custom.SeriesPalette.Should().HaveCount(2);
            custom.SeriesPalette[0].R.Should().Be(1);
            custom.PlotBackgroundColor.G.Should().Be(10);
        }

        [Fact]
        public void HighContrastTheme_UsesBlackBackgroundAndThickAxes()
        {
            var theme = ChartThemes.HighContrast;

            theme.PlotBackgroundColor.R.Should().Be(0);
            theme.PlotBackgroundColor.G.Should().Be(0);
            theme.PlotBackgroundColor.B.Should().Be(0);
            theme.AxisThickness.Should().BeGreaterThan(1);
            theme.SeriesPalette.Should().NotBeEmpty();
        }
    }
}
