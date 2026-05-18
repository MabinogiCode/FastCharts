using System;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Wpf.Extensions
{
    /// <summary>
    /// Extension methods for configuring logarithmic axes on a chart model.
    /// </summary>
    public static class LogarithmicAxisExtensions
    {
        /// <summary>
        /// Configures the X-axis as logarithmic with the specified base and visible range.
        /// </summary>
        /// <param name="model">Chart model to configure.</param>
        /// <param name="logBase">Logarithm base (default: 10.0).</param>
        /// <param name="minValue">Minimum visible value (must be positive).</param>
        /// <param name="maxValue">Maximum visible value (must be positive).</param>
        /// <returns>The configured X-axis for method chaining.</returns>
        public static IAxis<double> UseLogarithmicXAxis(this ChartModel model, double logBase = 10.0, double minValue = 1.0, double maxValue = 100.0)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ValidateRange(minValue, maxValue);

            model.SetXAxisLogarithmic(logBase);
            model.XAxis.VisibleRange = new FRange(minValue, maxValue);
            return model.XAxis;
        }

        /// <summary>
        /// Configures the Y-axis as logarithmic with the specified base and visible range.
        /// </summary>
        /// <param name="model">Chart model to configure.</param>
        /// <param name="logBase">Logarithm base (default: 10.0).</param>
        /// <param name="minValue">Minimum visible value (must be positive).</param>
        /// <param name="maxValue">Maximum visible value (must be positive).</param>
        /// <returns>The configured Y-axis for method chaining.</returns>
        public static IAxis<double> UseLogarithmicYAxis(this ChartModel model, double logBase = 10.0, double minValue = 1.0, double maxValue = 100.0)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ValidateRange(minValue, maxValue);

            model.SetYAxisLogarithmic(logBase);
            model.YAxis.VisibleRange = new FRange(minValue, maxValue);
            return model.YAxis;
        }

        /// <summary>
        /// Configures the X-axis as a base-10 logarithmic axis spanning the given powers of 10,
        /// using scientific-notation labels.
        /// </summary>
        /// <param name="model">Chart model to configure.</param>
        /// <param name="startPower">Starting power of 10 (e.g. -3 for 0.001).</param>
        /// <param name="endPower">Ending power of 10 (e.g. 6 for 1,000,000).</param>
        /// <returns>The configured X-axis.</returns>
        public static IAxis<double> CreateScientificLogAxis(this ChartModel model, int startPower = 0, int endPower = 3)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (startPower >= endPower)
            {
                throw new ArgumentException("End power must be greater than start power");
            }

            var axis = UseLogarithmicXAxis(model, 10.0, Math.Pow(10, startPower), Math.Pow(10, endPower));
            axis.LabelFormat = "E1";
            return axis;
        }

        /// <summary>
        /// Configures the Y-axis as a base-10 logarithmic axis for financial/percentage data,
        /// using percentage labels.
        /// </summary>
        /// <param name="model">Chart model to configure.</param>
        /// <param name="minPercent">Minimum percentage value (must be positive).</param>
        /// <param name="maxPercent">Maximum percentage value (must be positive).</param>
        /// <returns>The configured Y-axis.</returns>
        public static IAxis<double> CreateFinancialLogAxis(this ChartModel model, double minPercent = 0.1, double maxPercent = 10.0)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var axis = UseLogarithmicYAxis(model, 10.0, minPercent, maxPercent);
            axis.LabelFormat = "P1";
            return axis;
        }

        private static void ValidateRange(double minValue, double maxValue)
        {
            if (minValue <= 0 || maxValue <= 0)
            {
                throw new ArgumentException("Logarithmic axis values must be positive");
            }

            if (minValue >= maxValue)
            {
                throw new ArgumentException("Maximum value must be greater than minimum value");
            }
        }
    }
}
