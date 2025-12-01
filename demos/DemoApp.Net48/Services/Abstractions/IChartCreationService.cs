using System.Collections.Generic;
using FastCharts.Core;

namespace DemoApp.Net48.Services.Abstractions
{
    /// <summary>
    /// Service interface for creating demo charts
    /// </summary>
    public interface IChartCreationService
    {
        /// <summary>
        /// Creates all demo charts
        /// </summary>
        /// <returns>Collection of demo charts</returns>
        IEnumerable<ChartModel> CreateDemoCharts();

        /// <summary>
        /// Creates a chart with random data
        /// </summary>
        /// <param name="pointCount">Number of data points</param>
        /// <returns>Chart with random data</returns>
        ChartModel CreateRandomChart(int pointCount = 50);
    }
}