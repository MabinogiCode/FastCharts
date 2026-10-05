using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Interaction.Behaviors;

public sealed class ZoomRectBehavior : IBehavior
{
    private bool _dragging;
    private PointD _start;
    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        model.InteractionState ??= new InteractionState();
        var st = model.InteractionState;

        if (ev.Type == PointerEventType.Down && ev.Button == PointerButton.Left)
        {
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
        }
        else if (ev.Type == PointerEventType.Move)
        {
            if (_dragging)
            {
                st.SelX2 = ev.PixelX;
                st.SelY2 = ev.PixelY;
                return true;
            }
            return false;
        }
        else if (ev.Type == PointerEventType.Up && ev.Button == PointerButton.Left)
        {
            if (_dragging)
            {
                _dragging = false;
                st.ShowSelectionRect = false;
                var x1 = System.Math.Min(_start.X, ev.PixelX);
                var x2 = System.Math.Max(_start.X, ev.PixelX);
                var y1 = System.Math.Min(_start.Y, ev.PixelY);
                var y2 = System.Math.Max(_start.Y, ev.PixelY);
                if ((x2 - x1) < 6 || (y2 - y1) < 6)
                {
                    return true;
                }
                var m = model.PlotMargins;
                var left = m.Left; var top = m.Top; var right = m.Right; var bottom = m.Bottom;
                var plotW = System.Math.Max(0, ev.SurfaceWidth - (left + right));
                var plotH = System.Math.Max(0, ev.SurfaceHeight - (top + bottom));
                if (plotW <= 0 || plotH <= 0)
                {
                    return true;
                }
                var px1 = x1 - left; if (px1 < 0) { px1 = 0; } else if (px1 > plotW) { px1 = plotW; }
                var px2 = x2 - left; if (px2 < 0) { px2 = 0; } else if (px2 > plotW) { px2 = plotW; }
                var py1 = y1 - top; if (py1 < 0) { py1 = 0; } else if (py1 > plotH) { py1 = plotH; }
                var py2 = y2 - top; if (py2 < 0) { py2 = 0; } else if (py2 > plotH) { py2 = plotH; }
                // Go through the viewport so the secondary Y axis follows; normalized
                // coordinates keep the selection exact on logarithmic axes too.
                var newX = AxisCoordinates.SubRange(model.XAxis, model.XAxis.VisibleRange, px1 / plotW, px2 / plotW);
                var newY = AxisCoordinates.SubRange(model.YAxis, model.YAxis.VisibleRange, 1 - (py2 / plotH), 1 - (py1 / plotH));
                model.Viewport.SetVisible(newX, newY);
                return true;
            }
            return false;
        }
        else if (ev.Type == PointerEventType.Leave)
        {
            if (_dragging)
            {
                _dragging = false;
                st.ShowSelectionRect = false;
                return true;
            }
            return false;
        }
        return false; // ignore Wheel and others
    }
}
