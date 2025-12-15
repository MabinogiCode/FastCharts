using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
    /// Default renderer: SkiaChartRenderer (from FastCharts.Rendering.Skia)
    /// Extensibility: assign RenderOverride to plug a different renderer (OpenGL, etc.)
    /// Includes redraw throttling to coalesce rapid interaction events.
    /// </summary>
    [TemplatePart(Name = PartSkia, Type = typeof(SKElement))]
    public class FastChart : Control
    {
        private const string PartSkia = "PART_Skia";
        private const int MinRefreshRate = 1;
        private const int MaxRefreshRateLimit = 240;
        private const int DefaultRefreshRate = 60;
        private const double DefaultFrameTimeMs = 16.6; // ~60 FPS

        private SKElement? skiaElement;
        private bool userChangedView;
        private DateTime lastRedrawUtc = DateTime.MinValue;
        private bool redrawScheduled;
        private TimeSpan minRedrawInterval = TimeSpan.FromMilliseconds(DefaultFrameTimeMs);
        private readonly SkiaChartRenderer renderer = new SkiaChartRenderer();

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
                new PropertyMetadata(DefaultRefreshRate, OnMaxRefreshRateChanged, CoerceMaxRefreshRate));

        static FastChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FastChart),
                new FrameworkPropertyMetadata(typeof(FastChart)));
        }

        /// <summary>
        /// Maximum redraw rate (frames per second). Default 60. Set lower to reduce CPU usage.
        /// </summary>
        public int MaxRefreshRate
        {
            get => (int)GetValue(MaxRefreshRateProperty);
            set => SetValue(MaxRefreshRateProperty, value);
        }

        public ChartModel Model
        {
            get => (ChartModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (skiaElement != null)
            {
                skiaElement.PaintSurface -= OnSkiaPaintSurface;
                skiaElement.MouseDown -= OnSkiaMouseDown;
                skiaElement.MouseMove -= OnSkiaMouseMove;
                skiaElement.MouseUp -= OnSkiaMouseUp;
                skiaElement.MouseLeave -= OnSkiaMouseLeave;
                skiaElement.MouseWheel -= OnSkiaMouseWheel;
                skiaElement.KeyDown -= OnSkiaKeyDown;
            }

            skiaElement = GetTemplateChild(PartSkia) as SKElement;
            if (skiaElement == null)
            {
                return;
            }

            skiaElement.PaintSurface += OnSkiaPaintSurface;
            skiaElement.MouseDown += OnSkiaMouseDown;
            skiaElement.MouseMove += OnSkiaMouseMove;
            skiaElement.MouseUp += OnSkiaMouseUp;
            skiaElement.MouseLeave += OnSkiaMouseLeave;
            skiaElement.MouseWheel += OnSkiaMouseWheel;
            skiaElement.KeyDown += OnSkiaKeyDown;
            skiaElement.Focusable = true;
            skiaElement.Focus();

            this.Focusable = true;
            this.KeyDown -= OnChartKeyDown;
            this.KeyDown += OnChartKeyDown;

            this.Loaded -= OnLoaded;
            this.Loaded += OnLoaded;
        }

        private static object CoerceMaxRefreshRate(DependencyObject d, object baseValue)
        {
            if (baseValue is int v)
            {
                if (v < MinRefreshRate)
                {
                    return MinRefreshRate;
                }

                if (v > MaxRefreshRateLimit)
                {
                    return MaxRefreshRateLimit;
                }

                return v;
            }

            return DefaultRefreshRate;
        }

        private static void OnMaxRefreshRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FastChart fc && e.NewValue is int fps && fps > 0)
            {
                fc.minRedrawInterval = TimeSpan.FromSeconds(1.0 / fps);
            }
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = (FastChart)d;
            chart.userChangedView = false; // allow initial AutoFit
            chart.RequestRedraw();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeChartIfNeeded();
            RequestRedraw(forceImmediate: true);
        }

        private void InitializeChartIfNeeded()
        {
            if (Model == null)
            {
                return;
            }

            if (!userChangedView)
            {
                Model.AutoFitDataRange();
            }

            ConfigureDefaultBehaviors();
        }

        private void ConfigureDefaultBehaviors()
        {
            if (Model == null)
            {
                return;
            }

            if (Model.Behaviors.Count == 0)
            {
                AddDefaultBehaviors();
            }
            else
            {
                EnsureTooltipBehavior();
            }
        }

        private void AddDefaultBehaviors()
        {
            if (Model == null)
            {
                return;
            }

            var defaultBehaviors = new IBehavior[]
            {
                new CrosshairBehavior(),
                new MultiSeriesTooltipBehavior(),
                new ZoomRectBehavior(),
                new NearestPointBehavior(),
                new LegendToggleBehavior(),
                new ZoomWheelBehavior(),
                new PanBehavior()
            };

            foreach (var behavior in defaultBehaviors)
            {
                Model.Behaviors.Add(behavior);
            }
        }

        private void EnsureTooltipBehavior()
        {
            if (Model == null)
            {
                return;
            }

            if (!Model.Behaviors.Any(b => b is MultiSeriesTooltipBehavior))
            {
                Model.Behaviors.Insert(1, new MultiSeriesTooltipBehavior());
            }
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
                renderer.Render(Model, canvas, e.Info.Width, e.Info.Height);
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
            if (skiaElement == null)
            {
                return;
            }

            var pos = e.GetPosition(skiaElement);
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
                pos.X,
                pos.Y,
                0,
                skiaElement.ActualWidth,
                skiaElement.ActualHeight);

            if (RouteToBehaviors(ev))
            {
                RequestRedraw();
            }
        }

        private void OnSkiaMouseMove(object sender, MouseEventArgs e)
        {
            if (skiaElement == null)
            {
                return;
            }

            var pos = e.GetPosition(skiaElement);
            UpdateDataCoordsForTooltip(pos.X, pos.Y);

            var ev = new InteractionEvent(
                PointerEventType.Move,
                PointerButton.None,
                BuildModifiers(),
                pos.X,
                pos.Y,
                0,
                skiaElement.ActualWidth,
                skiaElement.ActualHeight);

            var handled = RouteToBehaviors(ev);
            userChangedView |= handled;
            Mouse.OverrideCursor = (Model?.InteractionState?.IsPanning == true) ? Cursors.Hand : null;
            RequestRedraw();
        }

        private void OnSkiaMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (skiaElement == null)
            {
                return;
            }

            var pos = e.GetPosition(skiaElement);
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
                pos.X,
                pos.Y,
                0,
                skiaElement.ActualWidth,
                skiaElement.ActualHeight);

            if (RouteToBehaviors(ev))
            {
                RequestRedraw();
            }

            if (Model?.InteractionState?.IsPanning != true)
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void OnSkiaMouseLeave(object sender, MouseEventArgs e)
        {
            if (skiaElement != null)
            {
                var pos = e.GetPosition(skiaElement);
                var ev = new InteractionEvent(
                    PointerEventType.Leave,
                    PointerButton.None,
                    new PointerModifiers(),
                    pos.X,
                    pos.Y,
                    0,
                    skiaElement.ActualWidth,
                    skiaElement.ActualHeight);

                if (RouteToBehaviors(ev))
                {
                    RequestRedraw();
                }
            }

            Mouse.OverrideCursor = null;
        }

        private void OnSkiaMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (skiaElement == null)
            {
                return;
            }

            userChangedView = true;
            var pos = (sender is IInputElement el) ? Mouse.GetPosition(el) : e.GetPosition(skiaElement);
            var ev = new InteractionEvent(
                PointerEventType.Wheel,
                PointerButton.None,
                BuildModifiers(),
                pos.X,
                pos.Y,
                e.Delta > 0 ? 1 : -1,
                skiaElement.ActualWidth,
                skiaElement.ActualHeight);

            if (RouteToBehaviors(ev))
            {
                RequestRedraw();
            }

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
            if (skiaElement == null)
            {
                return;
            }

            if (forceImmediate)
            {
                redrawScheduled = false;
                lastRedrawUtc = DateTime.UtcNow;
                skiaElement.InvalidateVisual();
                return;
            }

            var now = DateTime.UtcNow;
            var elapsed = now - lastRedrawUtc;

            if (elapsed >= minRedrawInterval && !redrawScheduled)
            {
                redrawScheduled = true;
                skiaElement.Dispatcher.BeginInvoke(new Action(() =>
                {
                    redrawScheduled = false;
                    lastRedrawUtc = DateTime.UtcNow;
                    skiaElement.InvalidateVisual();
                }), DispatcherPriority.Background);
            }
            else if (!redrawScheduled)
            {
                var delay = minRedrawInterval - elapsed;
                if (delay < TimeSpan.FromMilliseconds(1))
                {
                    delay = TimeSpan.FromMilliseconds(1);
                }

                redrawScheduled = true;
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
                redrawScheduled = false;
                return;
            }

            if (skiaElement == null)
            {
                redrawScheduled = false;
                return;
            }

            lastRedrawUtc = DateTime.UtcNow;
            redrawScheduled = false;
            skiaElement.InvalidateVisual();
        }

        private bool RouteToBehaviors(InteractionEvent ev)
        {
            if (Model == null)
            {
                return false;
            }

            var handled = false;
            for (var i = 0; i < Model.Behaviors.Count; i++)
            {
                handled |= Model.Behaviors[i].OnEvent(Model, ev);
            }

            return handled;
        }

        private void UpdateDataCoordsForTooltip(double pixelX, double pixelY)
        {
            if (skiaElement == null || Model == null)
            {
                return;
            }

            if (!ValidationHelper.AreValidCoordinates(pixelX, pixelY))
            {
                return;
            }

            var m = Model.PlotMargins;
            var plotW = skiaElement.ActualWidth - (m.Left + m.Right);
            var plotH = skiaElement.ActualHeight - (m.Top + m.Bottom);

            if (plotW <= 0 || plotH <= 0)
            {
                return;
            }

            var px = ClampValue(pixelX - m.Left, 0, plotW);
            var py = ClampValue(pixelY - m.Top, 0, plotH);
            var xr = Model.XAxis.VisibleRange;
            var yr = Model.YAxis.VisibleRange;

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

        private static double ClampValue(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
