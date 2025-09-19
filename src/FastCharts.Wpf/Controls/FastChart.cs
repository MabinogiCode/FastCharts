using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using FastCharts.Core;
using FastCharts.Core.Abstractions;

using SkiaSharp.Views.WPF;

using FastCharts.Rendering.Skia;

namespace FastCharts.Wpf.Controls
{
    [TemplatePart(Name = "PART_Skia", Type = typeof(SKElement))]
    public class FastChart : Control
    {
        
        private bool _userChangedView;
        
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
            ctrl._userChangedView = false;                     // reset user state
            if (ctrl.Model != null) ctrl.Model.AutoFitDataRange();  // fit once on attach
            ctrl.RefreshScalesAndRedraw();
        }

        private SKElement _skia;

        private bool _isPanning;
        private Point _lastMousePos;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_skia != null)
            {
                _skia.PaintSurface -= OnSkiaPaint;
                _skia.MouseWheel -= OnSkiaMouseWheel;
                _skia.MouseDown -= OnSkiaMouseDown;
                _skia.MouseMove -= OnSkiaMouseMove;
                _skia.MouseUp -= OnSkiaMouseUp;
                _skia.MouseLeave -= OnSkiaMouseLeave;
            }

            _skia = GetTemplateChild("PART_Skia") as SKElement;

            if (_skia != null)
            {
                _skia.IgnorePixelScaling = false;
                _skia.PaintSurface += OnSkiaPaint;

                // interactivity
                _skia.MouseWheel += OnSkiaMouseWheel;
                _skia.MouseDown += OnSkiaMouseDown;
                _skia.MouseMove += OnSkiaMouseMove;
                _skia.MouseUp += OnSkiaMouseUp;
                _skia.MouseLeave += OnSkiaMouseLeave;
                
                // Make sure SKElement can receive focus so it gets wheel events
                _skia.Focusable = true;
                _skia.Focus();
            }
            
            // Also listen at the control level to catch wheel even if SKElement misses it
            this.PreviewMouseWheel += OnSkiaMouseWheel;

            RefreshScalesAndRedraw();
        }

        private void OnSkiaMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Model == null) return;

            if (e.ClickCount == 2)             // optional: double-click to reset
            {
                ResetView();
                e.Handled = true;
                return;
            }

            if (e.ChangedButton != System.Windows.Input.MouseButton.Left) return;
            _userChangedView = true;           
            _isPanning = true;
            _lastMousePos = e.GetPosition(_skia);
            _skia.CaptureMouse();
        }

        private void OnSkiaMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                _isPanning = false;
                _skia.ReleaseMouseCapture();
            }
        }
        
        public void ResetView()
        {
            if (Model == null) return;
            _userChangedView = false;
            Model.AutoFitDataRange();
            RefreshScalesAndRedraw();
        }

        private void OnSkiaMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isPanning = false;
            _skia.ReleaseMouseCapture();
        }

        private void OnSkiaMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isPanning || Model == null) return;

            var pos = e.GetPosition(_skia);
            var dx = pos.X - _lastMousePos.X;
            var dy = pos.Y - _lastMousePos.Y;
            _lastMousePos = pos;

            // convert pixel delta -> data delta using current visible ranges
            var m = Model.PlotMargins;
            var plotW = Math.Max(1.0, _skia.ActualWidth  - (m.Left + m.Right));
            var plotH = Math.Max(1.0, _skia.ActualHeight - (m.Top  + m.Bottom));
            var xr = Model.XAxis.VisibleRange;
            var yr = Model.YAxis.VisibleRange;

            var deltaDataX = -dx * (xr.Size / plotW); // drag right -> pan right visually
            var deltaDataY = dy * (yr.Size / plotH); // pixel Y grows downward

            Model.Pan(deltaDataX, deltaDataY);
            RefreshScalesAndRedraw();
        }

        private void OnSkiaMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (Model == null) return;

            _userChangedView = true;           // <-- mark user interaction
            var step = e.Delta > 0 ? 0.9 : 1.1;

            // get plot-relative pixel// MouseWheel (zoom)
            var m = Model.PlotMargins;
            var px = e.GetPosition(_skia).X - m.Left;
            var py = e.GetPosition(_skia).Y - m.Top;
            var plotW = Math.Max(1.0, _skia.ActualWidth  - (m.Left + m.Right));
            var plotH = Math.Max(1.0, _skia.ActualHeight - (m.Top  + m.Bottom));
            px = Math.Max(0, Math.Min(plotW, px));
            py = Math.Max(0, Math.Min(plotH, py));

            // clamp to plot
            px = Math.Max(0, Math.Min(plotW, px));
            py = Math.Max(0, Math.Min(plotH, py));

            // pixel -> data conversion via visible ranges
            var xr = Model.XAxis.VisibleRange;
            var yr = Model.YAxis.VisibleRange;

            var centerDataX = xr.Min + (px / plotW) * xr.Size;
            var centerDataY = yr.Max - (py / plotH) * yr.Size; // top->max mapping

            // modifiers: Shift = X only, Alt = Y only, Ctrl = both
            bool alt = (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) != 0;
            bool ctrl = (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0;

            double fx = 1.0, fy = 1.0;
            if (ctrl)
            {
                fx = step;
                fy = step;
            }
            else if (alt)
            {
                fy = step;
            }
            else /* default or shift */
            {
                fx = step;
            }

            Model.ZoomAt(fx, fy, centerDataX, centerDataY);
            RefreshScalesAndRedraw();
            e.Handled = true;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (Model != null && !_userChangedView)
                Model.AutoFitDataRange();                      // first render, if needed
            RefreshScalesAndRedraw();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_skia != null)
            {
                _skia.PaintSurface -= OnSkiaPaint;
                _skia.MouseWheel -= OnSkiaMouseWheel;
                _skia.MouseDown -= OnSkiaMouseDown;
                _skia.MouseMove -= OnSkiaMouseMove;
                _skia.MouseUp -= OnSkiaMouseUp;
                _skia.MouseLeave -= OnSkiaMouseLeave;
            }
            this.PreviewMouseWheel -= OnSkiaMouseWheel;
            DetachFromModel(Model);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshScalesAndRedraw();
        }

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

        private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (Model is null) return;
            if (!_userChangedView) Model.AutoFitDataRange();   // only if user hasn’t interacted
            RefreshScalesAndRedraw();
        }

        private void RefreshScalesAndRedraw()
        {
            if (Model != null)
            {
                var m = Model.PlotMargins;
                var w = ActualWidth;
                var h = ActualHeight;

                if (!double.IsNaN(w) && !double.IsNaN(h) && w > 0 && h > 0)
                {
                    var plotW = Math.Max(1.0, w - (m.Left + m.Right));
                    var plotH = Math.Max(1.0, h - (m.Top  + m.Bottom));
                    Model.UpdateScales(plotW, plotH);
                }
            }

            _skia?.InvalidateVisual();
            InvalidateVisual();
        }

        private void OnSkiaPaint(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (Model == null)
            {
                e.Surface.Canvas.Clear();
                return;
            }

            Renderer.Render(Model, e.Surface.Canvas, e.Info.Width, e.Info.Height); // via interface
        }
    }
}
