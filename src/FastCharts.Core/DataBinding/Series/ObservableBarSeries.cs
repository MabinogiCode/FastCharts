using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Core.DataBinding.Series
{
    /// <summary>
    /// Bar series with observable data binding (MVVM).
    /// Inherits <see cref="BarSeries"/>, so it renders and participates in range
    /// calculation like any other series while staying synchronized with its
    /// <see cref="ItemsSource"/> through X/Y property paths.
    /// String/category X values are mapped to sequential indexes automatically.
    /// </summary>
    public class ObservableBarSeries : BarSeries, IObservableSeries<object>, IBarSeries, IDisposable
    {
        private readonly SeriesDataBinder _binder;
        /// <summary>
        /// Gets or sets the property path for bar labels
        /// </summary>
        public string? LabelPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the ObservableBarSeries class
        /// </summary>
        public ObservableBarSeries()
        {
            _binder = new SeriesDataBinder(ApplyPoints, () => Data.Count)
            {
                CustomConverter = ConvertItems
            };
            _binder.DataBindingUpdated += (_, args) => DataBindingUpdated?.Invoke(this, args);
        }

        /// <summary>
        /// Initializes a new instance of the ObservableBarSeries class with data source
        /// </summary>
        /// <param name="itemsSource">Data source</param>
        /// <param name="xPath">Property path for X values (numbers or categories)</param>
        /// <param name="yPath">Property path for Y values (heights)</param>
        public ObservableBarSeries(IEnumerable<object> itemsSource, string xPath, string yPath)
            : this()
        {
            _binder.XPath = xPath;
            _binder.YPath = yPath;
            _binder.ItemsSource = itemsSource;
        }

        /// <inheritdoc />
        public event EventHandler<DataBindingUpdatedEventArgs>? DataBindingUpdated;

        /// <inheritdoc />
        public IEnumerable<object> ItemsSource
        {
            get => (_binder.ItemsSource as IEnumerable<object>) ?? Enumerable.Empty<object>();
            set => _binder.ItemsSource = value;
        }

        /// <inheritdoc />
        public string? XPath
        {
            get => _binder.XPath;
            set => _binder.XPath = value;
        }

        /// <inheritdoc />
        public string? YPath
        {
            get => _binder.YPath;
            set => _binder.YPath = value;
        }

        /// <inheritdoc />
        public string? TitlePath { get; set; }

        /// <inheritdoc />
        public bool AutoRefresh
        {
            get => _binder.AutoRefresh;
            set => _binder.AutoRefresh = value;
        }

        /// <inheritdoc />
        public TimeSpan RefreshThrottle
        {
            get => _binder.RefreshThrottle;
            set => _binder.RefreshThrottle = value;
        }

        /// <inheritdoc />
        public void RefreshData()
        {
            _binder.RefreshNow();
        }

        /// <summary>
        /// Gets or sets the property path resolver used for data binding
        /// </summary>
        public IPropertyPathResolver PropertyPathResolver
        {
            get => _binder.PropertyPathResolver;
            set => _binder.PropertyPathResolver = value;
        }

        /// <inheritdoc />
        IReadOnlyList<BarPoint> IBarSeries.Data => (IReadOnlyList<BarPoint>)Data;

        /// <summary>
        /// Converts source items to points; string categories fall back to their index
        /// </summary>
        private IReadOnlyList<PointD> ConvertItems(IEnumerable items)
        {
            var result = items is ICollection collection ? new List<PointD>(collection.Count) : new List<PointD>();
            var resolver = _binder.PropertyPathResolver;
            var index = 0;

            foreach (var item in items)
            {
                if (item == null)
                {
                    index++;
                    continue;
                }

                var xValue = resolver.GetValue(item, XPath);
                var yValue = resolver.GetValue(item, YPath);

                // For bar series, if X is not numeric (e.g. a category string), use the item index
                var x = DataBindingConverter.IsValidCoordinate(xValue)
                    ? DataBindingConverter.ToDouble(xValue)
                    : index;

                if (DataBindingConverter.IsValidCoordinate(yValue))
                {
                    result.Add(new PointD(x, DataBindingConverter.ToDouble(yValue)));
                }

                index++;
            }

            return result;
        }

        private void ApplyPoints(IReadOnlyList<PointD> points)
        {
            Data.Clear();
            for (var i = 0; i < points.Count; i++)
            {
                Data.Add(new BarPoint(points[i].X, points[i].Y));
            }
        }

        /// <summary>
        /// Gets the data points for rendering
        /// </summary>
        /// <returns>Data points as bar points</returns>
        public IEnumerable<BarPoint> GetRenderPoints()
        {
            return Data;
        }

        /// <summary>
        /// Gets a human-readable summary of the series
        /// </summary>
        /// <returns>Series summary</returns>
        public override string ToString()
        {
            var sourceType = _binder.ItemsSource?.GetType().Name ?? "null";
            return $"ObservableBarSeries: {Data.Count} bars from {sourceType} (X: {XPath}, Y: {YPath})";
        }

        /// <summary>
        /// Releases data-binding subscriptions
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases data-binding subscriptions
        /// </summary>
        /// <param name="disposing">True when called from Dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _binder.Dispose();
            }
        }
    }
}
