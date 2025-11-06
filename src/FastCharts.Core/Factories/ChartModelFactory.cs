using System;
using FastCharts.Core.Axes;
using FastCharts.Core.Services;

namespace FastCharts.Core.Factories
{
    /// <summary>
    /// Implementation of ChartModel factory.
    /// </summary>
    public class ChartModelFactory : IChartModelFactory
    {
        private readonly IDataRangeCalculatorService _dataRangeCalculator;

        public ChartModelFactory(IDataRangeCalculatorService dataRangeCalculator)
        {
            _dataRangeCalculator = dataRangeCalculator ?? throw new ArgumentNullException(nameof(dataRangeCalculator));
        }

        public ChartModel CreateDefault()
        {
            return new ChartModel(_dataRangeCalculator);
        }

        public ChartModel CreateWithConfiguration(ChartConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var model = new ChartModel(_dataRangeCalculator);

            if (configuration.XAxis != null)
            {
                model.ReplaceXAxis(configuration.XAxis);
            }

            if (configuration.YAxis != null)
            {
                model.ReplaceYAxis(configuration.YAxis);
            }

            if (configuration.Theme != null)
            {
                model.Theme = configuration.Theme;
            }

            if (!string.IsNullOrEmpty(configuration.Title))
            {
                model.Title = configuration.Title!;
            }

            return model;
        }
    }
}