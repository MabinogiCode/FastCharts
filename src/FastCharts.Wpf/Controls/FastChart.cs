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
                Model.Behaviors.Add(new LegendToggleBehavior());
                Model.Behaviors.Add(new ZoomWheelBehavior());
                Model.Behaviors.Add(new PanBehavior());
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
        }

        private void OnSkiaMouseMove(object sender, MouseEventArgs e)
        {
            if (_skia == null)
            {
                return;
            }

            var pos = e.GetPosition(_skia);

            // Update data coords for tooltip/crosshair consumers
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
                _userChangedView = true;
                // Update cursor based on pan state
                if (Model.InteractionState != null && Model.InteractionState.IsPanning)
                {
                    Mouse.OverrideCursor = Cursors.Hand;
                }
                else
                {
                    Mouse.OverrideCursor = null;
                }
                Redraw();
            }
            else
            {
                // Update cursor also when not handled (e.g., just move)
                if (Model.InteractionState != null && Model.InteractionState.IsPanning)
                {
                    Mouse.OverrideCursor = Cursors.Hand;
                }
                else
                {
                    Mouse.OverrideCursor = null;
                }
                Redraw(); // crosshair/nearest may need redraw on move regardless
            }
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

                // cursor reset when pan stops
                if (Model.InteractionState == null || !Model.InteractionState.IsPanning)
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void OnSkiaMouseLeave(object sender, MouseEventArgs e)
        {
            if (_skia != null)
            {
                var pos = e.GetPosition(_skia);
                var ev = new InteractionEvent(PointerEventType.Leave, PointerButton.None, new PointerModifiers(), pos.X, pos.Y, 0, _skia.ActualWidth, _skia.ActualHeight);
                if (RouteToBehaviors(ev)) { Redraw(); }
            }
            // cursor reset on leave
            Mouse.OverrideCursor = null;
        }

        private void OnSkiaMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_skia == null) { return; }
            _userChangedView = true;
            bool zoomIn = e.Delta > 0;
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
