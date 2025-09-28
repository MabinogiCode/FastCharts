using System;
using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes.Ticks;

public sealed class NiceTicker : ITicker<double>
{
    public IReadOnlyList<double> GetTicks(FRange range, double approxStep)
    {
        var span = range.Max - range.Min;
        var list = new List<double>(8);
        if (span <= 0)
        {
            return list;
        }
        var req = approxStep;
        if (req <= 0 || double.IsNaN(req) || double.IsInfinity(req))
        {
            req = span / 5.0;
        }
        var step = NiceTickerHelper.CalculateNiceStep(req);
        var start = Math.Floor(range.Min / step) * step;
        var end = range.Max + (step * 0.5);
        var est = ((end - start) / step) + 2;
        if (est > 0 && est < 10000 && est > list.Capacity)
        {
            list.Capacity = (int)est;
        }
        for (var v = start; v <= end; v += step)
        {
            if (v >= (range.Min - (step * 0.25)) && v <= (range.Max + (step * 0.25)))
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
        var step = majorTicks[1] - majorTicks[0];
        if (step <= 0)
        {
            return minors;
        }
        var exp = Math.Floor(Math.Log10(step));
        var m = step / Math.Pow(10, exp);
        int subdiv;
        if (m < 1.5)
        {
            subdiv = 5;
        }
        else if (m < 3)
        {
            subdiv = 4;
        }
        else if (m < 7)
        {
            subdiv = 5;
        }
        else
        {
            subdiv = 2;
        }
        var minorStep = step / subdiv;
        var set = new HashSet<double>(majorTicks);
        var start = Math.Floor((range.Min - step) / minorStep) * minorStep;
        var end = range.Max + step;
        for (var v = start; v <= end; v += minorStep)
        {
            if (set.Contains(v))
            {
                continue;
            }
            if (v >= (range.Min - (minorStep * 0.25)) && v <= (range.Max + (minorStep * 0.25)))
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
