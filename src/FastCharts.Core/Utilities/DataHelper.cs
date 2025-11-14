using System;
using System.Collections.Generic;

namespace FastCharts.Core.Utilities
{
    /// <summary>
    /// Helper methods for data processing operations.
    /// </summary>
    public static class DataHelper
    {
        /// <summary>
        /// Calculates the minimum and maximum values from a collection in a single pass.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="data">The collection of data points.</param>
        /// <param name="selector">Function to extract the value to compare.</param>
        /// <returns>A tuple containing the minimum and maximum values.</returns>
        /// <remarks>
        /// This method performs a single-pass iteration over the collection,
        /// which is more efficient than calling Min() and Max() separately.
        /// </remarks>
        public static (double Min, double Max) GetMinMax<T>(IList<T> data, Func<T, double> selector)
        {
            if (data == null || data.Count == 0)
            {
                return (0, 0);
            }

            var min = double.MaxValue;
            var max = double.MinValue;

            foreach (var item in data)
            {
                var value = selector(item);
                if (value < min) min = value;
                if (value > max) max = value;
            }

            return (min, max);
        }

        /// <summary>
        /// Calculates the minimum and maximum values from a collection with a custom comparison function.
        /// Useful for more complex scenarios like comparing calculated values.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="data">The collection of data points.</param>
        /// <param name="minSelector">Function to extract the minimum value candidate.</param>
        /// <param name="maxSelector">Function to extract the maximum value candidate.</param>
        /// <returns>A tuple containing the minimum and maximum values.</returns>
        public static (double Min, double Max) GetMinMax<T>(
            IList<T> data,
            Func<T, double> minSelector,
            Func<T, double> maxSelector)
        {
            if (data == null || data.Count == 0)
            {
                return (0, 0);
            }

            var min = double.MaxValue;
            var max = double.MinValue;

            foreach (var item in data)
            {
                var minValue = minSelector(item);
                var maxValue = maxSelector(item);
                if (minValue < min) min = minValue;
                if (maxValue > max) max = maxValue;
            }

            return (min, max);
        }
    }
}
