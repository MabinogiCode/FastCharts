using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Formatting;
using FastCharts.Core.Primitives;
using FastCharts.Core.Scales;

namespace FastCharts.Core.Axes
{
    /// <summary>
    /// Categorical axis implementation for discrete string labels with custom spacing (P1-AX-CAT)
    /// Ideal for bar charts and other discrete data visualizations
    /// </summary>
    public sealed class CategoryAxis : AxisBase, IAxis<double>
    {
        private readonly List<string> _categories;
        private CategoryTicker _categoryTicker;

        /// <summary>
        /// Initializes a new CategoryAxis with empty categories
        /// </summary>
        public CategoryAxis()
        {
            _categories = new List<string>();
            _categoryTicker = new CategoryTicker(_categories);
            Scale = new LinearScale(0, 0, 0, 1);
            DataRange = new FRange(0, 0);
            VisibleRange = DataRange;
            NumberFormatter = null; // Categories don't use number formatting
        }

        /// <summary>
        /// Initializes a new CategoryAxis with predefined categories
        /// </summary>
        /// <param name="categories">List of category labels</param>
        public CategoryAxis(IEnumerable<string>? categories)
        {
            _categories = categories?.Where(c => !string.IsNullOrWhiteSpace(c)).ToList() ?? new List<string>();
            _categoryTicker = new CategoryTicker(_categories);

            var maxIndex = Math.Max(_categories.Count - 1, 0);
            Scale = new LinearScale(0, maxIndex, 0, 1);
            DataRange = new FRange(0, maxIndex);
            VisibleRange = DataRange;
            NumberFormatter = null;
        }

        /// <summary>
        /// Gets the list of categories for this axis
        /// </summary>
        public IReadOnlyList<string> Categories => _categories.AsReadOnly();

        /// <summary>
        /// Gets or sets the spacing between categories (default is 1.0)
        /// </summary>
        public double CategorySpacing { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets whether to show tick marks between categories
        /// </summary>
        public bool ShowCategoryTicks { get; set; } = true;

        public IScale<double> Scale { get; private set; }
        public ITicker<double> Ticker => _categoryTicker;
        public INumberFormatter? NumberFormatter { get; set; }

        /// <summary>
        /// Adds a category to the axis
        /// </summary>
        /// <param name="category">Category label to add</param>
        public void AddCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return;

            _categories.Add(category!); // Safe because we checked for null/whitespace above
            UpdateInternalState();
        }

        /// <summary>
        /// Adds multiple categories to the axis
        /// </summary>
        /// <param name="categories">Categories to add</param>
        public void AddCategories(IEnumerable<string> categories)
        {
            if (categories == null)
                return;

            var validCategories = categories.Where(c => !string.IsNullOrWhiteSpace(c));
            _categories.AddRange(validCategories);
            UpdateInternalState();
        }

        /// <summary>
        /// Sets the categories, replacing existing ones
        /// </summary>
        /// <param name="categories">New categories to set</param>
        public void SetCategories(IEnumerable<string> categories)
        {
            _categories.Clear();
            if (categories != null)
            {
                var validCategories = categories.Where(c => !string.IsNullOrWhiteSpace(c));
                _categories.AddRange(validCategories);
            }
            UpdateInternalState();
        }

        /// <summary>
        /// Clears all categories
        /// </summary>
        public void ClearCategories()
        {
            _categories.Clear();
            UpdateInternalState();
        }

        /// <summary>
        /// Gets the numeric position for a category label
        /// </summary>
        /// <param name="category">Category label</param>
        /// <returns>Numeric position, or -1 if not found</returns>
        public double GetCategoryPosition(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return -1;

            var index = _categories.IndexOf(category!); // Safe because we checked for null/whitespace above
            return index >= 0 ? index * CategorySpacing : -1;
        }

        /// <summary>
        /// Gets the category label at a given numeric position
        /// </summary>
        /// <param name="position">Numeric position</param>
        /// <returns>Category label, or null if position is invalid</returns>
        public string? GetCategoryAt(double position)
        {
            var index = (int)Math.Round(position / CategorySpacing);
            return index >= 0 && index < _categories.Count ? _categories[index] : null;
        }

        /// <summary>
        /// Gets the category index for a given position
        /// </summary>
        /// <param name="position">Numeric position</param>
        /// <returns>Category index, or -1 if invalid</returns>
        public int GetCategoryIndex(double position)
        {
            var index = (int)Math.Round(position / CategorySpacing);
            return index >= 0 && index < _categories.Count ? index : -1;
        }

        public override void UpdateScale(double pixelMin, double pixelMax)
        {
            Scale = new LinearScale(VisibleRange.Min, VisibleRange.Max, pixelMin, pixelMax);
        }

        /// <summary>
        /// Sets the visible range of categories
        /// </summary>
        /// <param name="startIndex">Starting category index</param>
        /// <param name="endIndex">Ending category index</param>
        public void SetVisibleCategories(int startIndex, int endIndex)
        {
            if (startIndex < 0) startIndex = 0;
            if (endIndex >= _categories.Count) endIndex = Math.Max(_categories.Count - 1, 0);
            if (startIndex > endIndex) (startIndex, endIndex) = (endIndex, startIndex);

            VisibleRange = new FRange(startIndex * CategorySpacing, endIndex * CategorySpacing);
        }

        /// <summary>
        /// Sets the visible range using numeric values
        /// </summary>
        /// <param name="min">Minimum visible value</param>
        /// <param name="max">Maximum visible value</param>
        public void SetVisibleRange(double min, double max)
        {
            if (double.IsNaN(min) || double.IsNaN(max))
                return;

            if (min > max)
                (min, max) = (max, min);

            // Ensure range is within category bounds
            var maxValue = Math.Max(_categories.Count - 1, 0) * CategorySpacing;
            min = Math.Max(0, Math.Min(min, maxValue));
            max = Math.Max(min, Math.Min(max, maxValue));

            VisibleRange = new FRange(min, max);
        }

        public IEnumerable<string> GetVisibleLabels(FRange visibleRange)
        {
            var startIndex = (int)Math.Floor(visibleRange.Min);
            var endIndex = (int)Math.Ceiling(visibleRange.Max);

            if (startIndex < 0)
            {
                startIndex = 0;
            }
            if (endIndex >= _categories.Count)
            {
                endIndex = Math.Max(_categories.Count - 1, 0);
            }
            if (startIndex > endIndex)
            {
                (startIndex, endIndex) = (endIndex, startIndex);
            }

            for (var i = startIndex; i <= endIndex && i < _categories.Count; i++)
            {
                yield return _categories[i];
            }
        }

        private void UpdateInternalState()
        {
            var maxIndex = Math.Max(_categories.Count - 1, 0);
            DataRange = new FRange(0, maxIndex * CategorySpacing);

            // Keep visible range within bounds
            if (VisibleRange.Max > DataRange.Max || VisibleRange.Min < DataRange.Min)
            {
                VisibleRange = DataRange;
            }

            _categoryTicker = new CategoryTicker(_categories, CategorySpacing);
        }
    }
}