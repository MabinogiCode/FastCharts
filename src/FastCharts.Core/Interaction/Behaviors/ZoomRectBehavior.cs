using FastCharts.Core.Primitives;
using FastCharts.Core.Axes;

namespace FastCharts.Core.Interaction.Behaviors
{
    /// <summary>
    /// Drag with left mouse to draw a selection rectangle inside the plot; on mouse up, zoom view to that data region.
    /// Requires host to route events with Pixel coords and to fill Model.UpdateScales before rendering.
    /// Start only when Shift is held (to avoid conflict with panning).
    /// </summary>
    public sealed class ZoomRectBehavior : IBehavior
    {
        private bool _dragging;
        private PointD _start;

        public bool OnEvent(ChartModel model, InteractionEvent ev)
        {
            if (model == null)
            {
                return false;
            }
            model.InteractionState ??= new InteractionState();
            var st = model.InteractionState;

            switch (ev.Type)
            {
                case PointerEventType.Down when ev.Button == PointerButton.Left:
                    if (!ev.Modifiers.Shift)
                    {
                        return false;
                    }
                    _dragging = true;
                    _start = new PointD(ev.PixelX, ev.PixelY);
                    st.ShowSelectionRect = true;
                    st.SelX1 = st.SelX2 = ev.PixelX;
                    st.SelY1 = st.SelY2 = ev.PixelY;
                    return true;

                case PointerEventType.Move:
                    if (_dragging)
                    {
                        st.SelX2 = ev.PixelX;
                        st.SelY2 = ev.PixelY;
                        return true;
                    }
                    return false;

                case PointerEventType.Up when ev.Button == PointerButton.Left:
                    if (_dragging)
                    {
                        _dragging = false;
                        st.ShowSelectionRect = false;

                        double x1 = System.Math.Min(_start.X, ev.PixelX);
                        double x2 = System.Math.Max(_start.X, ev.PixelX);
                        double y1 = System.Math.Min(_start.Y, ev.PixelY);
                        double y2 = System.Math.Max(_start.Y, ev.PixelY);

                        // Reject tiny drags
                        if ((x2 - x1) < 6 || (y2 - y1) < 6)
                        {
                            return true; // consumed drag end
                        }

                        // Convert SURFACE pixels to data using current VisibleRange and plotRect
                        var m = model.PlotMargins;
                        double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;
                        double plotW = System.Math.Max(0, ev.SurfaceWidth  - (left + right));
                        double plotH = System.Math.Max(0, ev.SurfaceHeight - (top  + bottom));
                        if (plotW <= 0 || plotH <= 0)
                        {
                            return true;
                        }

                        double px1 = x1 - left; if (px1 < 0) px1 = 0; else if (px1 > plotW) px1 = plotW;
                        double px2 = x2 - left; if (px2 < 0) px2 = 0; else if (px2 > plotW) px2 = plotW;
                        double py1 = y1 - top;  if (py1 < 0) py1 = 0; else if (py1 > plotH) py1 = plotH;
                        double py2 = y2 - top;  if (py2 < 0) py2 = 0; else if (py2 > plotH) py2 = plotH;

                        var xr = model.XAxis.VisibleRange;
                        var yr = model.YAxis.VisibleRange;
                        double dx1 = xr.Min + (px1 / plotW) * (xr.Max - xr.Min);
                        double dx2 = xr.Min + (px2 / plotW) * (xr.Max - xr.Min);
                        double dy1 = yr.Max - (py2 / plotH) * (yr.Max - yr.Min); // note py2 for bottom
                        double dy2 = yr.Max - (py1 / plotH) * (yr.Max - yr.Min); // note py1 for top

                        // Apply
                        model.XAxis.SetVisibleRange(dx1, dx2);
                        model.YAxis.SetVisibleRange(dy1, dy2);
                        return true;
                    }
                    return false;

                case PointerEventType.Leave:
                    if (_dragging)
                    {
                        _dragging = false;
                        st.ShowSelectionRect = false;
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }
    }
}
