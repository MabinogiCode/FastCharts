using System;
using System.ComponentModel;
using FastCharts.Core.Axes;

namespace FastCharts.Core.Interactivity
{
    /// <summary>
    /// Tracks one linked chart: follows its current X axis, including axis replacement
    /// (e.g. switching to a logarithmic axis re-hooks the subscription automatically).
    /// </summary>
    internal sealed class ChartLinkEntry : IDisposable
    {
        private readonly ChartLinkGroup _group;

        public ChartLinkEntry(ChartLinkGroup group, ChartModel model)
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
