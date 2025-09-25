using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FastCharts.Core;              // ChartModel
using FastCharts.Core.Primitives;   // FRange
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors; // IAxis<T>
using FastCharts.Rendering.Skia;    // SkiaChartRenderer (renderer par défaut)
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

        private SKElement? _skia;
        private bool _isPanning;
        private Point _lastMousePos;
        private bool _userChangedView;

        // Default renderer instance (Skia)
        private readonly SkiaChartRenderer _renderer = new SkiaChartRenderer();

        static FastChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FastChart),
                new FrameworkPropertyMetadata(typeof(FastChart)));
        }

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
            {
                return;
            }

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
            if (!_userChangedView)
            {
                Model.AutoFitDataRange();
            }

            if (Model.Behaviors.Count == 0)
            {
                Model.Behaviors.Add(new CrosshairBehavior());
                Model.Behaviors.Add(new ZoomRectBehavior());
                Model.Behaviors.Add(new NearestPointBehavior());
            }

            Redraw();
        }

        private void OnSkiaPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            _renderer.Render(Model, canvas, e.Info.Width, e.Info.Height);
        }

        private void OnSkiaMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_skia == null) return;
            var pos = e.GetPosition(_skia);
            var ev = new InteractionEvent(
                PointerEventType.Down,
                e.ChangedButton switch
                {
                    MouseButton.Left => PointerButton.Left,
                    MouseButton.Middle => PointerButton.Middle,
                    MouseButton.Right => PointerButton.Right,
                    _ => PointerButton.None
                },
                new PointerModifiers
                {
                    Ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                    Shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                    Alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)
                },
                pos.X, pos.Y,
                0,
                _skia.ActualWidth,
                _skia.ActualHeight);
            if (RouteToBehaviors(ev)) { Redraw(); }

            if (e.ChangedButton == MouseButton.Left)
            {
                _userChangedView = true;
                _isPanning = true;
                _lastMousePos = pos;
                _skia?.CaptureMouse();
                Mouse.OverrideCursor = Cursors.SizeAll;
            }
        }

        private void OnSkiaMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPanning || _skia == null)
            {
                return;
            }

            var pos = e.GetPosition(_skia);

            // Plot margins & size
            var m = Model.PlotMargins;
            double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;

            double plotW = _skia.ActualWidth  - (left + right);
            double plotH = _skia.ActualHeight - (top  + bottom);
            if (plotW < 0) { plotW = 0; }
            if (plotH < 0) { plotH = 0; }

            // Pixel deltas
            double dxPx = (pos.X - _lastMousePos.X);
            double dyPx = (pos.Y - _lastMousePos.Y);

            // Visible ranges (from axes which mirror the viewport)
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

                // Use viewport pan to keep axis/scale sync consistent.
                Model.Viewport.Pan(dxData, dyData);

                UpdateDataCoordsForTooltip(pos.X, pos.Y);
                var ev = new InteractionEvent(
                    PointerEventType.Move,
                    PointerButton.None,
                    new PointerModifiers
                    {
                        Ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                        Shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                        Alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)
                    },
                    pos.X, pos.Y,
                    0,
                    _skia.ActualWidth,
                    _skia.ActualHeight);

                if (RouteToBehaviors(ev))
                {
                    Redraw();
                }
                else
                {
                    Redraw(); // need redraw anyway for pan
                }
            }

            _lastMousePos = pos;
        }

        private void OnSkiaMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_skia != null)
            {
                var pos = e.GetPosition(_skia);
                var ev = new InteractionEvent(
                    PointerEventType.Up,
                    e.ChangedButton switch
                    {
                        MouseButton.Left => PointerButton.Left,
                        MouseButton.Middle => PointerButton.Middle,
                        MouseButton.Right => PointerButton.Right,
                        _ => PointerButton.None
                    },
                    new PointerModifiers
                    {
                        Ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                        Shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                        Alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)
                    },
                    pos.X, pos.Y,
                    0,
                    _skia.ActualWidth,
                    _skia.ActualHeight);
                if (RouteToBehaviors(ev)) { Redraw(); }
            }

            if (_isPanning && _skia != null)
            {
                _isPanning = false;
                _skia.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
            }
        }

        private void OnSkiaMouseLeave(object sender, MouseEventArgs e)
        {
            if (_skia != null)
            {
                var pos = _lastMousePos;
                var ev = new InteractionEvent(PointerEventType.Leave, PointerButton.None, new PointerModifiers(), pos.X, pos.Y, 0, _skia.ActualWidth, _skia.ActualHeight);
                if (RouteToBehaviors(ev)) { Redraw(); }
            }
            if (_isPanning && _skia != null)
            {
                _isPanning = false;
                _skia.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
            }
        }

        private void OnSkiaMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_skia == null) { return; }
            _userChangedView = true;
            bool zoomIn = e.Delta > 0;
            double scaleContract = zoomIn ? 1.1 : 0.9;
            Point pos = (sender is IInputElement el) ? Mouse.GetPosition(el) : e.GetPosition(_skia);

            var ev = new InteractionEvent(
                PointerEventType.Wheel,
                PointerButton.None,
                new PointerModifiers
                {
                    Ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                    Shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                    Alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)
                },
                pos.X, pos.Y,
                wheelDelta: zoomIn ? 1 : -1,
                surfaceWidth: _skia.ActualWidth,
                surfaceHeight: _skia.ActualHeight);
            if (RouteToBehaviors(ev)) { Redraw(); }

            // Current factor (<1 = zoom in) but Viewport.Zoom expects scale > 1 to shrink (contract) => invert mapping.
            // bool zoomIn = e.Delta > 0;
            // double scaleContract = zoomIn ? 1.1 : 0.9; // >1 contract, <1 expand

            // Cursor position (prefer sender if available)
            // Point pos = (sender is IInputElement el) ? Mouse.GetPosition(el) : e.GetPosition(_skia);

            // Plot margins & size
            // var m = Model.PlotMargins;
            // double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;

            // double plotW = _skia.ActualWidth  - (left + right);
            // double plotH = _skia.ActualHeight - (top  + bottom);
            // if (plotW < 0) { plotW = 0; }
            // if (plotH < 0) { plotH = 0; }

            // Plot-relative pixels
            // double px = pos.X - left; if (px < 0) { px = 0; } else if (px > plotW) { px = plotW; }
            // double py = pos.Y - top;  if (py < 0) { py = 0; } else if (py > plotH) { py = plotH; }

            // Visible ranges
            // FRange vx = Model.XAxis.VisibleRange;
            // FRange vy = Model.YAxis.VisibleRange;
            // double spanX = vx.Max - vx.Min;
            // double spanY = vy.Max - vy.Min;

            // Ratios 0..1 inside plot
            // double rx = (plotW > 0) ? (px / plotW) : 0.0;
            // double ry = (plotH > 0) ? (py / plotH) : 0.0;

            // Anchor in data space (Y inverted)
            // double anchorX = vx.Min + rx * spanX;
            // double anchorY = vy.Max - ry * spanY;

            // Apply zoom on viewport
            // Model.Viewport.Zoom(scaleContract, scaleContract, new PointD(anchorX, anchorY));

            // Redraw();
            e.Handled = true;
        }

        private void Redraw()
        {
            if (_skia != null)
            {
                _skia.InvalidateVisual();
            }
        }
        
        private bool RouteToBehaviors(InteractionEvent ev)
        {
            bool handled = false;
            for (int i = 0; i < Model.Behaviors.Count; i++)
            {
                handled |= Model.Behaviors[i].OnEvent(Model, ev);
            }

            return handled;
        }

        private void UpdateDataCoordsForTooltip(double pixelX, double pixelY)
        {
            if (_skia == null)
            {
                return;
            }
            
            // Convert SURFACE pixels to DATA using VisibleRange and plotRect (same logic as renderer)
            var m = Model.PlotMargins;
            double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;

            double plotW = _skia.ActualWidth  - (left + right);
            double plotH = _skia.ActualHeight - (top  + bottom);
            if (plotW <= 0 || plotH <= 0) { return; }

            double px = pixelX - left; if (px < 0) { px = 0; } else if (px > plotW) { px = plotW; }
            double py = pixelY - top;  if (py < 0) { py = 0; } else if (py > plotH) { py = plotH; }

            var xr = Model.XAxis.VisibleRange;
            var yr = Model.YAxis.VisibleRange;
            double x = xr.Min + (px / plotW) * (xr.Max - xr.Min);
            double y = yr.Max - (py / plotH) * (yr.Max - yr.Min);

            if (Model.InteractionState == null)
            {
                Model.InteractionState = new InteractionState();
            }

            Model.InteractionState.DataX = x;
            Model.InteractionState.DataY = y;
        }
    }
}
