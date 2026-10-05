using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Series;

namespace FastCharts.Core.Interaction.Behaviors;

/// <summary>
/// Behavior that tracks the nearest data point to the mouse cursor
/// Updates InteractionState with the closest point within MaxPixelDistance
/// </summary>
public sealed class NearestPointBehavior : IBehavior
{
    /// <summary>
    /// Gets or sets the maximum pixel distance to consider a point as "nearest"
    /// </summary>
    public double MaxPixelDistance { get; set; } = 24;

    /// <summary>
    /// Handles pointer events to find and track the nearest data point
    /// </summary>
    /// <param name="model">Chart model containing series data</param>
    /// <param name="ev">Pointer event to process</param>
    /// <returns>True if the event was handled and requires a redraw</returns>
    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        // Only handle Move events, ignore all others
        if (ev.Type != PointerEventType.Move)
        {
            return false;
        }

        model.InteractionState ??= new InteractionState();
        var st = model.InteractionState;

        var m = model.PlotMargins;
        var plotW = Math.Max(0, ev.SurfaceWidth - (m.Left + m.Right));
        var plotH = Math.Max(0, ev.SurfaceHeight - (m.Top + m.Bottom));

        if (plotW <= 0 || plotH <= 0 || model.XAxis.VisibleRange.Size <= 0)
        {
            st.ShowNearest = false;
            return false;
        }

        var plot = new PlotProjection(m.Left, m.Top, plotW, plotH, ev.PixelX, ev.PixelY);
        var best = new NearestCandidate();

        foreach (var s in model.Series)
        {
            // Hidden series (e.g. toggled off in the legend) must not be snapped to
            if (!s.IsVisible || s.IsEmpty)
            {
                continue;
            }

            var useSecondary = s.YAxisIndex == 1 && model.YAxisSecondary != null;
            var yAxis = useSecondary ? model.YAxisSecondary! : model.YAxis;
            if (yAxis.VisibleRange.Size <= 0)
            {
                continue;
            }

            var axisIndex = useSecondary ? 1 : 0;
            switch (s)
            {
                case LineSeries ls:
                    foreach (var p in ls.Data)
                    {
                        best.Consider(plot.DistanceSquared(model.XAxis, yAxis, p.X, p.Y), p.X, p.Y, axisIndex);
                    }

                    break;
                case ScatterSeries ss:
                    foreach (var p in ss.Data)
                    {
                        best.Consider(plot.DistanceSquared(model.XAxis, yAxis, p.X, p.Y), p.X, p.Y, axisIndex);
                    }

                    break;
                case BandSeries bs:
                    ProcessBandSeries(bs, model.XAxis, yAxis, plot, axisIndex, ref best);
                    break;
                default:
                    break;
            }
        }

        if (best.Found && best.DistanceSquared <= (MaxPixelDistance * MaxPixelDistance))
        {
            st.ShowNearest = true;
            st.NearestDataX = best.X;
            st.NearestDataY = best.Y;
            st.NearestYAxisIndex = best.AxisIndex;
            return true;
        }

        if (st.ShowNearest)
        {
            st.ShowNearest = false;
            return true;
        }

        return false;
    }

    private void ProcessBandSeries(BandSeries bs, IAxis<double> xAxis, IAxis<double> yAxis, PlotProjection plot, int axisIndex, ref NearestCandidate best)
    {
        foreach (var p in bs.Data)
        {
            if (!plot.TryProject(xAxis, yAxis, p.X, p.YHigh, out var px, out var pyh) ||
                !plot.TryProject(xAxis, yAxis, p.X, p.YLow, out _, out var pyl))
            {
                continue;
            }

            var minY = Math.Min(pyh, pyl);
            var maxY = Math.Max(pyh, pyl);
            var dxAbs = Math.Abs(px - plot.CursorX);

            if ((plot.CursorY >= minY) && (plot.CursorY <= maxY) && (dxAbs <= (MaxPixelDistance * 1.5)))
            {
                // Cursor inside the band: snap to the closest edge
                var dvh = Math.Abs(pyh - plot.CursorY);
                var dvl = Math.Abs(pyl - plot.CursorY);
                if (dvh < dvl)
                {
                    best.Consider(dvh * dvh, p.X, p.YHigh, axisIndex);
                }
                else
                {
                    best.Consider(dvl * dvl, p.X, p.YLow, axisIndex);
                }
            }
            else
            {
                best.Consider(plot.DistanceSquared(xAxis, yAxis, p.X, p.YHigh), p.X, p.YHigh, axisIndex);
                best.Consider(plot.DistanceSquared(xAxis, yAxis, p.X, p.YLow), p.X, p.YLow, axisIndex);
            }
        }
    }
}
