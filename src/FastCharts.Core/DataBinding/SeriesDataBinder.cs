using FastCharts.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Watches a bound source collection (and, when items implement
    /// <see cref="INotifyPropertyChanged"/>, their property changes) and invokes a
    /// refresh callback so an owning series can re-project the source into chart
    /// points. This composition replaces the former <c>ObservableSeriesBase</c>
    /// inheritance, which let observable series derive from the concrete series
    /// types and therefore be added to a chart and rendered like any other series.
    /// </summary>
    public sealed class SeriesDataBinder : IDisposable
    {
        private readonly Action _refresh;
        private readonly List<INotifyPropertyChanged> _itemSubscriptions = new();
        private IEnumerable<object>? _itemsSource;
        private INotifyCollectionChanged? _collectionSource;
        private string? _xPath;
        private string? _yPath;
        private string? _titlePath;
        private bool _autoRefresh = true;
        private TimeSpan _refreshThrottle = TimeSpan.FromMilliseconds(100);
        private bool _disposed;

        /// <summary>
        /// Initializes a new binder.
        /// </summary>
        /// <param name="refresh">Callback invoked when the bound data should be re-projected.</param>
        public SeriesDataBinder(Action refresh)
        {
            _refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
        }

        /// <summary>
        /// Resolver used to read values from source items by property path.
        /// </summary>
        public IPropertyPathResolver PropertyPathResolver { get; set; } = ReflectionPropertyPathResolver.Instance;

        /// <summary>
        /// Source collection bound to the series.
        /// </summary>
        public IEnumerable<object> ItemsSource
        {
            get => _itemsSource ?? Enumerable.Empty<object>();
            set
            {
                if (ReferenceEquals(_itemsSource, value))
                {
                    return;
                }

                _itemsSource = value;
                Subscribe();
                RequestRefresh();
            }
        }

        /// <summary>Property path for X values.</summary>
        public string? XPath
        {
            get => _xPath;
            set
            {
                if (_xPath == value)
                {
                    return;
                }

                _xPath = value;
                RequestRefresh();
            }
        }

        /// <summary>Property path for Y values.</summary>
        public string? YPath
        {
            get => _yPath;
            set
            {
                if (_yPath == value)
                {
                    return;
                }

                _yPath = value;
                RequestRefresh();
            }
        }

        /// <summary>Property path for item titles/labels.</summary>
        public string? TitlePath
        {
            get => _titlePath;
            set
            {
                if (_titlePath == value)
                {
                    return;
                }

                _titlePath = value;
                RequestRefresh();
            }
        }

        /// <summary>Whether the series refreshes automatically when the source changes.</summary>
        public bool AutoRefresh
        {
            get => _autoRefresh;
            set
            {
                if (_autoRefresh == value)
                {
                    return;
                }

                _autoRefresh = value;
                if (value)
                {
                    Subscribe();
                    RequestRefresh();
                }
                else
                {
                    Unsubscribe();
                }
            }
        }

        /// <summary>
        /// Refresh throttle interval. Stored for API compatibility; refreshes are
        /// applied immediately so manual and test-driven updates stay deterministic.
        /// </summary>
        public TimeSpan GetRefreshThrottle()
        {
            return _refreshThrottle;
        }

        public void SetRefreshThrottle(TimeSpan value)
        {
            _refreshThrottle = value;
        }

        /// <summary>
        /// Projects the bound source into chart points using the configured paths.
        /// </summary>
        /// <param name="useIndexForInvalidX">
        /// When true, items whose X value is not a numeric coordinate (e.g. string
        /// categories) are placed at their zero-based index. When false such items
        /// are skipped. Bar series use index placement; line/scatter series do not.
        /// </param>
        public List<PointD> Project(bool useIndexForInvalidX)
        {
            var result = new List<PointD>();
            var index = 0;

            foreach (var item in ItemsSource)
            {
                var xRaw = GetCoordinateValue(item, _xPath, useIndexForInvalidX ? (object)index : 0.0);
                var yRaw = GetCoordinateValue(item, _yPath, 0.0);

                double x;
                if (DataBindingConverter.IsValidCoordinate(xRaw))
                {
                    x = DataBindingConverter.ToDouble(xRaw);
                }
                else if (useIndexForInvalidX)
                {
                    x = index;
                }
                else
                {
                    index++;
                    continue;
                }

                if (DataBindingConverter.IsValidCoordinate(yRaw))
                {
                    result.Add(new PointD(x, DataBindingConverter.ToDouble(yRaw)));
                }

                index++;
            }

            return result;
        }

        /// <summary>
        /// Reads a value from a source item using a property path.
        /// </summary>
        public object? GetCoordinateValue(object? item, string? path, object? defaultValue)
        {
            if (item is null || string.IsNullOrEmpty(path))
            {
                return defaultValue;
            }

            return PropertyPathResolver.GetValue(item, path) ?? defaultValue;
        }

        private void RequestRefresh()
        {
            if (_autoRefresh && !_disposed)
            {
                _refresh();
            }
        }

        private void Subscribe()
        {
            Unsubscribe();

            if (!_autoRefresh || _itemsSource == null)
            {
                return;
            }

            if (_itemsSource is INotifyCollectionChanged collection)
            {
                _collectionSource = collection;
                collection.CollectionChanged += OnCollectionChanged;
            }

            SubscribeItems();
        }

        private void SubscribeItems()
        {
            foreach (var item in _itemSubscriptions)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }

            _itemSubscriptions.Clear();

            if (!_autoRefresh || _itemsSource == null)
            {
                return;
            }

            foreach (var item in _itemsSource.OfType<INotifyPropertyChanged>())
            {
                item.PropertyChanged += OnItemPropertyChanged;
                _itemSubscriptions.Add(item);
            }
        }

        private void Unsubscribe()
        {
            if (_collectionSource != null)
            {
                _collectionSource.CollectionChanged -= OnCollectionChanged;
                _collectionSource = null;
            }

            foreach (var item in _itemSubscriptions)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }

            _itemSubscriptions.Clear();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SubscribeItems();
            RequestRefresh();
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RequestRefresh();
        }

        /// <summary>
        /// Releases all source subscriptions.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Unsubscribe();
        }
    }
}
