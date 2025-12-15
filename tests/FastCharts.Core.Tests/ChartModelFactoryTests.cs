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
        private readonly IDataRangeCalculatorService _dataRangeService;
        private readonly IInteractionService _interactionService;
        private readonly ILegendSyncService _legendSyncService;
        private readonly IAxisManagementService _axisManagementService;
        private readonly ChartModelFactory _factory;

        public ChartModelFactoryTests()
        {
            _dataRangeService = new DataRangeCalculatorService();
            _interactionService = new InteractionService();
            _legendSyncService = new LegendSyncService();
            _axisManagementService = new AxisManagementService();
            _factory = new ChartModelFactory(_dataRangeService, _interactionService, _legendSyncService, _axisManagementService);
        }

        [Fact]
        public void ConstructorWithNullServiceThrowsArgumentNullException()
        {
            // Act & Assert
            var act1 = () => new ChartModelFactory(null!, _interactionService, _legendSyncService, _axisManagementService);
            var act2 = () => new ChartModelFactory(_dataRangeService, null!, _legendSyncService, _axisManagementService);
            var act3 = () => new ChartModelFactory(_dataRangeService, _interactionService, null!, _axisManagementService);
            var act4 = () => new ChartModelFactory(_dataRangeService, _interactionService, _legendSyncService, null!);

            act1.Should().Throw<ArgumentNullException>();
            act2.Should().Throw<ArgumentNullException>();
            act3.Should().Throw<ArgumentNullException>();
            act4.Should().Throw<ArgumentNullException>();
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
            var act = () => _factory.CreateWithConfiguration(null!);
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