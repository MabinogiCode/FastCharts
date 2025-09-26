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
        public IReadOnlyList<double> GetTicks(FRange range, double approxStep)
        {
            double span = range.Max - range.Min;
            var list = new List<double>(8);
            if (span <= 0)
            {
                return list;
            }
            double req = approxStep;
            if (req <= 0 || double.IsNaN(req) || double.IsInfinity(req))
            {
                req = span / 5.0;
            }
            double step = NiceStep(req);
            double start = Math.Floor(range.Min / step) * step;
            double end = range.Max + step * 0.5;
            double est = (end - start) / step + 2;
            if (est > 0 && est < 10000 && est > list.Capacity)
            {
                list.Capacity = (int)est;
            }
            for (double v = start; v <= end; v += step)
            {
                if (v >= range.Min - step * 0.25 && v <= range.Max + step * 0.25)
                {
                    list.Add(v);
                }
                if (list.Count > 1000)
                {
                    break;
                }
            }
            return list;
        }

        public IReadOnlyList<double> GetMinorTicks(FRange range, IReadOnlyList<double> majorTicks)
        {
            var minors = new List<double>();
            if (majorTicks == null || majorTicks.Count < 2)
            {
                return minors;
            }
            double step = majorTicks[1] - majorTicks[0];
            if (step <= 0)
            {
                return minors;
            }
            double exp = Math.Floor(Math.Log10(step));
            double m = step / Math.Pow(10, exp);
            int subdiv;
            if (m < 1.5) subdiv = 5;
            else if (m < 3) subdiv = 4;
            else if (m < 7) subdiv = 5;
            else subdiv = 2;
            double minorStep = step / subdiv;
            var set = new HashSet<double>(majorTicks);
            double start = Math.Floor((range.Min - step) / minorStep) * minorStep;
            double end = range.Max + step;
            for (double v = start; v <= end; v += minorStep)
            {
                if (set.Contains(v))
                {
                    continue;
                }
                if (v >= range.Min - minorStep * 0.25 && v <= range.Max + minorStep * 0.25)
                {
                    minors.Add(v);
                }
                if (minors.Count > 8000)
                {
                    break;
                }
            }
            return minors;
        }

        private static double NiceStep(double rough)
        {
            double sign = rough < 0 ? -1 : 1;
            rough = Math.Abs(rough);
            if (rough == 0)
            {
                return 1;
            }

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
