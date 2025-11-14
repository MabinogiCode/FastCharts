using System;
using FastCharts.Core.Series;

namespace FastCharts.Core.Interaction.Behaviors;

public sealed class NearestPointBehavior : IBehavior
{
    public double MaxPixelDistance { get; set; } = 24;

    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        switch (ev.Type)
        {
            case PointerEventType.Move:
                {
                    break; // proceed
                }
            case PointerEventType.Down:
            case PointerEventType.Up:
            case PointerEventType.Leave:
            case PointerEventType.Wheel:
            default:
                {
                    return false;
                }
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
                    {
                        foreach (var p in ls.Data)
                        {
                            var tX = (p.X - xr.Min) / spanX;
                            if (tX < 0) { tX = 0; }
                            else if (tX > 1) { tX = 1; }
                            var tY = (p.Y - yr.Min) / spanY;
                            if (tY < 0) { tY = 0; }
                            else if (tY > 1) { tY = 1; }
                            var px = left + (tX * plotW);
                            var py = top + ((1 - tY) * plotH);
                            var dx = px - cx;
                            var dy = py - cy;
                            var d2 = (dx * dx) + (dy * dy);
                            if (d2 < bestD2)
                            {
                                bestD2 = d2;
                                bestX = p.X;
                                bestY = p.Y;
                            }
                        }
                        break;
                    }
                case ScatterSeries ss:
                    {
                        foreach (var p in ss.Data)
                        {
                            var tX = (p.X - xr.Min) / spanX;
                            if (tX < 0) { tX = 0; }
                            else if (tX > 1) { tX = 1; }
                            var tY = (p.Y - yr.Min) / spanY;
                            if (tY < 0) { tY = 0; }
                            else if (tY > 1) { tY = 1; }
                            var px = left + (tX * plotW);
                            var py = top + ((1 - tY) * plotH);
                            var dx = px - cx;
                            var dy = py - cy;
                            var d2 = (dx * dx) + (dy * dy);
                            if (d2 < bestD2)
                            {
                                bestD2 = d2;
                                bestX = p.X;
                                bestY = p.Y;
                            }
                        }
                        break;
                    }
                case BandSeries bs:
                    {
                        foreach (var p in bs.Data)
                        {
                            var tX = (p.X - xr.Min) / spanX;
                            if (tX < 0) { tX = 0; }
                            else if (tX > 1) { tX = 1; }
                            var px = left + (tX * plotW);
                            var tYh = (p.YHigh - yr.Min) / spanY;
                            if (tYh < 0) { tYh = 0; }
                            else if (tYh > 1) { tYh = 1; }
                            var pyh = top + ((1 - tYh) * plotH);
                            var tYl = (p.YLow - yr.Min) / spanY;
                            if (tYl < 0) { tYl = 0; }
                            else if (tYl > 1) { tYl = 1; }
                            var pyl = top + ((1 - tYl) * plotH);
                            var minY = pyh < pyl ? pyh : pyl;
                            var maxY = pyh > pyl ? pyh : pyl;
                            var dxAbs = px > cx ? px - cx : cx - px;
                            if ((cy >= minY) && (cy <= maxY) && (dxAbs <= (MaxPixelDistance * 1.5)))
                            {
                                var dvh = pyh > cy ? pyh - cy : cy - pyh;
                                var dvl = pyl > cy ? pyl - cy : cy - pyl;
                                var d2edge = dvh < dvl ? (dvh * dvh) : (dvl * dvl);
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
                                var dxh = px - cx;
                                var dyh = pyh - cy;
                                var d2h = (dxh * dxh) + (dyh * dyh);
                                if (d2h < bestD2)
                                {
                                    bestD2 = d2h;
                                    bestX = p.X;
                                    bestY = p.YHigh;
                                }
                                var dxl = px - cx;
                                var dyl = pyl - cy;
                                var d2l = (dxl * dxl) + (dyl * dyl);
                                if (d2l < bestD2)
                                {
                                    bestD2 = d2l;
                                    bestX = p.X;
                                    bestY = p.YLow;
                                }
                            }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
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
}
