using System;
using System.Linq;
using FastCharts.Core.Series;

namespace FastCharts.Core.Interaction.Behaviors
{
    /// <summary>
    /// Computes nearest data point to the cursor (in DATA space) on Move and updates InteractionState.
    /// Does not render; renderer may draw a marker.
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

            if (!st.DataX.HasValue || !st.DataY.HasValue)
            {
                st.ShowNearest = false;
                return false; // host may call UpdateDataCoordsForTooltip before behaviors
            }

            double bestDistSq = double.PositiveInfinity;
            double bestX = 0, bestY = 0;

            // Consider Line-like series and Scatter
            foreach (var s in model.Series)
            {
                switch (s)
                {
                    case LineSeries ls:
                        foreach (var p in ls.Data)
                        {
                            double dx = p.X - st.DataX.Value;
                            double dy = p.Y - st.DataY.Value;
                            double d2 = dx * dx + dy * dy;
                            if (d2 < bestDistSq)
                            {
                                bestDistSq = d2; bestX = p.X; bestY = p.Y;
                            }
                        }
                        break;
                    case ScatterSeries ss:
                        foreach (var p in ss.Data)
                        {
                            double dx = p.X - st.DataX.Value;
                            double dy = p.Y - st.DataY.Value;
                            double d2 = dx * dx + dy * dy;
                            if (d2 < bestDistSq)
                            {
                                bestDistSq = d2; bestX = p.X; bestY = p.Y;
                            }
                        }
                        break;
                }
            }

            if (double.IsInfinity(bestDistSq))
            {
                st.ShowNearest = false;
                return false;
            }

            // Rough pixel distance threshold using axis scales
            var m = model.PlotMargins;
            double left = m.Left, top = m.Top, right = m.Right, bottom = m.Bottom;
            double plotW = Math.Max(0, ev.SurfaceWidth - (left + right));
            double plotH = Math.Max(0, ev.SurfaceHeight - (top + bottom));
            if (plotW <= 0 || plotH <= 0)
            {
                st.ShowNearest = false;
                return false;
            }

            var xr = model.XAxis.VisibleRange;
            var yr = model.YAxis.VisibleRange;

            // Convert threshold in data units (approx, using current spans)
            double dxData = (MaxPixelDistance / plotW) * (xr.Max - xr.Min);
            double dyData = (MaxPixelDistance / plotH) * (yr.Max - yr.Min);
            double maxD2 = dxData * dxData + dyData * dyData;

            if (bestDistSq <= maxD2)
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
                    return true; // request redraw to clear highlight
                }
            }

            return false;
        }
    }
}
