using System;
using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes.Ticks
{
    /// <summary>
    /// Ticker implementation for categorical axes that generates ticks at category positions
    /// </summary>
    public sealed class CategoryTicker : ITicker<double>
    {
        private readonly IReadOnlyList<string> _categories;
        private readonly double _spacing;

        /// <summary>
        /// Initializes a new CategoryTicker with the given categories
        /// </summary>
        /// <param name="categories">List of category labels</param>
        /// <param name="spacing">Spacing between categories (default 1.0)</param>
        public CategoryTicker(IReadOnlyList<string>? categories, double spacing = 1.0)
        {
            _categories = categories ?? new List<string>();
            _spacing = Math.Max(spacing, 0.1); // Prevent zero or negative spacing
        }

        /// <summary>
        /// Generates major ticks at each category position
        /// </summary>
        /// <param name="range">Visible range of the axis</param>
        /// <param name="approxStep">Approximate step size (ignored for categories)</param>
        /// <returns>Collection of tick positions</returns>
        public IReadOnlyList<double> GetTicks(FRange range, double approxStep)
        {
            var ticks = new List<double>();

            if (_categories.Count == 0)
                return ticks;

            // Calculate which category indices are visible
            var startIndex = (int)Math.Max(0, Math.Floor(range.Min / _spacing));
            var endIndex = (int)Math.Min(_categories.Count - 1, Math.Ceiling(range.Max / _spacing));

            // Generate ticks for visible categories
            for (var i = startIndex; i <= endIndex; i++)
            {
                var position = i * _spacing;
                if (position >= range.Min && position <= range.Max)
                {
                    ticks.Add(position);
                }
            }

            return ticks;
        }

        /// <summary>
        /// Generates minor ticks between categories (typically none for categorical data)
        /// </summary>
        /// <param name="range">Visible range of the axis</param>
        /// <param name="majorTicks">Major tick positions for reference</param>
        /// <returns>Collection of minor tick positions (empty for categories)</returns>
        public IReadOnlyList<double> GetMinorTicks(FRange range, IReadOnlyList<double> majorTicks)
        {
            // For categorical axes, we typically don't want minor ticks between categories
            // This maintains the discrete nature of categorical data
            return new List<double>();
        }
    }
}