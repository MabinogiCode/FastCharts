using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using FastCharts.Core;
using FastCharts.Core.Abstractions;

using SkiaSharp.Views.WPF;
using FastCharts.Rendering.Skia;        // NEW

namespace FastCharts.Wpf.Controls
{
    [TemplatePart(Name = "PART_Skia", Type = typeof(SKElement))]
    public class FastChart : Control
    {
        // NEW: depend on abstraction
        public IRenderer<SkiaSharp.SKCanvas> Renderer { get; set; } = new SkiaChartRenderer();
        
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
            Unloaded += OnUnloaded;
        }

        public ChartModel? Model
        {
            get { return (ChartModel?)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
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

        private SKElement _skia;
        private readonly SkiaChartRenderer _renderer = new SkiaChartRenderer(); // NEW

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_skia != null)
                _skia.PaintSurface -= OnSkiaPaint;

            _skia = GetTemplateChild("PART_Skia") as SKElement;

            if (_skia != null)
            {
                _skia.IgnorePixelScaling = false; // respect DPI
                _skia.PaintSurface += OnSkiaPaint;
            }

            RefreshScalesAndRedraw();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) { RefreshScalesAndRedraw(); }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_skia != null)
                _skia.PaintSurface -= OnSkiaPaint;

            DetachFromModel(Model);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) { RefreshScalesAndRedraw(); }

        private void AttachToModel(ChartModel model)
        {
            if (model == null) return;
            model.Series.CollectionChanged += OnSeriesCollectionChanged;
        }

        private void DetachFromModel(ChartModel model)
        {
            if (model == null) return;
            model.Series.CollectionChanged -= OnSeriesCollectionChanged;
        }

        private void OnSeriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Model == null) return;
            Model.AutoFitDataRange();
            RefreshScalesAndRedraw();
        }

        private void RefreshScalesAndRedraw()
        {
            if (Model != null)
            {
                // Ensure ranges and scales are sane before painting
                Model.AutoFitDataRange();
                var w = ActualWidth;
                var h = ActualHeight;
                if (!double.IsNaN(w) && !double.IsNaN(h) && w > 0 && h > 0)
                    Model.UpdateScales(w, h);
            }

            _skia?.InvalidateVisual();
            InvalidateVisual();
        }

        private void OnSkiaPaint(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (Model == null) { e.Surface.Canvas.Clear(); return; }
            Renderer.Render(Model, e.Surface.Canvas, e.Info.Width, e.Info.Height); // via interface
        }
    }
}
