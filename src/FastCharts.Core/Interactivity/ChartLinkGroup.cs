using System;
using System.Collections.Generic;
using System.ComponentModel;

using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Interactivity
{
    /// <summary>
    /// Synchronizes the visible X range across several charts (P2-AX-LINK).
    /// The classic finance layout — price chart on top, indicator chart below —
    /// stays aligned under zoom and pan:
    /// <code>
    /// var link = new ChartLinkGroup();
    /// link.Add(priceModel);
    /// link.Add(indicatorModel);
    /// </code>
    /// Dispose the group (or Remove a model) to stop synchronizing.
    /// </summary>
    public sealed class ChartLinkGroup : IDisposable
    {
        private readonly List<Entry> _entries = new List<Entry>();
        private bool _updating;
        private bool _disposed;

        /// <summary>
        /// Number of linked charts
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// Adds a chart to the group. Its X axis immediately adopts the group's
        /// current visible range (from the first chart added, if any).
        /// </summary>
        /// <param name="model">Chart model to link</param>
        public void Add(ChartModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ChartLinkGroup));
            }

            for (var i = 0; i < _entries.Count; i++)
            {
                if (ReferenceEquals(_entries[i].Model, model))
                {
                    return; // Already linked
                }
            }

            var entry = new Entry(this, model);
            _entries.Add(entry);

            // Align the newcomer with the group's current range
            if (_entries.Count > 1)
            {
                Propagate(_entries[0].Model, _entries[0].CurrentAxis.VisibleRange);
            }
        }

        /// <summary>
        /// Removes a chart from the group and stops synchronizing it.
        /// </summary>
        /// <param name="model">Chart model to unlink</param>
        /// <returns>True when the chart was part of the group</returns>
        public bool Remove(ChartModel model)
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                if (ReferenceEquals(_entries[i].Model, model))
                {
                    _entries[i].Dispose();
                    _entries.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private void OnAxisRangeChanged(ChartModel source, AxisBase axis)
        {
            if (_updating)
            {
                return;
            }

            Propagate(source, axis.VisibleRange);
        }

        private void Propagate(ChartModel source, FRange range)
        {
            _updating = true;
            try
            {
                for (var i = 0; i < _entries.Count; i++)
                {
                    if (!ReferenceEquals(_entries[i].Model, source))
                    {
                        _entries[i].CurrentAxis.VisibleRange = range;
                    }
                }
            }
            finally
            {
                _updating = false;
            }
        }

        /// <summary>
        /// Unlinks all charts
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            for (var i = 0; i < _entries.Count; i++)
            {
                _entries[i].Dispose();
            }

            _entries.Clear();
        }

        /// <summary>
        /// Tracks one linked chart: follows its current X axis, including axis replacement
        /// (e.g. switching to a logarithmic axis re-hooks the subscription automatically).
        /// </summary>
        private sealed class Entry : IDisposable
        {
            private readonly ChartLinkGroup _group;

            public Entry(ChartLinkGroup group, ChartModel model)
            {
                _group = group;
                Model = model;
                CurrentAxis = (AxisBase)model.XAxis;
                CurrentAxis.VisibleRangeChanged += OnRangeChanged;
                Model.PropertyChanged += OnModelPropertyChanged;
            }

            public ChartModel Model { get; }

            public AxisBase CurrentAxis { get; private set; }

            private void OnRangeChanged(object? sender, EventArgs e)
            {
                _group.OnAxisRangeChanged(Model, CurrentAxis);
            }

            private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != nameof(ChartModel.XAxis))
                {
                    return;
                }

                // X axis instance was replaced: move the subscription
                CurrentAxis.VisibleRangeChanged -= OnRangeChanged;
                CurrentAxis = (AxisBase)Model.XAxis;
                CurrentAxis.VisibleRangeChanged += OnRangeChanged;
            }

            public void Dispose()
            {
                CurrentAxis.VisibleRangeChanged -= OnRangeChanged;
                Model.PropertyChanged -= OnModelPropertyChanged;
            }
        }
    }
}
