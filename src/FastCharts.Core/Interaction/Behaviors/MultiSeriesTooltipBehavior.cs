using System;
using System.Globalization;
using System.Linq;
using FastCharts.Core.Series;

namespace FastCharts.Core.Interaction.Behaviors
{
    /// <summary>
    /// Aggregates Y values from all visible series near the cursor X, builds a multi-line tooltip.
    /// Left click locks/unlocks the tooltip. ESC key (handled externally) should clear TooltipLocked.
    /// </summary>
    public sealed class MultiSeriesTooltipBehavior : IBehavior
    {
        public double XSnapToleranceFraction { get; set; } = 0.01; // fraction of visible X span
        public int MaxSeries { get; set; } = 32;
        public string LineFormat { get; set; } = "{0}: {1}"; // title : y
        public string HeaderFormat { get; set; } = "X = {0}";
        public string NumberFormatY { get; set; } = "G";
        public string NumberFormatX { get; set; } = "G";

        public bool OnEvent(ChartModel model, InteractionEvent ev)
        {
            if (model == null) return false;
            model.InteractionState ??= new InteractionState();
            var st = model.InteractionState;

            switch (ev.Type)
            {
                case PointerEventType.Down when ev.Button == PointerButton.Left:
                    // Toggle lock
                    st.TooltipLocked = !st.TooltipLocked;
                    if (!st.TooltipLocked)
                    {
                        st.TooltipAnchorX = null;
                    }
                    return true;

                case PointerEventType.Leave:
                    if (!st.TooltipLocked)
                    {
                        st.TooltipText = null;
                        st.TooltipSeries.Clear();
                    }
                    return false;

                case PointerEventType.Move:
                    if (st.TooltipLocked) return false; // do not update while locked
                    if (!st.DataX.HasValue) return false;
                    Build(model, st, st.DataX.Value);
                    return true;

                default:
                    return false;
            }
        }

        private void Build(ChartModel model, InteractionState st, double x)
        {
            var xr = model.XAxis.VisibleRange;
            double tol = xr.Size * XSnapToleranceFraction;
            var ci = CultureInfo.InvariantCulture;
            st.TooltipSeries.Clear();

            foreach (var s in model.Series)
            {
                if (st.TooltipSeries.Count >= MaxSeries) break;
                if (!s.IsVisible || s.IsEmpty) continue;
                switch (s)
                {
                    case AreaSeries area:
                        AddXY(st, area.Data, x, tol, area.Title, area.PaletteIndex);
                        break;
                    case StepLineSeries step:
                        AddXY(st, step.Data, x, tol, step.Title, step.PaletteIndex);
                        break;
                    case LineSeries ls:
                        AddXY(st, ls.Data, x, tol, ls.Title, ls.PaletteIndex);
                        break;
                    case ScatterSeries sc:
                        AddXY(st, sc.Data, x, tol, sc.Title, sc.PaletteIndex);
                        break;
                    case BarSeries bar:
                        foreach (var p in bar.Data)
                            if (Math.Abs(p.X - x) <= tol)
                                st.TooltipSeries.Add(new TooltipSeriesValue { Title = bar.Title ?? "Bar", X = p.X, Y = p.Y, PaletteIndex = bar.PaletteIndex });
                        break;
                    case StackedBarSeries sbar:
                        foreach (var p in sbar.Data)
                            if (Math.Abs(p.X - x) <= tol && p.Values != null)
                            {
                                double sum = 0;
                                for (int i = 0; i < p.Values.Length; i++) sum += p.Values[i];
                                st.TooltipSeries.Add(new TooltipSeriesValue { Title = sbar.Title ?? "Stack", X = p.X, Y = sum, PaletteIndex = sbar.PaletteIndex });
                            }
                        break;
                    case OhlcSeries ohlc:
                        foreach (var p in ohlc.Data)
                            if (Math.Abs(p.X - x) <= tol)
                                st.TooltipSeries.Add(new TooltipSeriesValue { Title = ohlc.Title ?? "OHLC", X = p.X, Y = p.Close, PaletteIndex = ohlc.PaletteIndex });
                        break;
                    case ErrorBarSeries err:
                        foreach (var p in err.Data)
                            if (Math.Abs(p.X - x) <= tol)
                                st.TooltipSeries.Add(new TooltipSeriesValue { Title = err.Title ?? "Err", X = p.X, Y = p.Y, PaletteIndex = err.PaletteIndex });
                        break;
                }
            }

            if (st.TooltipSeries.Count == 0)
            {
                st.TooltipText = null;
                return;
            }

            double anchorX = st.TooltipSeries[0].X;
            st.TooltipAnchorX = anchorX;
            string header = string.Format(ci, HeaderFormat, anchorX.ToString(NumberFormatX, ci));
            var lines = st.TooltipSeries.Select(v =>
            {
                var val = v.Y.ToString(NumberFormatY, ci);
                return string.Format(ci, LineFormat, v.Title ?? "?", val);
            });
            st.TooltipText = header + "\n" + string.Join("\n", lines);
        }

        private static void AddXY(InteractionState st, System.Collections.Generic.IList<FastCharts.Core.Primitives.PointD> data, double x, double tol, string title, int? paletteIndex)
        {
            if (data == null || data.Count == 0) return;
            // Binary search nearest by X
            int lo = 0, hi = data.Count - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (data[mid].X < x) lo = mid + 1; else hi = mid;
            }
            int idx = lo;
            // check neighbors
            for (int d = -1; d <= 1; d++)
            {
                int i = idx + d;
                if (i < 0 || i >= data.Count) continue;
                var p = data[i];
                if (System.Math.Abs(p.X - x) <= tol)
                {
                    st.TooltipSeries.Add(new TooltipSeriesValue { Title = title ?? "Line", X = p.X, Y = p.Y, PaletteIndex = paletteIndex });
                }
            }
        }
    }
}
