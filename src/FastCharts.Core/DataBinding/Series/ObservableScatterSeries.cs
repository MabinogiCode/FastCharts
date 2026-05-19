using System;
using System.Collections.Generic;
using FastCharts.Core.DataBinding;
using FastCharts.Core.Series;

namespace FastCharts.Core.DataBinding.Series
{
    /// <summary>
    /// Scatter series that binds its points to a source collection through property
    /// paths. It is a <see cref="ScatterSeries"/>, so it can be added to a chart and
    /// rendered like any other scatter series.
    /// </summary>
    public sealed class ObservableScatterSeries : ScatterSeries, IObservableSeries<object>, IDisposable
    {
        private readonly SeriesDataBinder _binder;
        private bool _disposed;

        /// <summary>Initializes a new empty observable scatter series.</summary>
        public ObservableScatterSeries()
        {
            _binder = new SeriesDataBinder(RefreshData);
        }

        /// <summary>
        /// Initializes a new observable scatter series bound to a source collection.
        /// </summary>
        /// <param name="itemsSource">Source collection.</param>
        /// <param name="xPath">Property path for X values.</param>
        /// <param name="yPath">Property path for Y values.</param>
        public ObservableScatterSeries(IEnumerable<object> itemsSource, string xPath, string yPath)
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
            get => _binder.GetRefreshThrottle();
            set => _binder.SetRefreshThrottle(value);
        }

        /// <inheritdoc />
        public event EventHandler<DataBindingUpdatedEventArgs>? DataBindingUpdated;

        /// <inheritdoc />
        public void RefreshData()
        {
            var before = Data.Count;
            var points = _binder.Project(useIndexForInvalidX: false);

            Data.Clear();
            foreach (var point in points)
            {
                Data.Add(point);
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
