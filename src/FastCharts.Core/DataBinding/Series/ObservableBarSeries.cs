using System;
using System.Collections.Generic;
using FastCharts.Core.DataBinding;
using FastCharts.Core.Series;

namespace FastCharts.Core.DataBinding.Series
{
    /// <summary>
    /// Bar series that binds its bars to a source collection through property paths.
    /// It is a <see cref="BarSeries"/>, so it can be added to a chart and rendered
    /// like any other bar series. Items whose X value is not numeric (e.g. string
    /// categories) are placed at their zero-based index.
    /// </summary>
    public sealed class ObservableBarSeries : BarSeries, IObservableSeries<object>, IDisposable
    {
        private readonly SeriesDataBinder _binder;
        private bool _disposed;

        /// <summary>Initializes a new empty observable bar series.</summary>
        public ObservableBarSeries()
        {
            _binder = new SeriesDataBinder(RefreshData);
        }

        /// <summary>
        /// Initializes a new observable bar series bound to a source collection.
        /// </summary>
        /// <param name="itemsSource">Source collection.</param>
        /// <param name="xPath">Property path for X values (categories or numbers).</param>
        /// <param name="yPath">Property path for Y values (bar heights).</param>
        public ObservableBarSeries(IEnumerable<object> itemsSource, string xPath, string yPath)
            : this()
        {
            _binder.XPath = xPath;
            _binder.YPath = yPath;
            _binder.ItemsSource = itemsSource;
        }

        /// <inheritdoc />
        public IEnumerable<object> ItemsSource
        {
            get => _binder.ItemsSource;
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
        public string? TitlePath
        {
            get => _binder.TitlePath;
            set => _binder.TitlePath = value;
        }

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
        public event EventHandler<DataBindingUpdatedEventArgs>? DataBindingUpdated;

        /// <inheritdoc />
        public void RefreshData()
        {
            var before = Data.Count;
            var points = _binder.Project(useIndexForInvalidX: true);

            Data.Clear();
            foreach (var point in points)
            {
                Data.Add(new BarPoint(point.X, point.Y));
            }

            DataBindingUpdated?.Invoke(this, new DataBindingUpdatedEventArgs
            {
                ItemsAdded = Math.Max(0, Data.Count - before),
                ItemsRemoved = Math.Max(0, before - Data.Count),
                TotalItems = Data.Count,
                UpdateType = DataBindingUpdateType.FullRefresh
            });
        }

        /// <summary>Releases the source-collection subscriptions held by this series.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _binder.Dispose();
        }
    }
}
