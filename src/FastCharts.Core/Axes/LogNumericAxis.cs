using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Formatting;
using FastCharts.Core.Primitives;
using FastCharts.Core.Scales;
using System;

namespace FastCharts.Core.Axes
{
    /// <summary>
    /// Logarithmic numeric axis for exponential data scaling
    /// Provides logarithmic transformation with proper tick generation and formatting
    /// </summary>
    public sealed class LogNumericAxis : AxisBase, IAxis<double>
    {
        private readonly double _logBase;
        public const double MinPositiveValue = 1e-10;

        /// <summary>
        /// Creates a logarithmic numeric axis
        /// </summary>
        /// <param name="logBase">Base for logarithmic scaling (default: 10)</param>
        public LogNumericAxis(double logBase = 10.0)
        {
            if (logBase <= 0 || Math.Abs(logBase - 1.0) < 1e-10)
            {
                throw new ArgumentException("Logarithm base must be positive and not equal to 1");
            }

            _logBase = logBase;

            // Initialize with safe positive range
            var minVal = 1.0;
            var maxVal = 100.0;

            Scale = new LogScale(minVal, maxVal, 0, 1, _logBase);
            Ticker = new LogTicker(_logBase);
            DataRange = new FRange(minVal, maxVal);
            VisibleRange = new FRange(minVal, maxVal);

            // Use scientific notation for logarithmic values
            NumberFormatter = new ScientificNumberFormatter();
        }

        public IScale<double> Scale { get; private set; }
        public ITicker<double> Ticker { get; }
        public INumberFormatter? NumberFormatter { get; set; }

        /// <summary>
        /// Gets the logarithm base used by this axis
        /// </summary>
        public double LogBase => _logBase;

        public override void UpdateScale(double pixelMin, double pixelMax)
        {
            // Ensure we have a valid positive range
            var minVal = Math.Max(VisibleRange.Min, MinPositiveValue);
            var maxVal = Math.Max(VisibleRange.Max, minVal * 1.1);

            Scale = new LogScale(minVal, maxVal, pixelMin, pixelMax, _logBase);
        }

        /// <summary>
        /// Updates the visible range with logarithmic constraints
        /// Ensures values are positive and properly ordered
        /// </summary>
        /// <param name="min">Minimum value (will be clamped to positive)</param>
        /// <param name="max">Maximum value (will be clamped to positive)</param>
        public void SetVisibleRange(double min, double max)
        {
            if (double.IsNaN(min) || double.IsNaN(max))
            {
                return;
            }

            // Ensure positive values for logarithmic scale
            min = Math.Max(min, MinPositiveValue);
            max = Math.Max(max, MinPositiveValue);

            if (min > max)
            {
                (min, max) = (max, min);
            }

            // Ensure minimum ratio between min and max for log scale
            var ratio = max / min;
            if (ratio < 1.1) // Minimum 10% difference
            {
                var center = Math.Sqrt(min * max); // Geometric mean
                var expansion = Math.Max(center * 0.1, MinPositiveValue);
                min = Math.Max(center - expansion, MinPositiveValue);
                max = center + expansion;
            }

            VisibleRange = new FRange(min, max);
        }

        /// <summary>
        /// Sets the visible range using powers of the logarithm base
        /// Convenient for setting ranges like 1-1000 (10^0 to 10^3)
        /// </summary>
        /// <param name="minPower">Minimum power (e.g., 0 for 10^0 = 1)</param>
        /// <param name="maxPower">Maximum power (e.g., 3 for 10^3 = 1000)</param>
        public void SetVisibleRangeByPowers(double minPower, double maxPower)
        {
            var min = Math.Pow(_logBase, minPower);
            var max = Math.Pow(_logBase, maxPower);
            SetVisibleRange(min, max);
        }

        /// <summary>
        /// Gets the visible range expressed as powers of the logarithm base
        /// </summary>
        public (double MinPower, double MaxPower) GetVisibleRangePowers()
        {
            var minPower = Math.Log(VisibleRange.Min, _logBase);
            var maxPower = Math.Log(VisibleRange.Max, _logBase);
            return (minPower, maxPower);
        }

        /// <summary>
        /// Checks if a value is within the valid range for logarithmic axes
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if value is positive and within range</returns>
        public static bool IsValidValue(double value)
        {
            return value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        /// <summary>
        /// Clamps a value to the valid range for logarithmic axes
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <returns>Clamped value that is safe for logarithmic operations</returns>
        public static double ClampValue(double value)
        {
            if (!IsValidValue(value))
            {
                return MinPositiveValue;
            }

            return Math.Max(value, MinPositiveValue);
        }
    }
}