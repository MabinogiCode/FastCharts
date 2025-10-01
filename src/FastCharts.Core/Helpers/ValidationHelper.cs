using FastCharts.Core.Primitives;

namespace FastCharts.Core.Helpers
{
    /// <summary>
    /// ? SOLID PRINCIPLES: Helper class for validation operations
    /// Extracted from static methods to enable proper unit testing and maintain SRP
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates that a double value is finite (not NaN or Infinity)
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <returns>True if the value is finite, false otherwise</returns>
        public static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        /// <summary>
        /// Validates that a range contains finite, valid values and min &lt; max
        /// </summary>
        /// <param name="range">The range to validate</param>
        /// <returns>True if the range is valid, false otherwise</returns>
        public static bool IsValidRange(FRange range)
        {
            return IsFinite(range.Min) && IsFinite(range.Max) && range.Min < range.Max;
        }

        /// <summary>
        /// Validates that both min and max values form a valid finite range
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>True if the range is valid, false otherwise</returns>
        public static bool IsValidRange(double min, double max)
        {
            return IsFinite(min) && IsFinite(max) && min < max;
        }

        /// <summary>
        /// Validates that a coordinate pair contains finite values
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if both coordinates are finite, false otherwise</returns>
        public static bool AreValidCoordinates(double x, double y)
        {
            return IsFinite(x) && IsFinite(y);
        }

        /// <summary>
        /// Validates that a zoom factor is positive and finite
        /// </summary>
        /// <param name="factor">The zoom factor to validate</param>
        /// <returns>True if the factor is valid for zooming, false otherwise</returns>
        public static bool IsValidZoomFactor(double factor)
        {
            return IsFinite(factor) && factor > 0;
        }
    }
}