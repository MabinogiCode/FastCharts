using System;
using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes.Ticks;

public sealed class DateTicker : ITicker<double>
{
    public IReadOnlyList<double> GetTicks(FRange range, double approxStep)
    {
        var span = range.Max - range.Min;
        var ticks = new List<double>(16);
        if ((span <= 0) || double.IsNaN(span) || double.IsInfinity(span))
        {
            return ticks;
        }
        var min = DateTickerHelper.ClampOADate(range.Min);
        var max = DateTickerHelper.ClampOADate(range.Max);
        var dtMin = DateTime.FromOADate(min);
        var dtMax = DateTime.FromOADate(max);
        var days = span;
        DateTickerHelper.ChooseUnit(days, out var unit, out var stepCount);
        var start = DateTickerHelper.Align(dtMin, unit, stepCount);
        var approxStepDays = (dtMax - dtMin).TotalDays / 10.0;
        if ((unit == TimeUnit.Day) && (stepCount > 0))
        {
            approxStepDays = stepCount;
        }
        if (unit == TimeUnit.Month)
        {
            approxStepDays = 30 * stepCount;
        }
        if (unit == TimeUnit.Year)
        {
            approxStepDays = 365 * stepCount;
        }
        if (approxStepDays > 0)
        {
            var est = (days / approxStepDays) + 4;
            if ((est > ticks.Capacity) && (est < 6000))
            {
                ticks.Capacity = (int)est;
            }
        }
        for (var d = start; d <= dtMax.AddSeconds(1); d = DateTickerHelper.Add(d, unit, stepCount))
        {
            var oa = d.ToOADate();
            if ((oa >= (min - 1e-7)) && (oa <= (max + 1e-7)))
            {
                ticks.Add(oa);
            }
            if (ticks.Count > 5000)
            {
                break;
            }
        }
        return ticks;
    }

    public IReadOnlyList<double> GetMinorTicks(FRange range, IReadOnlyList<double> majorTicks)
    {
        var minors = new List<double>();
        if ((majorTicks == null) || (majorTicks.Count < 2))
        {
            return minors;
        }
        var spanDays = range.Size;
        var stepDays = majorTicks[1] - majorTicks[0];
        int subdiv;
        if (stepDays <= TimeSpan.FromHours(1).TotalDays)
        {
            subdiv = 4;
        }
        else if (stepDays <= TimeSpan.FromHours(6).TotalDays)
        {
            subdiv = 6;
        }
        else if (stepDays <= 1.0)
        {
            subdiv = (spanDays < 60) ? 4 : 2;
        }
        else if (stepDays <= 7.0)
        {
            subdiv = 7;
        }
        else if (stepDays < 32.0)
        {
            subdiv = 4;
        }
        else if (stepDays < 95.0)
        {
            subdiv = 3;
        }
        else if (stepDays < 400.0)
        {
            subdiv = 4;
        }
        else
        {
            subdiv = 2;
        }

        var minorStep = stepDays / subdiv;
        var majorSet = new HashSet<double>(majorTicks);
        var min = range.Min - (stepDays * 0.25);
        var max = range.Max + (stepDays * 0.25);
        var start = Math.Floor(min / minorStep) * minorStep;
        for (var v = start; v <= max; v += minorStep)
        {
            if (majorSet.Contains(v))
            {
                continue;
            }
            if ((v >= min) && (v <= max))
            {
                minors.Add(v);
            }
            if (minors.Count > 10000)
            {
                break;
            }
        }
        return minors;
    }
}
