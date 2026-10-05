using FastCharts.Core.Axes;
using FastCharts.Core.Helpers;
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
        var area = PlotLayout.Compute(model, ev.SurfaceWidth, ev.SurfaceHeight);
        if (area.IsEmpty)
        {
            return false;
        }
        var plotW = area.Width;
        var plotH = area.Height;
        var px = ev.PixelX - area.Left;
        if (px < 0) { px = 0; } else if (px > plotW) { px = plotW; }
        var py = ev.PixelY - area.Top;
        if (py < 0) { py = 0; } else if (py > plotH) { py = plotH; }
        var rx = px / plotW;
        var ry = py / plotH;
        var anchorX = AxisCoordinates.FromNormalized(model.XAxis, rx);
        var anchorY = AxisCoordinates.FromNormalized(model.YAxis, 1 - ry);
        model.Viewport.Zoom(scale, scale, new PointD(anchorX, anchorY));
        return true;
    }
}
