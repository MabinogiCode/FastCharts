using FastCharts.Core.Primitives;

namespace FastCharts.Core.Interaction.Behaviors
{
    /// <summary>
    /// Zooms in/out with mouse wheel around the cursor anchor inside the plot rect.
    /// Uses Viewport.Zoom(scaleX, scaleY, pivotData).
    /// </summary>
    public sealed class ZoomWheelBehavior : IBehavior
    {
        public double Step { get; set; } = 1.1; // >1 contracts (zoom-in)

        public bool OnEvent(ChartModel model, InteractionEvent ev)
        {
            if (model == null)
            {
                return false;
            }
            if (ev.Type != PointerEventType.Wheel)
            {
                return false;
            }

            bool zoomIn = ev.WheelDelta > 0;
            double scale = zoomIn ? Step : (1.0 / Step); // >1 contract, <1 expand

            // Plot margins & size
            var m = model.PlotMargins;
            double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;
            double plotW = ev.SurfaceWidth - (left + right);
            double plotH = ev.SurfaceHeight - (top + bottom);
            if (plotW <= 0 || plotH <= 0)
            {
                return false;
            }

            // Plot-relative pixels clamped
            double px = ev.PixelX - left; if (px < 0) px = 0; else if (px > plotW) px = plotW;
            double py = ev.PixelY - top;  if (py < 0) py = 0; else if (py > plotH) py = plotH;

            // Visible ranges
            var vx = model.XAxis.VisibleRange;
            var vy = model.YAxis.VisibleRange;
            double spanX = vx.Max - vx.Min;
            double spanY = vy.Max - vy.Min;

            // Ratios 0..1 inside plot
            double rx = plotW > 0 ? (px / plotW) : 0.0;
            double ry = plotH > 0 ? (py / plotH) : 0.0;

            // Anchor in data space (Y inverted)
            double anchorX = vx.Min + rx * spanX;
            double anchorY = vy.Max - ry * spanY;

            model.Viewport.Zoom(scale, scale, new PointD(anchorX, anchorY));
            return true;
        }
    }
}
