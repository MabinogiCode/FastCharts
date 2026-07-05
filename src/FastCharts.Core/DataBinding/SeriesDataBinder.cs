using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Reusable data-binding engine for chart series.
    /// Watches an items source (INotifyCollectionChanged) and its items (INotifyPropertyChanged),
    /// converts items to points via X/Y property paths, and pushes refreshed point lists to the
    /// owning series. Honors <see cref="RefreshThrottle"/> to coalesce refresh storms.
    /// </summary>
    public sealed class SeriesDataBinder : IDisposable
    {
        private readonly Action<IReadOnlyList<PointD>> _applyPoints;
        private readonly Func<int> _getPointCount;
        private readonly Subject<int> _refreshRequests = new Subject<int>();
        private readonly List<INotifyPropertyChanged> _observedItems = new List<INotifyPropertyChanged>();

        private IScheduler _observableScheduler = DefaultScheduler.Instance;
        private IEnumerable? _itemsSource;
        private string? _xPath;
        private string? _yPath;
        private bool _autoRefresh = true;
        private TimeSpan _refreshThrottle = TimeSpan.Zero;
        private IDisposable? _collectionSubscription;
        private IDisposable? _throttleSubscription;
        private bool _disposed;

        /// <summary>
        /// Creates a binder.
        /// </summary>
        /// <param name="applyPoints">Callback that stores converted points into the series</param>
        /// <param name="getPointCount">Callback returning the series' current point count</param>
        public SeriesDataBinder(Action<IReadOnlyList<PointD>> applyPoints, Func<int> getPointCount)
        {
            _applyPoints = applyPoints ?? throw new ArgumentNullException(nameof(applyPoints));
            _getPointCount = getPointCount ?? throw new ArgumentNullException(nameof(getPointCount));
            SetupThrottlePipeline();
        }

        /// <summary>
        /// Property path resolver. Defaults to the cached compiled resolver.
        /// </summary>
        public IPropertyPathResolver PropertyPathResolver { get; set; } = CachedPropertyPathResolver.Instance;

        /// <summary>
        /// Scheduler used for throttled refreshes. Defaults to the platform timer scheduler;
        /// UI hosts can supply a dispatcher scheduler so refreshes land on the UI thread.
        /// Only used when <see cref="RefreshThrottle"/> is greater than zero.
        /// </summary>
        public IScheduler ObservableScheduler
        {
            get => _observableScheduler;
            set
            {
                if (ReferenceEquals(_observableScheduler, value) || value == null)
                {
                    return;
                }

                _observableScheduler = value;
                SetupThrottlePipeline();
            }
        }

        /// <summary>
        /// Raised after each refresh with delta information
        /// </summary>
        public event EventHandler<DataBindingUpdatedEventArgs>? DataBindingUpdated;

        /// <summary>
        /// Source collection. When it implements INotifyCollectionChanged, updates are automatic.
        /// </summary>
        public IEnumerable? ItemsSource
        {
            get => _itemsSource;
            set
            {
                if (ReferenceEquals(_itemsSource, value))
                {
                    return;
                }

                _itemsSource = value;
                SubscribeToSource();

                if (_autoRefresh)
                {
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Property path for X values (e.g. "Time" or "Sample.Timestamp")
        /// </summary>
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

                if (_autoRefresh)
                {
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Property path for Y values
        /// </summary>
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

                if (_autoRefresh)
                {
                    Refresh();
                }
            }
        }

        /// <summary>
        /// When true (default), source changes refresh the series automatically
        /// </summary>
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
                    SubscribeToSource();
                    Refresh();
                }
                else
                {
                    UnsubscribeFromSource();
                }
            }
        }

        /// <summary>
        /// Coalescing window for automatic refreshes. Zero (default) refreshes synchronously;
        /// a positive value batches rapid changes into a single refresh.
        /// </summary>
        public TimeSpan RefreshThrottle
        {
            get => _refreshThrottle;
            set
            {
                if (_refreshThrottle == value)
                {
                    return;
                }

                _refreshThrottle = value;
                SetupThrottlePipeline();
            }
        }

        /// <summary>
        /// Converts the current source and applies points to the series immediately.
        /// </summary>
        public void Refresh()
        {
            if (_refreshThrottle > TimeSpan.Zero)
            {
                _refreshRequests.OnNext(0);
            }
            else
            {
                RefreshNow();
            }
        }

        /// <summary>
        /// Forces a synchronous refresh, bypassing the throttle window.
        /// </summary>
        public void RefreshNow()
        {
            if (_disposed || _itemsSource == null)
            {
                return;
            }

            try
            {
                var points = ConvertToPoints(_itemsSource);
                var oldCount = _getPointCount();

                _applyPoints(points);

                var newCount = _getPointCount();
                DataBindingUpdated?.Invoke(this, new DataBindingUpdatedEventArgs
                {
                    ItemsAdded = Math.Max(0, newCount - oldCount),
                    ItemsRemoved = Math.Max(0, oldCount - newCount),
                    ItemsUpdated = 0,
                    TotalItems = newCount,
                    UpdateType = DataBindingUpdateType.FullRefresh
                });
            }
            catch (Exception ex)
            {
                // Data binding must stay resilient; log and keep previous data
                System.Diagnostics.Debug.WriteLine($"FastCharts data binding refresh failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Converter hook: turn source items into points. Assigned by the owning series
        /// when it needs custom conversion (e.g. bars with category X). When null, the
        /// default X/Y path conversion applies.
        /// </summary>
        public Func<IEnumerable, IReadOnlyList<PointD>>? CustomConverter { get; set; }

        private IReadOnlyList<PointD> ConvertToPoints(IEnumerable items)
        {
            if (CustomConverter != null)
            {
                return CustomConverter(items);
            }

            var result = items is ICollection collection ? new List<PointD>(collection.Count) : new List<PointD>();
            var resolver = PropertyPathResolver;

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                var xValue = resolver.GetValue(item, _xPath);
                var yValue = resolver.GetValue(item, _yPath);

                if (DataBindingConverter.IsValidCoordinate(xValue) && DataBindingConverter.IsValidCoordinate(yValue))
                {
                    result.Add(new PointD(DataBindingConverter.ToDouble(xValue), DataBindingConverter.ToDouble(yValue)));
                }
            }

            return result;
        }

        private void SetupThrottlePipeline()
        {
            _throttleSubscription?.Dispose();
            _throttleSubscription = null;

            if (_refreshThrottle > TimeSpan.Zero)
            {
                _throttleSubscription = _refreshRequests
                    .Throttle(_refreshThrottle, ObservableScheduler)
                    .Subscribe(_ => RefreshNow());
            }
        }

        private void SubscribeToSource()
        {
            UnsubscribeFromSource();

            if (!_autoRefresh || _itemsSource == null)
            {
                return;
            }

            if (_itemsSource is INotifyCollectionChanged notifyingCollection)
            {
                notifyingCollection.CollectionChanged += OnCollectionChanged;
                _collectionSubscription = Disposable.Create(() => notifyingCollection.CollectionChanged -= OnCollectionChanged);
            }

            AttachItemListeners();
        }

        private void UnsubscribeFromSource()
        {
            _collectionSubscription?.Dispose();
            _collectionSubscription = null;
            DetachItemListeners();
        }

        private void AttachItemListeners()
        {
            if (_itemsSource == null)
            {
                return;
            }

            foreach (var item in _itemsSource)
            {
                if (item is INotifyPropertyChanged notifying)
                {
                    notifying.PropertyChanged += OnItemPropertyChanged;
                    _observedItems.Add(notifying);
                }
            }
        }

        private void DetachItemListeners()
        {
            for (var i = 0; i < _observedItems.Count; i++)
            {
                _observedItems[i].PropertyChanged -= OnItemPropertyChanged;
            }

            _observedItems.Clear();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            // Keep per-item listeners in sync, including items added after the initial subscription
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AttachItems(args.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    DetachItems(args.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    DetachItems(args.OldItems);
                    AttachItems(args.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    DetachItemListeners();
                    AttachItemListeners();
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                default:
                    break;
            }

            if (_autoRefresh)
            {
                Refresh();
            }
        }

        private void AttachItems(IList? items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item is INotifyPropertyChanged notifying)
                {
                    notifying.PropertyChanged += OnItemPropertyChanged;
                    _observedItems.Add(notifying);
                }
            }
        }

        private void DetachItems(IList? items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item is INotifyPropertyChanged notifying)
                {
                    notifying.PropertyChanged -= OnItemPropertyChanged;
                    _observedItems.Remove(notifying);
                }
            }
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (!_autoRefresh)
            {
                return;
            }

            if (IsRelevantPropertyChange(args.PropertyName))
            {
                Refresh();
            }
        }

        private bool IsRelevantPropertyChange(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return true; // Assume relevant when unspecified
            }

            return IsPropertyInPath(propertyName!, _xPath) || IsPropertyInPath(propertyName!, _yPath);
        }

        private static bool IsPropertyInPath(string propertyName, string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path!.Split('.').Contains(propertyName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Detaches all source subscriptions
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            UnsubscribeFromSource();
            _throttleSubscription?.Dispose();
            _refreshRequests.Dispose();
        }
    }
}
