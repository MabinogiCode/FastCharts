using System;
using System.Collections.Generic;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes.Ticks
{
    /// <summary>
    /// 1–2–5 tick generator (like OxyPlot/ScottPlot): chooses a “nice” step near the requested step.
    /// </summary>
    public sealed class NiceTicker : ITicker<double>
    {
        public IReadOnlyList<double> GetTicks(FRange visibleRange, double approxStep)
        {
            var list = new List<double>();
            double span = visibleRange.Max - visibleRange.Min;
            if (span <= 0) return list;

            // Guard approx step
            double req = approxStep;
            if (req <= 0 || double.IsNaN(req) || double.IsInfinity(req))
                req = span / 5.0;

            // Find 1–2–5 step near approxStep
            double step = NiceStep(req);

            // Align start to step
            double start = Math.Floor(visibleRange.Min / step) * step;

            // Generate ticks within an expanded guard to include edges
            // (slightly beyond to avoid off-by-one when labels sit exactly on edges)
            double end = visibleRange.Max + step * 0.5;
            for (double v = start; v <= end; v += step)
            {
                // Skip extreme values that are just outside due to FP drift
                if (v >= visibleRange.Min - step * 0.25 && v <= visibleRange.Max + step * 0.25)
                    list.Add(v);
                // Safety net to avoid infinite loops on step ~ 0
                if (list.Count > 1000) break;
            }
            return list;
        }

        public IReadOnlyList<double> GetMinorTicks(FRange range, IReadOnlyList<double> majorTicks)
        {
            var minors = new List<double>();
            if (majorTicks == null || majorTicks.Count < 2) return minors;
            double step = majorTicks[1] - majorTicks[0];
            if (step <= 0) return minors;
            double exp = Math.Floor(Math.Log10(step));
            double m = step / Math.Pow(10, exp);
            int subdiv;
            if (m < 1.5) subdiv = 5;        // 1 -> 0.2
            else if (m < 3) subdiv = 4;     // 2 -> 0.5
            else if (m < 7) subdiv = 5;     // 5 -> 1
            else subdiv = 2;                // 10 -> 5
            double minorStep = step / subdiv;
            var set = new HashSet<double>(majorTicks);
            double start = Math.Floor((range.Min - step) / minorStep) * minorStep;
            double end = range.Max + step;
            for (double v = start; v <= end; v += minorStep)
            {
                if (set.Contains(v)) continue;
                if (v >= range.Min - minorStep * 0.25 && v <= range.Max + minorStep * 0.25)
                    minors.Add(v);
                if (minors.Count > 8000) break;
            }
            return minors;
        }

        private static double NiceStep(double rough)
        {
            double sign = rough < 0 ? -1 : 1;
            rough = Math.Abs(rough);
            if (rough == 0) return 1;

            double exp = Math.Floor(Math.Log10(rough));
            double baseStep = Math.Pow(10, exp);
            double m = rough / baseStep;

            double nice;
            if (m < 1.5) nice = 1;
            else if (m < 3) nice = 2;
            else if (m < 7) nice = 5;
            else nice = 10;

            return sign * nice * baseStep;
        }
    }
}
