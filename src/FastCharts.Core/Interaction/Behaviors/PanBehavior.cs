namespace FastCharts.Core.Interaction.Behaviors
{
    /// <summary>
    /// Pans the viewport while dragging with left mouse button (unless Shift is held for ZoomRect).
    /// Converts pixel delta to data delta using current visible ranges and plot margins.
    /// </summary>
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
                    if (ev.Button == PointerButton.Left && !ev.Modifiers.Shift)
                    {
                        _dragging = true;
                        _lastX = ev.PixelX;
                        _lastY = ev.PixelY;
                        st.IsPanning = true;
                        return true;
                    }
                    return false;

                case PointerEventType.Move:
                    if (_dragging)
                    {
                        double dxPx = ev.PixelX - _lastX;
                        double dyPx = ev.PixelY - _lastY;

                        var m = model.PlotMargins;
                        double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;
                        double plotW = ev.SurfaceWidth - (left + right);
                        double plotH = ev.SurfaceHeight - (top + bottom);
                        if (plotW < 0) { plotW = 0; }
                        if (plotH < 0) { plotH = 0; }

                        var vx = model.XAxis.VisibleRange;
                        var vy = model.YAxis.VisibleRange;
                        double spanX = vx.Max - vx.Min;
                        double spanY = vy.Max - vy.Min;

                        double dxData = (plotW > 0) ? -dxPx / plotW * spanX : 0.0;
                        double dyData = (plotH > 0) ?  dyPx / plotH * spanY : 0.0;

                        if (dxData != 0.0 || dyData != 0.0)
                        {
                            model.Viewport.Pan(dxData, dyData);
                        }

                        _lastX = ev.PixelX;
                        _lastY = ev.PixelY;
                        return true;
                    }
                    return false;

                case PointerEventType.Up:
                case PointerEventType.Leave:
                    if (_dragging)
                    {
                        _dragging = false;
                        st.IsPanning = false;
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }
    }
}
