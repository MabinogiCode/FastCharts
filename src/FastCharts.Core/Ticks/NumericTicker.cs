using System;
using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Ticks;

public sealed class NumericTicker : ITicker<double>
{
    public int MinorMode { get; set; } = 0; // 0 auto 1 half 2 quarters 3 fifths

    public IReadOnlyList<double> GetTicks(FRange range, double approxStep)
    {
        var ticks = new List<double>();
        if (range.Size <= 0) return ticks;
        double span = range.Size;
        double req = approxStep > 0 ? approxStep : span / 5.0;
        double step = Nice(req);
        double start = Math.Floor(range.Min / step) * step;
        double end = range.Max + step * 0.25;
        for (double v = start; v <= end; v += step)
        {
            if (v >= range.Min - step * 0.25 && v <= range.Max + step * 0.25)
                ticks.Add(v);
            if (ticks.Count > 2000) break;
        }
        return ticks;
    }

    public IReadOnlyList<double> GetMinorTicks(FRange range, IReadOnlyList<double> majorTicks)
    {
        var minors = new List<double>();
        if (majorTicks == null || majorTicks.Count < 2) return minors;
        double step = majorTicks[1] - majorTicks[0];
        if (step <= 0) return minors;
        // Determine subdivision pattern 1-2-5
        int subdiv; double first = majorTicks[0];
        double m = step / Math.Pow(10, Math.Floor(Math.Log10(step)));
        if (m < 1.5) subdiv = 5; // 1 -> 5 minors (0.2 step)
        else if (m < 3) subdiv = 4; // 2 -> 4 minors (0.5 step)
        else if (m < 7) subdiv = 5; // 5 -> 5 minors (1 step) skip center overlapped
        else subdiv = 2;            // 10 -> 2 minors (5)

        double minorStep = step / subdiv;
        double min = range.Min - step * 0.25;
        double max = range.Max + step * 0.25;
        double start = Math.Floor((min - first) / minorStep) * minorStep + first;
        var majorsSet = new HashSet<double>(majorTicks);
        for (double v = start; v <= max; v += minorStep)
        {
            if (majorsSet.Contains(v)) continue;
            if (v >= min && v <= max) minors.Add(v);
            if (minors.Count > 8000) break;
        }
        return minors;
    }

    private static double Nice(double rough)
    {
        double exp = Math.Floor(Math.Log10(rough));
        double baseStep = Math.Pow(10, exp);
        double m = rough / baseStep;
        double nice;
        if (m < 1.5) nice = 1;
        else if (m < 3) nice = 2;
        else if (m < 7) nice = 5;
        else nice = 10;
        return nice * baseStep;
    }
}
