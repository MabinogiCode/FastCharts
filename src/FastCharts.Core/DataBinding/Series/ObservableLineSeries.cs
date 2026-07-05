using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Core.DataBinding.Series
{
    /// <summary>
    /// Line series with observable data binding (MVVM).
    /// Inherits <see cref="LineSeries"/>, so it renders, resamples (LTTB) and participates in
    /// range calculation like any other series while staying synchronized with its
    /// <see cref="ItemsSource"/> through X/Y property paths.
    /// </summary>
    public class ObservableLineSeries : LineSeries, IObservableSeries<object>, ILineSeries, IDisposable
    {
        private readonly SeriesDataBinder _binder;
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
        /// Initializes a new instance of the ObservableLineSeries class
        /// </summary>
        public ObservableLineSeries()
        {
            _binder = new SeriesDataBinder(ReplacePoints, () => Data.Count);
            _binder.DataBindingUpdated += (_, args) => DataBindingUpdated?.Invoke(this, args);
        }

        /// <summary>
        /// Initializes a new instance of the ObservableLineSeries class with data source
        /// </summary>
        /// <param name="itemsSource">Data source</param>
        /// <param name="xPath">Property path for X values</param>
        /// <param name="yPath">Property path for Y values</param>
        public ObservableLineSeries(IEnumerable<object> itemsSource, string xPath, string yPath)
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
        IReadOnlyList<PointD> ILineSeries.Data => (IReadOnlyList<PointD>)Data;

        /// <summary>
        /// Gets the data points for rendering
        /// </summary>
        /// <returns>Data points</returns>
        public IEnumerable<PointD> GetRenderPoints()
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
            return $"ObservableLineSeries: {Data.Count} points from {sourceType} (X: {XPath}, Y: {YPath})";
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
