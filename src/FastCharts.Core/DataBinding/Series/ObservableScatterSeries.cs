using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.DataBinding;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Core.DataBinding.Series
{
    /// <summary>
    /// Observable scatter series that supports data binding
    /// Automatically synchronizes with source collections and updates when data changes
    /// </summary>
    public class ObservableScatterSeries : ObservableSeriesBase<object>, IScatterSeries
    {
        private readonly List<PointD> _data = new();

        /// <summary>
        /// Gets the scatter series data as points
        /// </summary>
        public IReadOnlyList<PointD> Data => _data.AsReadOnly();

        /// <inheritdoc />
        public override bool IsEmpty => _data.Count == 0;

        /// <summary>
        /// Gets or sets the marker size in pixels
        /// </summary>
        public double MarkerSize { get; set; } = 6.0;

        /// <summary>
        /// Gets or sets the marker shape
        /// </summary>
        public MarkerShape MarkerShape { get; set; } = MarkerShape.Circle;

        /// <summary>
        /// Gets or sets the marker fill opacity (0.0 to 1.0)
        /// </summary>
        public double MarkerOpacity { get; set; } = 1.0;

        /// <summary>
        /// Initializes a new instance of the ObservableScatterSeries class
        /// </summary>
        public ObservableScatterSeries()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ObservableScatterSeries class with data source
        /// </summary>
        /// <param name="itemsSource">Data source</param>
        /// <param name="xPath">Property path for X values</param>
        /// <param name="yPath">Property path for Y values</param>
        public ObservableScatterSeries(IEnumerable<object> itemsSource, string xPath, string yPath)
        {
            ItemsSource = itemsSource;
            XPath = xPath;
            YPath = yPath;
        }

        /// <inheritdoc />
        protected override void UpdateSeriesData(IEnumerable<PointD> points)
        {
            _data.Clear();
            _data.AddRange(points);
        }

        /// <inheritdoc />
        protected override int GetPointCount()
        {
            return _data.Count;
        }

        /// <summary>
        /// Gets the data points for rendering
        /// </summary>
        /// <returns>Data points</returns>
        public IEnumerable<PointD> GetRenderPoints()
        {
            return _data;
        }

        /// <summary>
        /// Gets a human-readable summary of the series
        /// </summary>
        /// <returns>Series summary</returns>
        public override string ToString()
        {
            var sourceType = ItemsSource?.GetType().Name ?? "null";
            return $"ObservableScatterSeries: {_data.Count} points from {sourceType} (X: {XPath}, Y: {YPath})";
        }
    }
}