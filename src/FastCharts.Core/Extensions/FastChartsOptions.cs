using FastCharts.Core.Services;

namespace FastCharts.Core.Extensions
{
    /// <summary>
    /// Configuration options for FastCharts.
    /// </summary>
    public class FastChartsOptions
    {
        /// <summary>
        /// Custom service for data range calculations.
        /// </summary>
        public IDataRangeCalculatorService? DefaultDataRangeCalculator { get; set; }

        /// <summary>
        /// Custom behavior manager.
        /// </summary>
        public IBehaviorManager? DefaultBehaviorManager { get; set; }

        /// <summary>
        /// Enables or disables strict validations.
        /// </summary>
        public bool EnableStrictValidation { get; set; } = true;

        /// <summary>
        /// Minimum interval between redraws (in milliseconds).
        /// </summary>
        public double MinRedrawIntervalMs { get; set; } = 16.6; // 60 FPS by default
    }
}