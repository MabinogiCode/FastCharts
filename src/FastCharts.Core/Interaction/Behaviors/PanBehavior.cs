using FastCharts.Core.Utilities;

namespace FastCharts.Core.Interaction.Behaviors;

public sealed class PanBehavior : IBehavior
{
    private bool _dragging;
    private double _lastX;
    private double _lastY;

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
            case PointerEventType.Down:
            {
                if (ev.Button == PointerButton.Left && !ev.Modifiers.Shift)
                {
                    _dragging = true;
                    _lastX = ev.PixelX;
                    _lastY = ev.PixelY;
                    st.IsPanning = true;
                    return true;
                }
                return false;
            }
            case PointerEventType.Move:
            {
                if (_dragging)
                {
                    var dxPx = ev.PixelX - _lastX;
                    var dyPx = ev.PixelY - _lastY;

                    var m = model.PlotMargins;
                    var left = m.Left; var top = m.Top; var right = m.Right; var bottom = m.Bottom;
                    var plotW = ev.SurfaceWidth - (left + right);
                    var plotH = ev.SurfaceHeight - (top + bottom);
                    if (plotW < 0) { plotW = 0; }
                    if (plotH < 0) { plotH = 0; }

                    var vx = model.XAxis.VisibleRange;
                    var vy = model.YAxis.VisibleRange;
                    var spanX = vx.Max - vx.Min;
                    var spanY = vy.Max - vy.Min;

                    var dxData = plotW > 0 ? -dxPx / plotW * spanX : 0.0;
                    var dyData = plotH > 0 ? dyPx / plotH * spanY : 0.0;

                    if (DoubleUtils.IsNotZero(dxData) || DoubleUtils.IsNotZero(dyData))
                    {
                        model.Viewport.Pan(dxData, dyData);
                    }

                    _lastX = ev.PixelX;
                    _lastY = ev.PixelY;
                    return true;
                }
                return false;
            }
            case PointerEventType.Up:
            case PointerEventType.Leave:
            {
                if (_dragging)
                {
                    _dragging = false;
                    st.IsPanning = false;
                    return true;
                }
                return false;
            }
            case PointerEventType.Wheel:
            default:
            {
                return false;
            }
        }
    }
}
