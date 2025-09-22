// src/FastCharts.Wpf/Controls/FastChart.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FastCharts.Core;              // ChartModel
using FastCharts.Core.Primitives;   // FRange
using FastCharts.Core.Axes;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors; // IAxis<T>
using FastCharts.Rendering.Skia;    // SkiaChartRenderer (renderer par défaut)
using SkiaSharp;                    // SKCanvas
using SkiaSharp.Views.WPF;
using SkiaSharp.Views.Desktop;      // SKPaintSurfaceEventArgs

namespace FastCharts.Wpf.Controls
{
    /// <summary>
    /// WPF host control for FastCharts with Skia rendering and user interactions (zoom/pan).
    /// - Default renderer: SkiaChartRenderer (from FastCharts.Rendering.Skia)
    /// - Extensibility: assign RenderOverride to plug a different renderer (OpenGL, etc.)
    /// </summary>
    [TemplatePart(Name = PartSkia, Type = typeof(SKElement))]
    public class FastChart : Control
    {
        private const string PartSkia = "PART_Skia";

        private SKElement _skia;
        private bool _isPanning;
        private Point _lastMousePos;
        private bool _userChangedView;

        // Default renderer instance (Skia)
        private readonly SkiaChartRenderer _renderer = new SkiaChartRenderer();

        /// <summary>
        /// Optional hook to override rendering.
        /// If set, it will be called on PaintSurface. If it returns true, default Skia rendering is skipped.
        /// Signature: (model, canvas, width, height) => rendered?
        /// </summary>
        public Func<ChartModel, SKCanvas, int, int, bool> RenderOverride { get; set; }

