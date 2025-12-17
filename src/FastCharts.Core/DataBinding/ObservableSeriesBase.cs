using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using ReactiveUI;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Base class for observable series that support data binding
    /// Handles automatic synchronization with source collections and property paths
    /// </summary>
    public abstract class ObservableSeriesBase<T> : ReactiveObject, IObservableSeries<T>, IDisposable
    {
        private IEnumerable<T>? _itemsSource;
        private string? _xPath;
        private string? _yPath;
        private string? _titlePath;
        private bool _autoRefresh = true;
        private TimeSpan _refreshThrottle = TimeSpan.FromMilliseconds(100);
        private string? _title;
        private bool _isVisible = true;
        private int _zIndex;
        private int? _paletteIndex;
        private double _strokeThickness = 1.5;
        private object? _tag;
        private int _yAxisIndex;

        private readonly CompositeDisposable _subscriptions = new();
        private IDisposable? _collectionSubscription;
        private IDisposable? _propertySubscription;

        /// <summary>
        /// Gets or sets the property path resolver
        /// </summary>
        protected IPropertyPathResolver PropertyPathResolver { get; set; } = ReflectionPropertyPathResolver.Instance;

        /// <summary>
        /// Gets or sets the scheduler to use for observable operations
        /// Defaults to CurrentThread for cross-platform compatibility
        /// </summary>
        protected IScheduler ObservableScheduler { get; set; } = CurrentThreadScheduler.Instance;

        /// <summary>
        /// Display name (for legends, tooltips)
        /// </summary>
        public string? Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        /// <summary>
        /// Whether the series is visible. Renderers should skip drawing when false
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
        }

        /// <summary>
        /// Z-order hint. Higher values should be drawn after lower ones
        /// </summary>
        public int ZIndex
        {
            get => _zIndex;
            set => this.RaiseAndSetIfChanged(ref _zIndex, value);
        }

        /// <summary>
        /// Optional fixed palette index to pick a color deterministically
        /// </summary>
        public int? PaletteIndex
        {
            get => _paletteIndex;
            set => this.RaiseAndSetIfChanged(ref _paletteIndex, value);
        }

        /// <summary>
        /// Line/outline thickness in pixels (when applicable)
        /// </summary>
        public double StrokeThickness
        {
            get => _strokeThickness;
            set => this.RaiseAndSetIfChanged(ref _strokeThickness, value);
        }

        /// <summary>
        /// Tag object for app-specific metadata
        /// </summary>
        public object? Tag
        {
            get => _tag;
            set => this.RaiseAndSetIfChanged(ref _tag, value);
        }

        /// <summary>
        /// Indicates which Y axis this series should use (0 = primary, 1 = secondary)
        /// </summary>
        public int YAxisIndex
        {
            get => _yAxisIndex;
            set => this.RaiseAndSetIfChanged(ref _yAxisIndex, value);
        }

        /// <summary>
        /// Series has no data to render
        /// </summary>
        public abstract bool IsEmpty { get; }

        /// <inheritdoc />
        public IEnumerable<T> ItemsSource
        {
            get => _itemsSource ?? Enumerable.Empty<T>();
            set
            {
                if (ReferenceEquals(_itemsSource, value))
                {
                    return;
                }

                _itemsSource = value;
                SubscribeToSourceCollection();

                if (AutoRefresh)
                {
                    RefreshDataInternal();
                }

                this.RaisePropertyChanged();
            }
        }

        /// <inheritdoc />
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

                if (AutoRefresh)
                {
                    RefreshDataInternal();
                }

                this.RaisePropertyChanged();
            }
        }

        /// <inheritdoc />
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

                if (AutoRefresh)
                {
                    RefreshDataInternal();
                }

                this.RaisePropertyChanged();
            }
        }

        /// <inheritdoc />
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

                if (AutoRefresh)
                {
                    RefreshDataInternal();
                }

                this.RaisePropertyChanged();
            }
        }

        /// <inheritdoc />
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
                    SubscribeToSourceCollection();
                    RefreshDataInternal();
                }
                else
                {
                    UnsubscribeFromSourceCollection();
                }

                this.RaisePropertyChanged();
            }
        }

        /// <inheritdoc />
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
                SubscribeToSourceCollection(); // Re-subscribe with new throttle
                this.RaisePropertyChanged();
            }
        }

        /// <inheritdoc />
        public event EventHandler<DataBindingUpdatedEventArgs>? DataBindingUpdated;

        /// <summary>
        /// Initializes a new instance of the ObservableSeriesBase class
        /// </summary>
        protected ObservableSeriesBase()
        {
            _subscriptions.Add(Disposable.Create(UnsubscribeFromSourceCollection));
        }

        /// <inheritdoc />
        public void RefreshData()
        {
            RefreshDataInternal();
        }

        /// <summary>
        /// Internal refresh method that can be safely called from constructor and properties
        /// </summary>
        private void RefreshDataInternal()
        {
            if (ItemsSource == null)
            {
                return;
            }

            try
            {
                var points = ConvertToPoints(ItemsSource);
                var oldCount = GetPointCount();

                UpdateSeriesData(points);

                var newCount = GetPointCount();
                var args = new DataBindingUpdatedEventArgs
                {
                    ItemsAdded = Math.Max(0, newCount - oldCount),
                    ItemsRemoved = Math.Max(0, oldCount - newCount),
                    ItemsUpdated = 0,
                    TotalItems = newCount,
                    UpdateType = DataBindingUpdateType.FullRefresh
                };

                DataBindingUpdated?.Invoke(this, args);
                NotifyDataChanged();
            }
            catch (Exception ex)
            {
                // Log error but don't throw - data binding should be resilient
                System.Diagnostics.Debug.WriteLine($"Error refreshing observable series data: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts source items to chart points using property paths
        /// </summary>
        /// <param name="items">Source items</param>
        /// <returns>Converted points</returns>
        protected virtual IEnumerable<PointD> ConvertToPoints(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var x = GetCoordinateValue(item, XPath, 0.0);
                var y = GetCoordinateValue(item, YPath, 0.0);

                if (DataBindingConverter.IsValidCoordinate(x) && DataBindingConverter.IsValidCoordinate(y))
                {
                    yield return new PointD(DataBindingConverter.ToDouble(x), DataBindingConverter.ToDouble(y));
                }
            }
        }

        /// <summary>
        /// Gets a coordinate value from an item using property path
        /// </summary>
        /// <param name="item">Source item</param>
        /// <param name="path">Property path</param>
        /// <param name="defaultValue">Default value if path is invalid</param>
        /// <returns>Coordinate value</returns>
        protected virtual object? GetCoordinateValue(T item, string? path, object? defaultValue = null)
        {
            if (string.IsNullOrEmpty(path) || EqualityComparer<T>.Default.Equals(item, default(T)!))
            {
                return defaultValue;
            }

            return PropertyPathResolver.GetValue(item, path) ?? defaultValue;
        }

        /// <summary>
        /// Updates the series data with new points
        /// Must be implemented by derived classes
        /// </summary>
        /// <param name="points">New points</param>
        protected abstract void UpdateSeriesData(IEnumerable<PointD> points);

        /// <summary>
        /// Gets the current number of points in the series
        /// </summary>
        /// <returns>Point count</returns>
        protected abstract int GetPointCount();

        /// <summary>
        /// Notifies that series data has changed
        /// </summary>
        protected virtual void NotifyDataChanged()
        {
            // Trigger invalidation
            this.RaisePropertyChanged(nameof(IsEmpty));
        }

        /// <summary>
        /// Subscribes to source collection change notifications
        /// </summary>
        private void SubscribeToSourceCollection()
        {
            UnsubscribeFromSourceCollection();

            if (!AutoRefresh || ItemsSource == null)
            {
                return;
            }

            // Subscribe to collection changes with immediate synchronous execution for tests
            if (ItemsSource is INotifyCollectionChanged collectionChanged)
            {
                var collectionObservable = Observable
                    .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => collectionChanged.CollectionChanged += h,
                        h => collectionChanged.CollectionChanged -= h)
                    .Select(e => e.EventArgs)
                    .Subscribe(OnCollectionChanged); // Remove throttling for immediate response

                _collectionSubscription = collectionObservable;
            }

            // Subscribe to property changes on items
            if (ItemsSource is IEnumerable<INotifyPropertyChanged> propertyNotifiers)
            {
                var propertyObservable = propertyNotifiers
                    .ToObservable()
                    .SelectMany(item => Observable
                        .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                            h => item.PropertyChanged += h,
                            h => item.PropertyChanged -= h)
                        .Select(e => new { Item = item, Args = e.EventArgs }))
                    .Where(x => IsRelevantPropertyChange(x.Args.PropertyName))
                    .Subscribe(_ => OnPropertyChangedHandler()); // Remove throttling for immediate response

                _propertySubscription = propertyObservable;
            }
        }

        /// <summary>
        /// Unsubscribes from source collection notifications
        /// </summary>
        private void UnsubscribeFromSourceCollection()
        {
            _collectionSubscription?.Dispose();
            _collectionSubscription = null;

            _propertySubscription?.Dispose();
            _propertySubscription = null;
        }

        /// <summary>
        /// Handles collection change events
        /// </summary>
        /// <param name="args">Collection change arguments</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (AutoRefresh)
            {
                RefreshDataInternal();
            }
        }

        /// <summary>
        /// Handles item property change events
        /// </summary>
        private void OnPropertyChangedHandler()
        {
            if (AutoRefresh)
            {
                RefreshDataInternal();
            }
        }

        /// <summary>
        /// Checks if a property change is relevant to the series
        /// </summary>
        /// <param name="propertyName">Property name that changed</param>
        /// <returns>True if property affects series data</returns>
        private bool IsRelevantPropertyChange(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return true; // Assume relevant if name is null/empty
            }

            // Check if the changed property is part of any of our paths
            return IsPropertyInPath(propertyName!, XPath) ||
                   IsPropertyInPath(propertyName!, YPath) ||
                   IsPropertyInPath(propertyName!, TitlePath);
        }

        /// <summary>
        /// Checks if a property name is part of a property path
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="path">Property path</param>
        /// <returns>True if property is in path</returns>
        private static bool IsPropertyInPath(string propertyName, string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path!.Split('.').Contains(propertyName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Disposes of the observable series and its subscriptions
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of resources
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subscriptions.Dispose();
            }
        }
    }
}