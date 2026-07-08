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
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace FastCharts.Wpf.Controls
{
    /// <summary>
    /// WPF host control for FastCharts with Skia rendering and user interactions (zoom/pan).
    /// Default renderer: SkiaChartRenderer (from FastCharts.Rendering.Skia)
    /// Extensibility: assign RenderOverride to plug a different renderer (OpenGL, etc.)
    /// Includes redraw throttling to coalesce rapid interaction events.
    /// Set <see cref="UseGpu"/> to true to render through an OpenGL-backed surface
    /// (<see cref="SKGLElement"/>) instead of the default CPU surface (<see cref="SKElement"/>).
    /// </summary>
    [TemplatePart(Name = PartHost, Type = typeof(Border))]
    [TemplatePart(Name = PartSkia, Type = typeof(SKElement))]
    public class FastChart : Control
    {
        private const string PartHost = "PART_Host";
        private const string PartSkia = "PART_Skia";
        private const int MinRefreshRate = 1;
        private const int MaxRefreshRateLimit = 240;
        private const int DefaultRefreshRate = 60;
        private const double DefaultFrameTimeMs = 16.6; // ~60 FPS

        private Border? host;
        private SKElement? rasterElement;
        private FrameworkElement? surfaceElement;
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

        public static readonly DependencyProperty UseGpuProperty =
            DependencyProperty.Register(
                nameof(UseGpu),
                typeof(bool),
                typeof(FastChart),
                new PropertyMetadata(false, OnUseGpuChanged));

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

        /// <summary>
        /// When true, renders through an OpenGL-backed <see cref="SKGLElement"/> surface;
        /// when false (default) renders through the CPU <see cref="SKElement"/> surface.
        /// Opt-in: the default keeps the zero-dependency raster path. Requires a working
        /// OpenGL context on the machine; switching at runtime rebuilds the surface.
        /// </summary>
        public bool UseGpu
        {
            get => (bool)GetValue(UseGpuProperty);
            set => SetValue(UseGpuProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DetachSurface();

            host = GetTemplateChild(PartHost) as Border;
            rasterElement = GetTemplateChild(PartSkia) as SKElement;
            if (host == null)
            {
                return;
            }

            CreateSurface();

            this.Focusable = true;
            this.KeyDown -= OnChartKeyDown;
            this.KeyDown += OnChartKeyDown;

            this.Loaded -= OnLoaded;
            this.Loaded += OnLoaded;
        }

        /// <summary>
        /// Selects the active surface: the template's CPU <see cref="SKElement"/> by default,
        /// or a code-created OpenGL <see cref="SKGLElement"/> when <see cref="UseGpu"/> is set.
        /// The default path reuses the templated element untouched (zero behavioral change).
        /// </summary>
        private void CreateSurface()
        {
            if (host == null)
            {
                return;
            }

            if (UseGpu)
            {
                var gl = new SKGLElement
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                gl.PaintSurface += OnGlPaintSurface;
                host.Child = gl; // replaces the templated raster element in the tree
                surfaceElement = gl;
            }
            else
            {
                if (rasterElement == null)
                {
                    return;
                }

                rasterElement.PaintSurface += OnSkiaPaintSurface;
                if (!ReferenceEquals(host.Child, rasterElement))
                {
                    host.Child = rasterElement;
                }

                surfaceElement = rasterElement;
            }

            surfaceElement.MouseDown += OnSkiaMouseDown;
            surfaceElement.MouseMove += OnSkiaMouseMove;
            surfaceElement.MouseUp += OnSkiaMouseUp;
            surfaceElement.MouseLeave += OnSkiaMouseLeave;
            surfaceElement.MouseWheel += OnSkiaMouseWheel;
            surfaceElement.KeyDown += OnSkiaKeyDown;
            surfaceElement.Focusable = true;
            surfaceElement.Focus();
        }

        /// <summary>
        /// Unsubscribes from the current surface (if any) before rebuild/teardown. A GPU
        /// surface is also detached from the visual tree; the templated raster element is
        /// left in place for reuse.
        /// </summary>
        private void DetachSurface()
        {
            if (surfaceElement == null)
            {
                return;
            }

            if (surfaceElement is SKGLElement gl)
            {
                gl.PaintSurface -= OnGlPaintSurface;
                if (host != null && ReferenceEquals(host.Child, gl))
                {
                    host.Child = null;
                }
            }
            else if (surfaceElement is SKElement sk)
            {
                sk.PaintSurface -= OnSkiaPaintSurface;
            }

            surfaceElement.MouseDown -= OnSkiaMouseDown;
            surfaceElement.MouseMove -= OnSkiaMouseMove;
            surfaceElement.MouseUp -= OnSkiaMouseUp;
            surfaceElement.MouseLeave -= OnSkiaMouseLeave;
            surfaceElement.MouseWheel -= OnSkiaMouseWheel;
            surfaceElement.KeyDown -= OnSkiaKeyDown;

            surfaceElement = null;
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

        private static void OnUseGpuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Rebuild the surface only if the template is already applied; otherwise
            // OnApplyTemplate will pick up the new value when it runs.
            if (d is FastChart chart && chart.host != null)
            {
                chart.DetachSurface();
                chart.CreateSurface();
                chart.RequestRedraw(forceImmediate: true);
            }
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
            PaintChart(e.Surface, e.Info.Width, e.Info.Height);
        }

        private void OnGlPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
        {
            PaintChart(e.Surface, e.Info.Width, e.Info.Height);
        }

        private void PaintChart(SKSurface surface, int width, int height)
        {
            try
            {
                if (Model == null)
                {
                    return;
                }

                renderer.Render(Model, surface.Canvas, width, height);
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
            if (surfaceElement == null)
            {
                return;
            }

            var pos = e.GetPosition(surfaceElement);
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
                surfaceElement.ActualWidth,
                surfaceElement.ActualHeight);

            if (RouteToBehaviors(ev))
            {
                RequestRedraw();
            }
        }

        private void OnSkiaMouseMove(object sender, MouseEventArgs e)
        {
            if (surfaceElement == null)
            {
                return;
            }

            var pos = e.GetPosition(surfaceElement);
            UpdateDataCoordsForTooltip(pos.X, pos.Y);

            var ev = new InteractionEvent(
                PointerEventType.Move,
                PointerButton.None,
                BuildModifiers(),
                pos.X,
                pos.Y,
                0,
                surfaceElement.ActualWidth,
                surfaceElement.ActualHeight);

            var handled = RouteToBehaviors(ev);
            userChangedView |= handled;
            Mouse.OverrideCursor = (Model?.InteractionState?.IsPanning == true) ? Cursors.Hand : null;
            RequestRedraw();
        }

        private void OnSkiaMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (surfaceElement == null)
            {
                return;
            }

            var pos = e.GetPosition(surfaceElement);
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
                surfaceElement.ActualWidth,
                surfaceElement.ActualHeight);

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
            if (surfaceElement != null)
            {
                var pos = e.GetPosition(surfaceElement);
                var ev = new InteractionEvent(
                    PointerEventType.Leave,
                    PointerButton.None,
                    new PointerModifiers(),
                    pos.X,
                    pos.Y,
                    0,
                    surfaceElement.ActualWidth,
                    surfaceElement.ActualHeight);

                if (RouteToBehaviors(ev))
                {
                    RequestRedraw();
                }
            }

            Mouse.OverrideCursor = null;
        }

        private void OnSkiaMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (surfaceElement == null)
            {
                return;
            }

            userChangedView = true;
            var pos = (sender is IInputElement el) ? Mouse.GetPosition(el) : e.GetPosition(surfaceElement);
            var ev = new InteractionEvent(
                PointerEventType.Wheel,
                PointerButton.None,
                BuildModifiers(),
                pos.X,
                pos.Y,
                e.Delta > 0 ? 1 : -1,
                surfaceElement.ActualWidth,
                surfaceElement.ActualHeight);

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
            if (surfaceElement == null)
            {
                return;
            }

            if (forceImmediate)
            {
                redrawScheduled = false;
                lastRedrawUtc = DateTime.UtcNow;
                surfaceElement.InvalidateVisual();
                return;
            }

            var now = DateTime.UtcNow;
            var elapsed = now - lastRedrawUtc;

            if (elapsed >= minRedrawInterval && !redrawScheduled)
            {
                redrawScheduled = true;
                surfaceElement.Dispatcher.BeginInvoke(new Action(() =>
                {
                    redrawScheduled = false;
                    lastRedrawUtc = DateTime.UtcNow;
                    surfaceElement.InvalidateVisual();
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

            if (surfaceElement == null)
            {
                redrawScheduled = false;
                return;
            }

            lastRedrawUtc = DateTime.UtcNow;
            redrawScheduled = false;
            surfaceElement.InvalidateVisual();
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
            if (surfaceElement == null || Model == null)
            {
                return;
            }

            if (!ValidationHelper.AreValidCoordinates(pixelX, pixelY))
            {
                return;
            }

            var m = Model.PlotMargins;
            var plotW = surfaceElement.ActualWidth - (m.Left + m.Right);
            var plotH = surfaceElement.ActualHeight - (m.Top + m.Bottom);

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
