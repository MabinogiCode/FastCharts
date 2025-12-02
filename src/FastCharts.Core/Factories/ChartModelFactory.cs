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
        private readonly IInteractionService _interactionService;
        private readonly ILegendSyncService _legendSyncService;
        private readonly IAxisManagementService _axisManagementService;

        public ChartModelFactory(
            IDataRangeCalculatorService dataRangeCalculator,
            IInteractionService interactionService,
            ILegendSyncService legendSyncService,
            IAxisManagementService axisManagementService)
        {
            _dataRangeCalculator = dataRangeCalculator ?? throw new ArgumentNullException(nameof(dataRangeCalculator));
            _interactionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
            _legendSyncService = legendSyncService ?? throw new ArgumentNullException(nameof(legendSyncService));
            _axisManagementService = axisManagementService ?? throw new ArgumentNullException(nameof(axisManagementService));
        }

        public ChartModel CreateDefault()
        {
            return new ChartModel(_dataRangeCalculator, _interactionService, _legendSyncService, _axisManagementService);
        }

        public ChartModel CreateWithConfiguration(ChartConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var model = new ChartModel(_dataRangeCalculator, _interactionService, _legendSyncService, _axisManagementService);

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