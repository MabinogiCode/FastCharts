using System;
using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Ticks;

public sealed class NumericTicker : ITicker<double>
{
    public int MinorMode { get; set; }

    public IReadOnlyList<double> GetTicks(FRange range, double approxStep)
    {
        var ticks = new List<double>();
        if (range.Size <= 0)
        {
            return ticks;
        }
        var span = range.Size;
        var req = approxStep > 0 ? approxStep : span / 5.0;
        var step = NumericTickerHelper.CalculateNiceStep(req);
        var start = Math.Floor(range.Min / step) * step;
        var end = range.Max + step * 0.25;
        for (var v = start; v <= end; v += step)
        {
            if (v >= range.Min - step * 0.25 && v <= range.Max + step * 0.25)
            {
                ticks.Add(v);
            }
            if (ticks.Count > 2000)
            {
                break;
            }
        }
        return ticks;
    }

    public IReadOnlyList<double> GetMinorTicks(FRange range, IReadOnlyList<double> majorTicks)
    {
        var minors = new List<double>();
        if (majorTicks == null || majorTicks.Count < 2)
        {
            return minors;
        }
        var step = majorTicks[1] - majorTicks[0];
        if (step <= 0)
        {
            return minors;
        }
        var m = step / Math.Pow(10, Math.Floor(Math.Log10(step)));
        int subdiv;
        if (m < 1.5)
        {
            subdiv = 5; // 1 -> 5 minors
        }
        else if (m < 3)
        {
            subdiv = 4; // 2 -> 4 minors
        }
        else if (m < 7)
        {
            subdiv = 5; // 5 -> 5 minors
        }
        else
        {
            subdiv = 2; // 10 -> 2 minors
        }
        var minorStep = step / subdiv;
        var min = range.Min - step * 0.25;
        var max = range.Max + step * 0.25;
        var first = majorTicks[0];
        var start = Math.Floor((min - first) / minorStep) * minorStep + first;
        var majorsSet = new HashSet<double>(majorTicks);
        for (var v = start; v <= max; v += minorStep)
        {
            if (majorsSet.Contains(v))
            {
                continue;
            }
            if (v >= min && v <= max)
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
}
