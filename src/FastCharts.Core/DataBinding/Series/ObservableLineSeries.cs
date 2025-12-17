using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.DataBinding;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Core.DataBinding.Series
{
    /// <summary>
    /// Observable line series that supports data binding
    /// Automatically synchronizes with source collections and updates when data changes
    /// </summary>
    public class ObservableLineSeries : ObservableSeriesBase<object>, ILineSeries
    {
        private readonly List<PointD> _data = new();

        /// <summary>
        /// Gets the line series data as points
        /// </summary>
        public IReadOnlyList<PointD> Data => _data.AsReadOnly();

        /// <inheritdoc />
        public override bool IsEmpty => _data.Count == 0;

        /// <summary>
        /// Gets or sets whether to show markers on data points
        /// </summary>
        public bool ShowMarkers { get; set; }

        /// <summary>
        /// Gets or sets the marker size in pixels
        /// </summary>
        public double MarkerSize { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the marker shape
        /// </summary>
        public MarkerShape MarkerShape { get; set; } = MarkerShape.Circle;

        /// <summary>
        /// Gets or sets whether to enable auto-resampling for large datasets
        /// </summary>
        public bool EnableAutoResampling { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the ObservableLineSeries class
        /// </summary>
        public ObservableLineSeries()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ObservableLineSeries class with data source
        /// </summary>
        /// <param name="itemsSource">Data source</param>
        /// <param name="xPath">Property path for X values</param>
        /// <param name="yPath">Property path for Y values</param>
        public ObservableLineSeries(IEnumerable<object> itemsSource, string xPath, string yPath)
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
            return $"ObservableLineSeries: {_data.Count} points from {sourceType} (X: {XPath}, Y: {YPath})";
        }
    }
}