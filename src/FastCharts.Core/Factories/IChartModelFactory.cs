using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Services;

namespace FastCharts.Core.Factories
{
    /// <summary>
    /// Factory for creating and configuring chart models.
    /// </summary>
    public interface IChartModelFactory
    {
        ChartModel CreateDefault();
        ChartModel CreateWithConfiguration(ChartConfiguration configuration);
    }
}