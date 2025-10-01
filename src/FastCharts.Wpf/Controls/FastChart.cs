using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;

using FastCharts.Core;
using FastCharts.Core.Helpers;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using FastCharts.Core.Primitives;
using FastCharts.Rendering.Skia;

using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace FastCharts.Wpf.Controls
{
    /// <summary>
    /// WPF host control for FastCharts with Skia rendering and user interactions (zoom/pan).
    /// - Default renderer: SkiaChartRenderer (from FastCharts.Rendering.Skia)
    /// - Extensibility: assign RenderOverride to plug a different renderer (OpenGL, etc.)
    /// Includes redraw throttling to coalesce rapid interaction events.
    /// </summary>
    [TemplatePart(Name = PartSkia, Type = typeof(SKElement))]
    public class FastChart : Control
    {
        private const string PartSkia = "PART_Skia";

        private SKElement? _skia;
        private bool _userChangedView;

        // Throttling state
        private DateTime _lastRedrawUtc = DateTime.MinValue;
        private bool _redrawScheduled;
        private TimeSpan _minRedrawInterval = TimeSpan.FromMilliseconds(16.6); // ~60 FPS default

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

        public static readonly DependencyProperty MaxRefreshRateProperty =
            DependencyProperty.Register(
                nameof(MaxRefreshRate),
                typeof(int),
                typeof(FastChart),
                new PropertyMetadata(60, OnMaxRefreshRateChanged, CoerceMaxRefreshRate));

        /// <summary>
        /// Maximum redraw rate (frames per second). Default 60. Set lower to reduce CPU usage.
        /// </summary>
        public int MaxRefreshRate
        {
            get => (int)GetValue(MaxRefreshRateProperty);
            set => SetValue(MaxRefreshRateProperty, value);
        }

        private static object CoerceMaxRefreshRate(DependencyObject d, object baseValue)
        {
            if (baseValue is int v)
            {
                if (v < 1)
                {
                    return 1;
                }
                if (v > 240)
                {
                    return 240; // clamp to sane upper bound
                }
                return v;
            }
            return 60;
        }

        private static void OnMaxRefreshRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FastChart fc && e.NewValue is int fps && fps > 0)
            {
                fc._minRedrawInterval = TimeSpan.FromSeconds(1.0 / fps);
            }
        }

        public ChartModel Model
        {
            get => (ChartModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = (FastChart)d;
            chart._userChangedView = false; // allow initial AutoFit
            chart.RequestRedraw();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_skia != null)
            {
                _skia.PaintSurface -= OnSkiaPaintSurface;
                _skia.MouseDown -= OnSkiaMouseDown;
                _skia.MouseMove -= OnSkiaMouseMove;
                _skia.MouseUp -= OnSkiaMouseUp;
                _skia.MouseLeave -= OnSkiaMouseLeave;
                _skia.MouseWheel -= OnSkiaMouseWheel;
                _skia.KeyDown -= OnSkiaKeyDown;
            }

            _skia = GetTemplateChild(PartSkia) as SKElement;
            if (_skia == null)
            {
                return;
            }

            _skia.PaintSurface += OnSkiaPaintSurface;
            _skia.MouseDown += OnSkiaMouseDown;
            _skia.MouseMove += OnSkiaMouseMove;
            _skia.MouseUp += OnSkiaMouseUp;
            _skia.MouseLeave += OnSkiaMouseLeave;
            _skia.MouseWheel += OnSkiaMouseWheel;
            _skia.KeyDown += OnSkiaKeyDown;
            _skia.Focusable = true;
            _skia.Focus();

            this.Focusable = true;
            this.KeyDown -= OnChartKeyDown;
            this.KeyDown += OnChartKeyDown;

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
                Model.Behaviors.Add(new MultiSeriesTooltipBehavior());
                Model.Behaviors.Add(new ZoomRectBehavior());
                Model.Behaviors.Add(new NearestPointBehavior());
                Model.Behaviors.Add(new LegendToggleBehavior());
                Model.Behaviors.Add(new ZoomWheelBehavior());
                Model.Behaviors.Add(new PanBehavior());
            }
            else if (!Model.Behaviors.Any(b => b is MultiSeriesTooltipBehavior))
            {
                Model.Behaviors.Insert(1, new MultiSeriesTooltipBehavior());
            }

            RequestRedraw(forceImmediate: true);
        }

        private void OnSkiaPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            try
            {
                if (Model == null)
                {
                    return;
                }
                var canvas = e.Surface.Canvas;
                _renderer.Render(Model, canvas, e.Info.Width, e.Info.Height);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chart rendering validation error: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chart rendering state error: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected chart rendering error: {ex}");
            }
        }

        private void OnSkiaMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_skia == null)
            {
                return;
            }
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
                BuildModifiers(),
                pos.X, pos.Y,
                0,
                _skia.ActualWidth,
                _skia.ActualHeight);
            if (RouteToBehaviors(ev)) { RequestRedraw(); }
        }

        private void OnSkiaMouseMove(object sender, MouseEventArgs e)
        {
            if (_skia == null)
            {
                return;
            }
            var pos = e.GetPosition(_skia);
            UpdateDataCoordsForTooltip(pos.X, pos.Y);
            var ev = new InteractionEvent(
                PointerEventType.Move,
                PointerButton.None,
                BuildModifiers(),
                pos.X, pos.Y,
                0,
                _skia.ActualWidth,
                _skia.ActualHeight);
            var handled = RouteToBehaviors(ev);
            _userChangedView |= handled;
            Mouse.OverrideCursor = (Model.InteractionState != null && Model.InteractionState.IsPanning) ? Cursors.Hand : null;
            RequestRedraw();
        }

        private void OnSkiaMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_skia == null)
            {
                return;
            }
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
                BuildModifiers(),
                pos.X, pos.Y,
                0,
                _skia.ActualWidth,
                _skia.ActualHeight);
            if (RouteToBehaviors(ev)) { RequestRedraw(); }
            if (Model.InteractionState == null || !Model.InteractionState.IsPanning)
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void OnSkiaMouseLeave(object sender, MouseEventArgs e)
        {
            if (_skia != null)
            {
                var pos = e.GetPosition(_skia);
                var ev = new InteractionEvent(PointerEventType.Leave, PointerButton.None, new PointerModifiers(), pos.X, pos.Y, 0, _skia.ActualWidth, _skia.ActualHeight);
                if (RouteToBehaviors(ev)) { RequestRedraw(); }
            }
            Mouse.OverrideCursor = null;
        }

        private void OnSkiaMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_skia == null)
            {
                return;
            }
            _userChangedView = true;
            var pos = (sender is IInputElement el) ? Mouse.GetPosition(el) : e.GetPosition(_skia);
            var ev = new InteractionEvent(
                PointerEventType.Wheel,
                PointerButton.None,
                BuildModifiers(),
                pos.X, pos.Y,
                e.Delta > 0 ? 1 : -1,
                _skia.ActualWidth,
                _skia.ActualHeight);
            if (RouteToBehaviors(ev)) { RequestRedraw(); }
            e.Handled = true;
        }

        private void OnSkiaKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Model?.InteractionState != null && Model.InteractionState.TooltipLocked)
            {
                Model.InteractionState.TooltipLocked = false;
                Model.InteractionState.TooltipAnchorX = null;
                Model.InteractionState.TooltipSeries.Clear();
                Model.InteractionState.TooltipText = null;
                RequestRedraw();
                e.Handled = true;
            }
        }

        private void OnChartKeyDown(object sender, KeyEventArgs e)
        {
            OnSkiaKeyDown(sender, e);
        }

        private PointerModifiers BuildModifiers()
        {
            return new PointerModifiers
            {
                Ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                Shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                Alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)
            };
        }

        /// <summary>
        /// Coalesces rapid redraw requests and enforces MaxRefreshRate.
        /// </summary>
        private void RequestRedraw(bool forceImmediate = false)
        {
            if (_skia == null)
            {
                return;
            }
            if (forceImmediate)
            {
                _redrawScheduled = false;
                _lastRedrawUtc = DateTime.UtcNow;
                _skia.InvalidateVisual();
                return;
            }
            var now = DateTime.UtcNow;
            var elapsed = now - _lastRedrawUtc;
            if (elapsed >= _minRedrawInterval && !_redrawScheduled)
            {
                _redrawScheduled = true;
                _skia.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _redrawScheduled = false;
                    _lastRedrawUtc = DateTime.UtcNow;
                    _skia.InvalidateVisual();
                }), DispatcherPriority.Background);
            }
            else if (!_redrawScheduled)
            {
                var delay = _minRedrawInterval - elapsed;
                if (delay < TimeSpan.FromMilliseconds(1))
                {
                    delay = TimeSpan.FromMilliseconds(1);
                }
                _redrawScheduled = true;
                _ = ScheduleDelayedRedraw(delay);
            }
        }

        private async Task ScheduleDelayedRedraw(TimeSpan delay)
        {
            try
            {
                await Task.Delay(delay).ConfigureAwait(true);
            }
            catch
            {
                _redrawScheduled = false;
                return;
            }
            if (_skia == null)
            {
                _redrawScheduled = false;
                return;
            }
            _lastRedrawUtc = DateTime.UtcNow;
            _redrawScheduled = false;
            _skia.InvalidateVisual();
        }

        private bool RouteToBehaviors(InteractionEvent ev)
        {
            var handled = false;
            for (var i = 0; i < Model.Behaviors.Count; i++)
            {
                handled |= Model.Behaviors[i].OnEvent(Model, ev);
            }
            return handled;
        }

        private void UpdateDataCoordsForTooltip(double pixelX, double pixelY)
        {
            if (_skia == null || Model == null)
            {
                return;
            }
            if (!ValidationHelper.AreValidCoordinates(pixelX, pixelY))
            {
                return;
            }
            var m = Model.PlotMargins;
            double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;
            var plotW = _skia.ActualWidth - (left + right);
            var plotH = _skia.ActualHeight - (top + bottom);
            if (plotW <= 0 || plotH <= 0)
            {
                return;
            }
            var px = pixelX - left;
            if (px < 0) { px = 0; } else if (px > plotW) { px = plotW; }
            var py = pixelY - top;
            if (py < 0) { py = 0; } else if (py > plotH) { py = plotH; }
            var xr = Model.XAxis.VisibleRange; var yr = Model.YAxis.VisibleRange;
            if (!ValidationHelper.IsValidRange(xr) || !ValidationHelper.IsValidRange(yr))
            {
                return;
            }
            var x = xr.Min + (px / plotW) * (xr.Max - xr.Min);
            var y = yr.Max - (py / plotH) * (yr.Max - yr.Min);
            if (!ValidationHelper.AreValidCoordinates(x, y))
            {
                return;
            }
            Model.InteractionState ??= new InteractionState();
            Model.InteractionState.DataX = x;
            Model.InteractionState.DataY = y;
        }
    }
}
