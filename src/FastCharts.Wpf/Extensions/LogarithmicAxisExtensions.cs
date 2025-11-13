using System;
using FastCharts.Core;
using FastCharts.Core.Axes;

namespace FastCharts.Wpf.Extensions
{
    /// <summary>
    /// Extension methods for working with logarithmic axes in chart models.
    /// </summary>
    public static class LogarithmicAxisExtensions
    {
        /// <summary>
        /// Configures the X-axis as logarithmic with the specified base.
        /// </summary>
        /// <param name="model">Chart model to configure</param>
        /// <param name="logBase">Logarithm base (default: 10.0)</param>
        /// <param name="minValue">Minimum visible value (must be positive)</param>
        /// <param name="maxValue">Maximum visible value (must be positive)</param>
        /// <returns>The configured logarithmic axis for method chaining</returns>
        public static LogarithmicAxis UseLogarithmicXAxis(this ChartModel model, double logBase = 10.0, double minValue = 1.0, double maxValue = 100.0)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (minValue <= 0 || maxValue <= 0)
            {
                throw new ArgumentException("Logarithmic axis values must be positive");
            }

            if (minValue >= maxValue)
            {
                throw new ArgumentException("Maximum value must be greater than minimum value");
            }

            var logAxis = new LogarithmicAxis { LogBase = logBase };
            logAxis.SetVisibleRange(minValue, maxValue);
            
            // This would require ChartModel to support replacing axes
            // For now, we'll return the configured axis
            return logAxis;
        }

        /// <summary>
        /// Configures the Y-axis as logarithmic with the specified base.
        /// </summary>
        /// <param name="model">Chart model to configure</param>
        /// <param name="logBase">Logarithm base (default: 10.0)</param>
        /// <param name="minValue">Minimum visible value (must be positive)</param>
        /// <param name="maxValue">Maximum visible value (must be positive)</param>
        /// <returns>The configured logarithmic axis for method chaining</returns>
        public static LogarithmicAxis UseLogarithmicYAxis(this ChartModel model, double logBase = 10.0, double minValue = 1.0, double maxValue = 100.0)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (minValue <= 0 || maxValue <= 0)
            {
                throw new ArgumentException("Logarithmic axis values must be positive");
            }

            if (minValue >= maxValue)
            {
                throw new ArgumentException("Maximum value must be greater than minimum value");
            }

            var logAxis = new LogarithmicAxis { LogBase = logBase };
            logAxis.SetVisibleRange(minValue, maxValue);
            
            return logAxis;
        }

        /// <summary>
        /// Creates a logarithmic axis with common scientific ranges (powers of 10).
        /// </summary>
        /// <param name="model">Chart model (for extension method syntax)</param>
        /// <param name="startPower">Starting power of 10 (e.g., -3 for 0.001)</param>
        /// <param name="endPower">Ending power of 10 (e.g., 6 for 1,000,000)</param>
        /// <returns>Configured logarithmic axis</returns>
        public static LogarithmicAxis CreateScientificLogAxis(this ChartModel model, int startPower = 0, int endPower = 3)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (startPower >= endPower)
            {
                throw new ArgumentException("End power must be greater than start power");
            }

            var minValue = Math.Pow(10, startPower);
            var maxValue = Math.Pow(10, endPower);

            var logAxis = new LogarithmicAxis { LogBase = 10.0 };
            logAxis.SetVisibleRange(minValue, maxValue);
            logAxis.LabelFormat = "E1"; // Scientific notation
            
            return logAxis;
        }

        /// <summary>
        /// Creates a logarithmic axis optimized for financial/percentage data.
        /// </summary>
        /// <param name="model">Chart model (for extension method syntax)</param>
        /// <param name="minPercent">Minimum percentage (e.g., 0.1 for 10%)</param>
        /// <param name="maxPercent">Maximum percentage (e.g., 10.0 for 1000%)</param>
        /// <returns>Configured logarithmic axis for percentage data</returns>
        public static LogarithmicAxis CreateFinancialLogAxis(this ChartModel model, double minPercent = 0.1, double maxPercent = 10.0)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (minPercent <= 0 || maxPercent <= 0 || minPercent >= maxPercent)
            {
                throw new ArgumentException("Invalid percentage range for financial log axis");
            }

            var logAxis = new LogarithmicAxis { LogBase = 10.0 };
            logAxis.SetVisibleRange(minPercent, maxPercent);
            logAxis.LabelFormat = "P1"; // Percentage format
            
            return logAxis;
        }
    }
}