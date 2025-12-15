using FastCharts.Core.DependencyInjection;
using FastCharts.Core.Services;
using FastCharts.Core.Factories;
using FastCharts.Core.Extensions;

namespace FastCharts.Core.Examples
{
    /// <summary>
    /// Example of configuring and using FastCharts services with dependency injection
    /// </summary>
    public static class ServiceConfigurationExample
    {
        /// <summary>
        /// Configures the container with all FastCharts services
        /// </summary>
        public static IServiceContainer ConfigureServices()
        {
            var container = new SimpleServiceContainer();

            // Basic FastCharts service configuration
            container.AddFastChartsServices();

            return container;
        }

        /// <summary>
        /// Configures the container with custom options
        /// </summary>
        public static IServiceContainer ConfigureServicesWithOptions()
        {
            var container = new SimpleServiceContainer();

            // Custom configuration
            var options = new FastChartsOptions
            {
                EnableStrictValidation = true,
                MinRedrawIntervalMs = 33.3 // 30 FPS instead of 60
            };

            container.AddFastChartsServices(options);

            return container;
        }

        /// <summary>
        /// Example of using the container to create a ChartModel
        /// </summary>
        public static ChartModel CreateChartModelWithDI()
        {
            var container = ConfigureServices();

            // Dependency resolution
            var factory = container.Resolve<IChartModelFactory>();

            // ChartModel creation via factory
            var chartModel = factory.CreateDefault();

            return chartModel;
        }

        /// <summary>
        /// Example of usage with custom configuration
        /// </summary>
        public static ChartModel CreateCustomChartModel()
        {
            var container = ConfigureServices();
            var factory = container.Resolve<IChartModelFactory>();

            var config = new ChartConfiguration
            {
                Title = "Custom Chart",
                XAxis = new Axes.DateTimeAxis(),
                Theme = new Themes.BuiltIn.DarkTheme()
            };

            return factory.CreateWithConfiguration(config);
        }
    }
}