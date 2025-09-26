using System;
using System.Globalization;
using FastCharts.Core.Abstractions;

namespace FastCharts.Core.Formatting
{
    /// <summary>
    /// Formats numbers using scientific notation when the absolute value is large or very small.
    /// Examples: 1.23e+06, -9.5e-04. Falls back to plain number for moderate magnitudes.
    /// </summary>
    public sealed class ScientificNumberFormatter : INumberFormatter
    {
        /// <summary>
        /// Minimum absolute value (or its inverse) at which scientific notation is used.
        /// Default: 1e6 or < 1e-3.
        /// </summary>
        public double UpperThreshold { get; }
        public double LowerThreshold { get; }
        public int SignificantDigits { get; }

        public ScientificNumberFormatter(double upperThreshold = 1e6, double lowerThreshold = 1e-3, int significantDigits = 3)
        {
            if (upperThreshold <= 0) upperThreshold = 1e6;
            if (lowerThreshold <= 0 || lowerThreshold >= 1) lowerThreshold = 1e-3;
            if (significantDigits < 1) significantDigits = 3;
            UpperThreshold = upperThreshold;
            LowerThreshold = lowerThreshold;
            SignificantDigits = significantDigits;
        }

        public string Format(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
            double abs = Math.Abs(value);
            if ((abs >= UpperThreshold) || (abs > 0 && abs < LowerThreshold))
            {
                // Scientific formatting with given significant digits.
                return value.ToString("E" + (SignificantDigits - 1), CultureInfo.InvariantCulture);
            }
            // Plain formatting; G ensures trimming of trailing zeros while keeping reasonable precision.
            return value.ToString("G", CultureInfo.InvariantCulture);
        }
    }
}
