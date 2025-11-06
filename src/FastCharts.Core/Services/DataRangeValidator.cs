using System;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Helper for validating and normalizing data ranges.
    /// </summary>
    public static class DataRangeValidator
    {
        /// <summary>
        /// Ensures the provided min/max defines a valid range; expands zero-span and replaces invalid values.
        /// </summary>
        public static FRange EnsureValidRange(double min, double max)
        {
            if (double.IsNaN(min) || double.IsNaN(max) || double.IsInfinity(min) || double.IsInfinity(max))
            {
                return new FRange(0, 1);
            }

            if (Math.Abs(max - min) < 1e-10)
            {
                var center = min;
                var halfSpan = Math.Max(Math.Abs(center) * 0.1, 0.5);
                return new FRange(center - halfSpan, center + halfSpan);
            }

            return new FRange(min, max);
        }
    }
}