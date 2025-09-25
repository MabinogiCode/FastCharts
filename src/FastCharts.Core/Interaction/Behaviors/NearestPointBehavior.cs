using System;
using System.Linq;
using FastCharts.Core.Series;

namespace FastCharts.Core.Interaction.Behaviors
{
    /// <summary>
    /// Computes nearest data point to the cursor using pixel distance and updates InteractionState.
    /// </summary>
    public sealed class NearestPointBehavior : IBehavior
    {
        public double MaxPixelDistance { get; set; } = 24; // radius in pixels

        public bool OnEvent(ChartModel model, InteractionEvent ev)
        {
            if (model == null) return false;
            if (ev.Type != PointerEventType.Move) return false;
            if (model.InteractionState == null) model.InteractionState = new InteractionState();
            var st = model.InteractionState;

            // Plot rect from margins
            var m = model.PlotMargins;
            double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;
            double plotW = Math.Max(0, ev.SurfaceWidth - (left + right));
            double plotH = Math.Max(0, ev.SurfaceHeight - (top + bottom));
            if (plotW <= 0 || plotH <= 0)
            {
                st.ShowNearest = false; return false;
            }

            var xr = model.XAxis.VisibleRange;
            var yr = model.YAxis.VisibleRange;
            double spanX = xr.Max - xr.Min;
            double spanY = yr.Max - yr.Min;
            if (spanX <= 0 || spanY <= 0)
            {
                st.ShowNearest = false; return false;
            }

            // Cursor pixel
            double cx = ev.PixelX;
            double cy = ev.PixelY;

            double bestD2 = double.PositiveInfinity;
            double bestX = 0, bestY = 0;

            foreach (var s in model.Series)
            {
                if (s is LineSeries ls)
                {
                    foreach (var p in ls.Data)
                    {
                        double tX = (p.X - xr.Min) / spanX; if (tX < 0) tX = 0; else if (tX > 1) tX = 1;
                        double tY = (p.Y - yr.Min) / spanY; if (tY < 0) tY = 0; else if (tY > 1) tY = 1;
                        double px = left + tX * plotW;
                        double py = top + (1 - tY) * plotH;
                        double dx = px - cx, dy = py - cy;
                        double d2 = dx * dx + dy * dy;
                        if (d2 < bestD2)
                        {
                            bestD2 = d2; bestX = p.X; bestY = p.Y;
                        }
                    }
                }
                else if (s is ScatterSeries ss)
                {
                    foreach (var p in ss.Data)
                    {
                        double tX = (p.X - xr.Min) / spanX; if (tX < 0) tX = 0; else if (tX > 1) tX = 1;
                        double tY = (p.Y - yr.Min) / spanY; if (tY < 0) tY = 0; else if (tY > 1) tY = 1;
                        double px = left + tX * plotW;
                        double py = top + (1 - tY) * plotH;
                        double dx = px - cx, dy = py - cy;
                        double d2 = dx * dx + dy * dy;
                        if (d2 < bestD2)
                        {
                            bestD2 = d2; bestX = p.X; bestY = p.Y;
                        }
                    }
                }
                else if (s is BandSeries bs)
                {
                    foreach (var p in bs.Data)
                    {
                        double tX = (p.X - xr.Min) / spanX; if (tX < 0) tX = 0; else if (tX > 1) tX = 1;
                        double px = left + tX * plotW;

                        // High and low edge pixels
                        double tYh = (p.YHigh - yr.Min) / spanY; if (tYh < 0) tYh = 0; else if (tYh > 1) tYh = 1;
                        double pyh = top + (1 - tYh) * plotH;
                        double tYl = (p.YLow - yr.Min) / spanY;  if (tYl < 0) tYl = 0; else if (tYl > 1) tYl = 1;
                        double pyl = top + (1 - tYl) * plotH;

                        // If cursor is vertically inside the band at this X and horizontally close to this X,
                        // snap using vertical distance only (better UX for ribbons).
                        double minY = pyh < pyl ? pyh : pyl;
                        double maxY = pyh > pyl ? pyh : pyl;
                        double dxAbs = px > cx ? px - cx : cx - px;
                        if (cy >= minY && cy <= maxY && dxAbs <= MaxPixelDistance * 1.5)
                        {
                            double dvh = pyh > cy ? pyh - cy : cy - pyh;
                            double dvl = pyl > cy ? pyl - cy : cy - pyl;
                            double d2edge = dvh < dvl ? (dvh * dvh) : (dvl * dvl);
                            if (d2edge < bestD2)
                            {
                                bestD2 = d2edge;
                                if (dvh < dvl) { bestX = p.X; bestY = p.YHigh; }
                                else { bestX = p.X; bestY = p.YLow; }
                            }
                        }
                        else
                        {
                            // Fallback: 2D distance to both edges
                            double dxh = px - cx, dyh = pyh - cy; double d2h = dxh * dxh + dyh * dyh;
                            if (d2h < bestD2) { bestD2 = d2h; bestX = p.X; bestY = p.YHigh; }
                            double dxl = px - cx, dyl = pyl - cy; double d2l = dxl * dxl + dyl * dyl;
                            if (d2l < bestD2) { bestD2 = d2l; bestX = p.X; bestY = p.YLow; }
                        }
                    }
                }
            }

            if (double.IsInfinity(bestD2))
            {
                if (st.ShowNearest) { st.ShowNearest = false; return true; }
                return false;
            }

            if (bestD2 <= MaxPixelDistance * MaxPixelDistance)
            {
                st.ShowNearest = true;
                st.NearestDataX = bestX;
                st.NearestDataY = bestY;
                return true;
            }
            else
            {
                if (st.ShowNearest)
                {
                    st.ShowNearest = false;
                    return true;
                }
                return false;
            }
        }
    }
}
