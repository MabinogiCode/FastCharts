using System;
using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes.Ticks
{
    /// <summary>
    /// Date/Time ticker based on OADate doubles. Chooses sensible steps from seconds to years
    /// depending on the visible span and returns ticks as OADate values.
    /// </summary>
    public sealed partial class DateTicker : ITicker<double>
    {
        public IReadOnlyList<double> GetTicks(FRange visibleRange, double approxStep)
        {
            var ticks = new List<double>();
            double span = visibleRange.Max - visibleRange.Min;
            if (span <= 0 || double.IsNaN(span) || double.IsInfinity(span))
            {
                return ticks;
            }

            var min = ClampOADate(visibleRange.Min);
            var max = ClampOADate(visibleRange.Max);
            DateTime dtMin = DateTime.FromOADate(min);
            DateTime dtMax = DateTime.FromOADate(max);

            // Choose granularity based on total days
            double days = span; // 1.0 OADate = 1 day

            TimeUnit unit;
            int stepCount;
            ChooseUnit(days, out unit, out stepCount);

            // Align start to unit boundary
            DateTime start = Align(dtMin, unit, stepCount);

            // Iterate
            for (DateTime d = start; d <= dtMax.AddSeconds(1); d = Add(d, unit, stepCount))
            {
                double oa = d.ToOADate();
                if (oa >= min - 1e-7 && oa <= max + 1e-7)
                {
                    ticks.Add(oa);
                }
                if (ticks.Count > 5000) break; // safety
            }

            return ticks;
        }

        public IReadOnlyList<double> GetMinorTicks(FRange range, IReadOnlyList<double> majorTicks)
        {
            var minors = new List<double>();
            if (majorTicks == null || majorTicks.Count < 2) return minors;
            double spanDays = range.Size;
            // Determine unit by reusing two majors
            double stepDays = majorTicks[1] - majorTicks[0];
            // Heuristic subdivisions based on step size
            int subdiv = 0;
            if (stepDays <= TimeSpan.FromHours(1).TotalDays) subdiv = 4;               // hourly -> 15 min
            else if (stepDays <= TimeSpan.FromHours(6).TotalDays) subdiv = 6;           // 6h -> 1h
            else if (stepDays <= 1.0) subdiv = spanDays < 60 ? 4 : 2;                   // daily -> 6h or 12h
            else if (stepDays <= 7.0) subdiv = 7;                                       // weekly -> daily
            else if (stepDays < 32.0) subdiv = 4;                                       // monthly -> ~weekly (approx)
            else if (stepDays < 95.0) subdiv = 3;                                       // quarterly -> monthly
            else if (stepDays < 400.0) subdiv = 4;                                      // yearly -> quarterly
            else subdiv = 2;                                                            // multi-year -> semi-year

            if (subdiv <= 1) return minors;
            double minorStep = stepDays / subdiv;
            var majorSet = new HashSet<double>(majorTicks);
            double min = range.Min - stepDays * 0.25;
            double max = range.Max + stepDays * 0.25;
            double start = Math.Floor(min / minorStep) * minorStep;
            for (double v = start; v <= max; v += minorStep)
            {
                if (majorSet.Contains(v)) continue;
                if (v >= min && v <= max) minors.Add(v);
                if (minors.Count > 10000) break;
            }
            return minors;
        }

        private enum TimeUnit { Second, Minute, Hour, Day, Month, Year }

        private static void ChooseUnit(double days, out TimeUnit unit, out int step)
        {
            if (days <= 1.0 / 24.0) { unit = TimeUnit.Minute; step = 5; return; }         // < 1h => 5 min
            if (days <= 2.0)        { unit = TimeUnit.Hour; step = 1; return; }           // <= 2d => hourly
            if (days <= 10.0)       { unit = TimeUnit.Hour; step = 6; return; }           // <= 10d => 6h
            if (days <= 40.0)       { unit = TimeUnit.Day; step = 1; return; }            // <= 40d => daily
            if (days <= 200.0)      { unit = TimeUnit.Day; step = 7; return; }            // <= ~6m => weekly
            if (days <= 800.0)      { unit = TimeUnit.Month; step = 1; return; }          // <= ~2y => monthly
            if (days <= 3650.0)     { unit = TimeUnit.Month; step = 3; return; }          // <= 10y => quarterly
            unit = TimeUnit.Year; step = 1;                                               // big spans => yearly
        }

        private static DateTime Align(DateTime t, TimeUnit unit, int step)
        {
            switch (unit)
            {
                case TimeUnit.Second:
                    return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, (t.Second / step) * step, DateTimeKind.Local);
                case TimeUnit.Minute:
                    return new DateTime(t.Year, t.Month, t.Day, t.Hour, (t.Minute / step) * step, 0, DateTimeKind.Local);
                case TimeUnit.Hour:
                    return new DateTime(t.Year, t.Month, t.Day, (t.Hour / step) * step, 0, 0, DateTimeKind.Local);
                case TimeUnit.Day:
                    // align to midnight and then to multiple of step from the start of month
                    var d0 = new DateTime(t.Year, t.Month, 1, 0, 0, 0, DateTimeKind.Local);
                    int dayIndex = Math.Max(0, t.Day - 1);
                    int alignedDayIndex = (dayIndex / step) * step;
                    alignedDayIndex = Math.Min(alignedDayIndex, DateTime.DaysInMonth(t.Year, t.Month) - 1);
                    return d0.AddDays(alignedDayIndex);
                case TimeUnit.Month:
                    return new DateTime(t.Year, ((t.Month - 1) / step) * step + 1, 1, 0, 0, 0, DateTimeKind.Local);
                case TimeUnit.Year:
                    return new DateTime((t.Year / step) * step, 1, 1, 0, 0, 0, DateTimeKind.Local);
                default:
                    return t;
            }
        }

        private static DateTime Add(DateTime t, TimeUnit unit, int step)
        {
            switch (unit)
            {
                case TimeUnit.Second: return t.AddSeconds(step);
                case TimeUnit.Minute: return t.AddMinutes(step);
                case TimeUnit.Hour:   return t.AddHours(step);
                case TimeUnit.Day:    return t.AddDays(step);
                case TimeUnit.Month:  return t.AddMonths(step);
                case TimeUnit.Year:   return t.AddYears(step);
                default: return t;
            }
        }

        private static double ClampOADate(double oa)
        {
            if (oa < DateTime.MinValue.ToOADate()) return DateTime.MinValue.ToOADate();
            if (oa > DateTime.MaxValue.ToOADate()) return DateTime.MaxValue.ToOADate();
            return oa;
        }
    }
}
