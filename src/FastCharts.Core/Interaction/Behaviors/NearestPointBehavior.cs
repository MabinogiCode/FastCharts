using System;
using FastCharts.Core.Series;

namespace FastCharts.Core.Interaction.Behaviors;

public sealed class NearestPointBehavior : IBehavior
{
    public double MaxPixelDistance { get; set; } = 24;

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
        var left = m.Left;
        var top = m.Top;
        var right = m.Right;
        var bottom = m.Bottom;
        var plotW = Math.Max(0, ev.SurfaceWidth - (left + right));
        var plotH = Math.Max(0, ev.SurfaceHeight - (top + bottom));

        if (plotW <= 0 || plotH <= 0)
        {
            st.ShowNearest = false;
            return false;
        }

        var xr = model.XAxis.VisibleRange;
        var yr = model.YAxis.VisibleRange;
        var spanX = xr.Max - xr.Min;
        var spanY = yr.Max - yr.Min;

        if (spanX <= 0 || spanY <= 0)
        {
            st.ShowNearest = false;
            return false;
        }

        var cx = ev.PixelX;
        var cy = ev.PixelY;
        var bestD2 = double.PositiveInfinity;
        double bestX = 0;
        double bestY = 0;

        foreach (var s in model.Series)
        {
            switch (s)
            {
                case LineSeries ls:
                    ProcessLineSeries(ls, xr, yr, spanX, spanY, left, top, plotW, plotH, cx, cy, ref bestD2, ref bestX, ref bestY);
                    break;
                case ScatterSeries ss:
                    ProcessScatterSeries(ss, xr, yr, spanX, spanY, left, top, plotW, plotH, cx, cy, ref bestD2, ref bestX, ref bestY);
                    break;
                case BandSeries bs:
                    ProcessBandSeries(bs, xr, yr, spanX, spanY, left, top, plotW, plotH, cx, cy, ref bestD2, ref bestX, ref bestY);
                    break;
                default:
                    break;
            }
        }

        if (double.IsInfinity(bestD2))
        {
            if (st.ShowNearest)
            {
                st.ShowNearest = false;
                return true;
            }
            return false;
        }

        if (bestD2 <= (MaxPixelDistance * MaxPixelDistance))
        {
            st.ShowNearest = true;
            st.NearestDataX = bestX;
            st.NearestDataY = bestY;
            return true;
        }

        if (st.ShowNearest)
        {
            st.ShowNearest = false;
            return true;
        }

        return false;
    }

    private static void ProcessLineSeries(LineSeries ls, Primitives.FRange xr, Primitives.FRange yr,
        double spanX, double spanY, double left, double top, double plotW, double plotH,
        double cx, double cy, ref double bestD2, ref double bestX, ref double bestY)
    {
        foreach (var p in ls.Data)
        {
            ProcessPoint(p.X, p.Y, xr, yr, spanX, spanY, left, top, plotW, plotH, cx, cy, ref bestD2, ref bestX, ref bestY);
        }
    }

    private static void ProcessScatterSeries(ScatterSeries ss, Primitives.FRange xr, Primitives.FRange yr,
        double spanX, double spanY, double left, double top, double plotW, double plotH,
        double cx, double cy, ref double bestD2, ref double bestX, ref double bestY)
    {
        foreach (var p in ss.Data)
        {
            ProcessPoint(p.X, p.Y, xr, yr, spanX, spanY, left, top, plotW, plotH, cx, cy, ref bestD2, ref bestX, ref bestY);
        }
    }

    private void ProcessBandSeries(BandSeries bs, Primitives.FRange xr, Primitives.FRange yr,
        double spanX, double spanY, double left, double top, double plotW, double plotH,
        double cx, double cy, ref double bestD2, ref double bestX, ref double bestY)
    {
        foreach (var p in bs.Data)
        {
            var tX = Math.Max(0, Math.Min(1, (p.X - xr.Min) / spanX));
            var px = left + (tX * plotW);

            var tYh = Math.Max(0, Math.Min(1, (p.YHigh - yr.Min) / spanY));
            var pyh = top + ((1 - tYh) * plotH);

            var tYl = Math.Max(0, Math.Min(1, (p.YLow - yr.Min) / spanY));
            var pyl = top + ((1 - tYl) * plotH);

            var minY = Math.Min(pyh, pyl);
            var maxY = Math.Max(pyh, pyl);
            var dxAbs = Math.Abs(px - cx);

            if ((cy >= minY) && (cy <= maxY) && (dxAbs <= (MaxPixelDistance * 1.5)))
            {
                var dvh = Math.Abs(pyh - cy);
                var dvl = Math.Abs(pyl - cy);
                var d2edge = Math.Min(dvh * dvh, dvl * dvl);

                if (d2edge < bestD2)
                {
                    bestD2 = d2edge;
                    if (dvh < dvl)
                    {
                        bestX = p.X;
                        bestY = p.YHigh;
                    }
                    else
                    {
                        bestX = p.X;
                        bestY = p.YLow;
                    }
                }
            }
            else
            {
                // Check both high and low points
                ProcessPoint(p.X, p.YHigh, xr, yr, spanX, spanY, left, top, plotW, plotH, cx, cy, ref bestD2, ref bestX, ref bestY);
                ProcessPoint(p.X, p.YLow, xr, yr, spanX, spanY, left, top, plotW, plotH, cx, cy, ref bestD2, ref bestX, ref bestY);
            }
        }
    }

    private static void ProcessPoint(double dataX, double dataY, Primitives.FRange xr, Primitives.FRange yr,
        double spanX, double spanY, double left, double top, double plotW, double plotH,
        double cx, double cy, ref double bestD2, ref double bestX, ref double bestY)
    {
        var tX = Math.Max(0, Math.Min(1, (dataX - xr.Min) / spanX));
        var tY = Math.Max(0, Math.Min(1, (dataY - yr.Min) / spanY));
        var px = left + (tX * plotW);
        var py = top + ((1 - tY) * plotH);
        var dx = px - cx;
        var dy = py - cy;
        var d2 = (dx * dx) + (dy * dy);

        if (d2 < bestD2)
        {
            bestD2 = d2;
            bestX = dataX;
            bestY = dataY;
        }
    }
}
