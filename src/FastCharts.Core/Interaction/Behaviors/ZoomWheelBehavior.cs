using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Interaction.Behaviors;

public sealed class ZoomWheelBehavior : IBehavior
{
    public double Step { get; set; } = 1.1;
    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        if (ev.Type != PointerEventType.Wheel)
        {
            return false;
        }
        var zoomIn = ev.WheelDelta > 0;
        var scale = zoomIn ? Step : 1.0 / Step;
        var m = model.PlotMargins;
        var left = m.Left; var top = m.Top; var right = m.Right; var bottom = m.Bottom;
        var plotW = ev.SurfaceWidth - (left + right);
        var plotH = ev.SurfaceHeight - (top + bottom);
        if (plotW <= 0 || plotH <= 0)
        {
            return false;
        }
        var px = ev.PixelX - left;
        if (px < 0) { px = 0; } else if (px > plotW) { px = plotW; }
        var py = ev.PixelY - top;
        if (py < 0) { py = 0; } else if (py > plotH) { py = plotH; }
        var rx = px / plotW;
        var ry = py / plotH;
        var anchorX = AxisCoordinates.FromNormalized(model.XAxis, rx);
        var anchorY = AxisCoordinates.FromNormalized(model.YAxis, 1 - ry);
        model.Viewport.Zoom(scale, scale, new PointD(anchorX, anchorY));
        return true;
    }
}
