using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.DataBinding;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Core.DataBinding.Series
{
    /// <summary>
    /// Observable bar series that supports data binding
    /// Automatically synchronizes with source collections and updates when data changes
    /// </summary>
    public class ObservableBarSeries : ObservableSeriesBase<object>, IBarSeries
    {
        private readonly List<BarPoint> _data = new();

        /// <summary>
        /// Gets the bar series data as bar points
        /// </summary>
        public IReadOnlyList<BarPoint> Data => _data.AsReadOnly();

        /// <inheritdoc />
        public override bool IsEmpty => _data.Count == 0;

        /// <summary>
        /// Gets or sets the bar width (null for auto-sizing)
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// Gets or sets the baseline Y value for bars
        /// </summary>
        public double Baseline { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the bar fill opacity (0.0 to 1.0)
        /// </summary>
        public double FillOpacity { get; set; } = 0.8;

        /// <summary>
        /// Gets or sets the property path for bar labels
        /// </summary>
        public string? LabelPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the ObservableBarSeries class
        /// </summary>
        public ObservableBarSeries()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ObservableBarSeries class with data source
        /// </summary>
        /// <param name="itemsSource">Data source</param>
        /// <param name="xPath">Property path for X values (categories)</param>
        /// <param name="yPath">Property path for Y values (heights)</param>
        public ObservableBarSeries(IEnumerable<object> itemsSource, string xPath, string yPath)
        {
            ItemsSource = itemsSource;
            XPath = xPath;
            YPath = yPath;
        }

        /// <inheritdoc />
        protected override void UpdateSeriesData(IEnumerable<PointD> points)
        {
            _data.Clear();
            
            foreach (var point in points)
            {
                var barPoint = new BarPoint(point.X, point.Y);
                // Note: BarPoint doesn't have Label property yet, this will be added when BarPoint is enhanced
                _data.Add(barPoint);
            }
        }

        /// <inheritdoc />
        protected override int GetPointCount()
        {
            return _data.Count;
        }

        /// <summary>
        /// Converts source items to bar points using property paths
        /// </summary>
        /// <param name="items">Source items</param>
        /// <returns>Converted bar points</returns>
        protected override IEnumerable<PointD> ConvertToPoints(IEnumerable<object> items)
        {
            var itemList = items.ToList();
            for (var i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                var x = GetCoordinateValue(item, XPath, i); // Default to index if no X path
                var y = GetCoordinateValue(item, YPath, 0.0);

                if (DataBindingConverter.IsValidCoordinate(x) && DataBindingConverter.IsValidCoordinate(y))
                {
                    yield return new PointD(DataBindingConverter.ToDouble(x), DataBindingConverter.ToDouble(y));
                }
            }
        }

        /// <summary>
        /// Gets the data points for rendering
        /// </summary>
        /// <returns>Data points as bar points</returns>
        public IEnumerable<BarPoint> GetRenderPoints()
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
            return $"ObservableBarSeries: {_data.Count} bars from {sourceType} (X: {XPath}, Y: {YPath})";
        }
    }
}