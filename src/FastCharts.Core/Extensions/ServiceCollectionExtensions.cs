using FastCharts.Core.DependencyInjection;
using FastCharts.Core.Services;
using FastCharts.Core.Factories;

namespace FastCharts.Core.Extensions
{
    /// <summary>
    /// Extensions for configuring FastCharts services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures all FastCharts services in the container.
        /// </summary>
        public static IServiceContainer AddFastChartsServices(this IServiceContainer services)
        {
            // Core services
            services.RegisterSingleton<IDataRangeCalculatorService, DataRangeCalculatorService>();
            services.RegisterSingleton<IBehaviorManager, BehaviorManager>();

            // Factories
            services.RegisterTransient<IChartModelFactory, ChartModelFactory>();

            return services;
        }

        /// <summary>
        /// Configures FastCharts services with custom options.
        /// </summary>
        public static IServiceContainer AddFastChartsServices(this IServiceContainer services,
            FastChartsOptions options)
        {
            AddFastChartsServices(services);

            // Custom configuration based on options
            if (options.DefaultDataRangeCalculator != null)
            {
                services.RegisterInstance<IDataRangeCalculatorService>(options.DefaultDataRangeCalculator);
            }

            if (options.DefaultBehaviorManager != null)
            {
                services.RegisterInstance<IBehaviorManager>(options.DefaultBehaviorManager);
            }

            return services;
        }
    }
}