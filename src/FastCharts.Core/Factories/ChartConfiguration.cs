using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;

namespace FastCharts.Core.Factories
{
    /// <summary>
    /// Configuration for ChartModel creation.
    /// </summary>
    public class ChartConfiguration
    {
        public IAxis<double>? XAxis { get; set; }
        public IAxis<double>? YAxis { get; set; }
        public ITheme? Theme { get; set; }
        public string? Title { get; set; }
        public bool AutoConfigureBehaviors { get; set; } = true;
    }
}