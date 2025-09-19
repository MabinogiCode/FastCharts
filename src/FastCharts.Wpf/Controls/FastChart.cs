using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FastCharts.Core;
using FastCharts.Core.Series;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace FastCharts.Wpf.Controls
{

    [TemplatePart(Name = "PART_Skia", Type = typeof(SKElement))]
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
            Unloaded += OnUnloaded;
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

        private SKElement? _skia;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_skia != null)
            {
                _skia.PaintSurface -= OnSkiaPaint;
            }

            _skia = GetTemplateChild("PART_Skia") as SKElement;
            if (_skia != null)
            {
                _skia.PaintSurface += OnSkiaPaint;
                _skia.IgnorePixelScaling = false; // respect DPI
            }

            RefreshScalesAndRedraw();
        }

        private void OnLoaded(object? sender, RoutedEventArgs e) => RefreshScalesAndRedraw();

        private void OnUnloaded(object? sender, RoutedEventArgs e)
        {
            if (_skia != null)
            {
                _skia.PaintSurface -= OnSkiaPaint;
            }

            DetachFromModel(Model);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => RefreshScalesAndRedraw();

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

        private void RefreshScalesAndRedraw()
        {
            if (Model is null)
            {
                InvalidateVisual();
                _skia?.InvalidateVisual();
                return;
            }

            var w = ActualWidth;
            var h = ActualHeight;
            if (double.IsNaN(w) || double.IsNaN(h) || w <= 0 || h <= 0)
            {
                InvalidateVisual();
                _skia?.InvalidateVisual();
                return;
            }

            // Keep scales in sync with current layout size (DIPs).
            Model.UpdateScales(w, h);

            // Ask Skia to repaint
            _skia?.InvalidateVisual();
        }

        private void OnSkiaPaint(object? sender, SKPaintSurfaceEventArgs e)
        {
            // Canvas in device pixels:
            var canvas = e.Surface.Canvas;
            var widthPx = e.Info.Width;
            var heightPx = e.Info.Height;

            // (Optional) Re-sync scales to exact pixel size to avoid off-by-DPI effects.
            if (Model != null)
                Model.UpdateScales(widthPx, heightPx);

            // Clear background (transparent → Border’s Background shows through).
            canvas.Clear(SKColors.Transparent);

            if (Model == null) return;

            // Draw the first LineSeries, if any.
            var ls = Model.Series.OfType<LineSeries>().FirstOrDefault();
            if (ls == null || ls.IsEmpty) return;

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (float)ls.StrokeThickness,
                Color = new SKColor(0x33, 0x99, 0xFF) // minimal color; we’ll theme later
            };

            using var path = new SKPath();
            bool started = false;

            foreach (var p in ls.Data)
            {
                var x = (float)Model.XAxis.Scale.ToPixels(p.X);
                var y = (float)Model.YAxis.Scale.ToPixels(p.Y);
                if (!started)
                {
                    path.MoveTo(x, y);
                    started = true;
                }
                else
                {
                    path.LineTo(x, y);
                }
            }

            canvas.DrawPath(path, paint);
        }
    }
}