        static FastChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FastChart),
                new FrameworkPropertyMetadata(typeof(FastChart)));
        }

        #region Dependency Properties

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register(
                nameof(Model),
                typeof(ChartModel),
                typeof(FastChart),
                new PropertyMetadata(null, OnModelChanged));

        public ChartModel Model
        {
            get { return (ChartModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = (FastChart)d;
            chart._userChangedView = false; // allow initial AutoFit
            if (chart._skia != null) chart._skia.InvalidateVisual();
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_skia != null)
            {
                _skia.PaintSurface -= OnSkiaPaintSurface;
                _skia.MouseDown    -= OnSkiaMouseDown;
                _skia.MouseMove    -= OnSkiaMouseMove;
                _skia.MouseUp      -= OnSkiaMouseUp;
                _skia.MouseLeave   -= OnSkiaMouseLeave;
                _skia.MouseWheel   -= OnSkiaMouseWheel;
            }

            _skia = GetTemplateChild(PartSkia) as SKElement;
            if (_skia == null)
                return;

            _skia.PaintSurface += OnSkiaPaintSurface;
            _skia.MouseDown    += OnSkiaMouseDown;
            _skia.MouseMove    += OnSkiaMouseMove;
            _skia.MouseUp      += OnSkiaMouseUp;
            _skia.MouseLeave   += OnSkiaMouseLeave;

            // Wheel: both direct on SKElement and Preview on the control (to beat parent ScrollViewer)
            _skia.MouseWheel += OnSkiaMouseWheel;
            this.PreviewMouseWheel -= OnSkiaMouseWheel;
            this.PreviewMouseWheel += OnSkiaMouseWheel;

            _skia.Focusable = true;
            _skia.Focus();

            this.Loaded -= OnLoaded;
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Model == null) return;

            if (!_userChangedView)
                Model.AutoFitDataRange();

            if (Model.Behaviors.Count == 0)
            {
                Model.Behaviors.Add(new CrosshairBehavior());
            }

            Redraw();
        }

        private void OnSkiaPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            if (Model == null)
            {
                canvas.Clear(SKColors.White);
                return;
            }

            // 1) Optional external renderer
            var hook = RenderOverride;
            if (hook != null)
            {
                try
                {
                    bool handled = hook(Model, canvas, e.Info.Width, e.Info.Height);
                    if (handled) return;
                }
                catch
                {
                    // Swallow hook exceptions to avoid breaking the control
                }
            }

            // 2) Default Skia renderer
            _renderer.Render(Model, canvas, e.Info.Width, e.Info.Height);
        }

        #region Interaction (Pan / Zoom)

        private void OnSkiaMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Model == null) return;

            if (e.ChangedButton == MouseButton.Left)
            {
                _userChangedView = true;
                _isPanning = true;
                _lastMousePos = e.GetPosition(_skia);
                _skia.CaptureMouse();
                Mouse.OverrideCursor = Cursors.SizeAll;
            }
        }

        private void OnSkiaMouseMove(object sender, MouseEventArgs e)
        {
            if (Model == null || !_isPanning) return;

            var pos = e.GetPosition(_skia);

            // Plot margins & size
            var m = Model.PlotMargins;
            double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;

            double plotW = _skia.ActualWidth  - (left + right);
            double plotH = _skia.ActualHeight - (top  + bottom);
            if (plotW < 0) plotW = 0;
            if (plotH < 0) plotH = 0;

            // Pixel deltas
            double dxPx = (pos.X - _lastMousePos.X);
            double dyPx = (pos.Y - _lastMousePos.Y);

            // Visible ranges
            FRange vx = Model.XAxis.VisibleRange;
            FRange vy = Model.YAxis.VisibleRange;

            double spanX = vx.Max - vx.Min;
            double spanY = vy.Max - vy.Min;

            // Pixel -> data (X normal, Y inverted in pixels)
            double dxData = (plotW > 0) ? -dxPx / plotW * spanX : 0.0;
            double dyData = (plotH > 0) ?  dyPx / plotH * spanY : 0.0;

            if (dxData != 0.0 || dyData != 0.0)
            {
                _userChangedView = true;

                Model.XAxis.SetVisibleRange(vx.Min + dxData, vx.Max + dxData);
                Model.YAxis.SetVisibleRange(vy.Min + dyData, vy.Max + dyData);

                UpdateDataCoordsForTooltip(pos.X, pos.Y);
                var ev = new InteractionEvent(
                    PointerEventType.Move,
                    PointerButton.None,
                    new PointerModifiers { Ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                        Shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                        Alt = Keyboard.IsKeyDown(Key.LeftAlt)   || Keyboard.IsKeyDown(Key.RightAlt) },
                    pos.X, pos.Y);

                if (RouteToBehaviors(ev))
                    Redraw();
            }

            _lastMousePos = pos;
        }

        private void OnSkiaMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                _skia.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
            }
        }

        private void OnSkiaMouseLeave(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                _skia.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
            }
            var ev = new InteractionEvent(PointerEventType.Leave, PointerButton.None, new PointerModifiers(), _lastMousePos.X, _lastMousePos.Y);
            if (RouteToBehaviors(ev))
                Redraw();
        }

        private void OnSkiaMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Model == null) return;

            _userChangedView = true;

            // Zoom factor (<1 = zoom in, >1 = zoom out)
            double factor = (e.Delta > 0) ? 0.9 : 1.1;

            // Cursor position (prefer sender if available)
            Point pos = (sender is IInputElement el) ? Mouse.GetPosition(el) : e.GetPosition(_skia);

            // Plot margins & size
            var m = Model.PlotMargins;
            double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;

            double plotW = _skia.ActualWidth  - (left + right);
            double plotH = _skia.ActualHeight - (top  + bottom);
            if (plotW < 0) plotW = 0;
            if (plotH < 0) plotH = 0;

            // Plot-relative pixels (manual clamp for net48)
            double px = pos.X - left; if (px < 0) px = 0; else if (px > plotW) px = plotW;
            double py = pos.Y - top;  if (py < 0) py = 0; else if (py > plotH) py = plotH;

            // Visible ranges
            FRange vx = Model.XAxis.VisibleRange;
            FRange vy = Model.YAxis.VisibleRange;

            double spanX = vx.Max - vx.Min;
            double spanY = vy.Max - vy.Min;

            // Ratios 0..1 inside plot
            double rx = (plotW > 0) ? (px / plotW) : 0.0;
            double ry = (plotH > 0) ? (py / plotH) : 0.0;

            // Anchor in data space (Y inverted)
            double anchorX = vx.Min + rx * spanX;
            double anchorY = vy.Max - ry * spanY;

            // New window around anchor
            double newMinX = anchorX + (vx.Min - anchorX) * factor;
            double newMaxX = anchorX + (vx.Max - anchorX) * factor;
            double newMinY = anchorY + (vy.Min - anchorY) * factor;
            double newMaxY = anchorY + (vy.Max - anchorY) * factor;

            // Avoid degenerate windows
            if (newMaxX == newMinX) { newMinX -= 0.5; newMaxX += 0.5; }
            if (newMaxY == newMinY) { newMinY -= 0.5; newMaxY += 0.5; }

            Model.XAxis.SetVisibleRange(newMinX, newMaxX);
            Model.YAxis.SetVisibleRange(newMinY, newMaxY);

            Redraw();
            e.Handled = true;
        }

        #endregion

        private void Redraw()
        {
            if (_skia != null)
                _skia.InvalidateVisual();
        }
        
        private bool RouteToBehaviors(InteractionEvent ev)
        {
            if (Model == null || Model.Behaviors == null) return false;
            bool handled = false;
            for (int i = 0; i < Model.Behaviors.Count; i++)
                handled |= Model.Behaviors[i].OnEvent(Model, ev);
            return handled;
        }

        private void UpdateDataCoordsForTooltip(double pixelX, double pixelY)
        {
            if (Model == null) return;

            // Convert SURFACE pixels to DATA using VisibleRange and plotRect (same logic as renderer)
            var m = Model.PlotMargins;
            double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;

            double plotW = _skia.ActualWidth  - (left + right);
            double plotH = _skia.ActualHeight - (top  + bottom);
            if (plotW <= 0 || plotH <= 0) return;

            double px = pixelX - left; if (px < 0) px = 0; else if (px > plotW) px = plotW;
            double py = pixelY - top;  if (py < 0) py = 0; else if (py > plotH) py = plotH;

            var xr = Model.XAxis.VisibleRange;
            var yr = Model.YAxis.VisibleRange;
            double x = xr.Min + (px / plotW) * (xr.Max - xr.Min);
            double y = yr.Max - (py / plotH) * (yr.Max - yr.Min);

            if (Model.InteractionState == null)
                Model.InteractionState = new InteractionState();

            Model.InteractionState.DataX = x;
            Model.InteractionState.DataY = y;
        }
    }
}
