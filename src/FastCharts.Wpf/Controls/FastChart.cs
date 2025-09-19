using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using FastCharts.Core;

namespace FastCharts.Wpf.Controls
{

    public class FastChart : Control
    {
        static FastChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FastChart),
                new FrameworkPropertyMetadata(typeof(FastChart)));
        }

        public FastChart()
        {
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        public ChartModel? Model
        {
            get => (ChartModel?)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register(
                nameof(Model),
                typeof(ChartModel),
                typeof(FastChart),
                new FrameworkPropertyMetadata(
                    default(ChartModel),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnModelChanged));

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (FastChart)d;
            ctrl.DetachFromModel(e.OldValue as ChartModel);
            ctrl.AttachToModel(e.NewValue as ChartModel);
            ctrl.RefreshScalesAndRedraw();
        }

        private void OnLoaded(object? sender, RoutedEventArgs e) => RefreshScalesAndRedraw();

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => RefreshScalesAndRedraw();

        private void RefreshScalesAndRedraw()
        {
            if (Model is null)
            {
                InvalidateVisual();
                return;
            }

            var w = ActualWidth;
            var h = ActualHeight;

            // Guard during initial measure/arrange
            if (double.IsNaN(w) || double.IsNaN(h) || w <= 0 || h <= 0)
            {
                InvalidateVisual();
                return;
            }

            // Ensure ranges & scales are consistent with current size
            Model.AutoFitDataRange();
            Model.UpdateScales(w, h);

            InvalidateVisual(); // renderer will hook here in a later step
        }

        private void AttachToModel(ChartModel? model)
        {
            if (model is null) return;
            model.Series.CollectionChanged += OnSeriesCollectionChanged;
        }

        private void DetachFromModel(ChartModel? model)
        {
            if (model is null) return;
            model.Series.CollectionChanged -= OnSeriesCollectionChanged;
        }

        private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (Model is null) return;
            Model.AutoFitDataRange();
            RefreshScalesAndRedraw();
        }
    }
}
