using System;
using FastCharts.Core.Factories;
using FastCharts.Core.Services;
using FastCharts.Core.Axes;
using FastCharts.Core.Themes.BuiltIn;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests
{
    /// <summary>
    /// Tests for ChartModel factory
    /// </summary>
    public class ChartModelFactoryTests
    {
        private readonly IDataRangeCalculatorService _mockService;
        private readonly ChartModelFactory _factory;

        public ChartModelFactoryTests()
        {
            _mockService = new DataRangeCalculatorService();
            _factory = new ChartModelFactory(_mockService);
        }

        [Fact]
        public void ConstructorWithNullServiceThrowsArgumentNullException()
        {
            // Act & Assert
            Action act = () => new ChartModelFactory(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateDefaultReturnsConfiguredChartModel()
        {
            // Act
            var model = _factory.CreateDefault();

            // Assert
            model.Should().NotBeNull();
            model.XAxis.Should().NotBeNull();
            model.YAxis.Should().NotBeNull();
            model.Series.Should().BeEmpty();
            model.Title.Should().Be("Chart");
        }

        [Fact]
        public void CreateWithConfigurationWithNullConfigurationThrowsArgumentNullException()
        {
            // Act & Assert
            Action act = () => _factory.CreateWithConfiguration(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateWithConfigurationWithValidConfigurationAppliesSettings()
        {
            // Arrange
            var config = new ChartConfiguration
            {
                XAxis = new DateTimeAxis(),
                Theme = new DarkTheme(),
                Title = "Test Chart"
            };

            // Act
            var model = _factory.CreateWithConfiguration(config);

            // Assert
            model.Should().NotBeNull();
            model.XAxis.Should().BeOfType<DateTimeAxis>();
            model.Theme.Should().BeOfType<DarkTheme>();
            model.Title.Should().Be("Test Chart");
        }

        [Fact]
        public void CreateWithConfigurationWithPartialConfigurationAppliesOnlyProvidedSettings()
        {
            // Arrange
            var config = new ChartConfiguration
            {
                Title = "Partial Config"
            };

            // Act
            var model = _factory.CreateWithConfiguration(config);

            // Assert
            model.Should().NotBeNull();
            model.Title.Should().Be("Partial Config");
            model.XAxis.Should().BeOfType<NumericAxis>(); // Default should remain
            model.Theme.Should().BeOfType<LightTheme>(); // Default should remain
        }

        [Fact]
        public void CreateWithConfigurationWithEmptyTitleDoesNotOverrideDefault()
        {
            // Arrange
            var config = new ChartConfiguration
            {
                Title = string.Empty
            };

            // Act
            var model = _factory.CreateWithConfiguration(config);

            // Assert
            model.Should().NotBeNull();
            model.Title.Should().Be("Chart"); // Should remain default
        }
    }
}