using System;
using System.Collections.Generic;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Ticks;

public sealed class NumericTicker : ITicker<double>
{
    public IReadOnlyList<double> GetTicks(FRange range, double approxStep)
    {
        var ticks = new List<double>();
        if (range.Size <= 0) return ticks;

        // crude step rounding
        var step = Math.Max(1, Math.Round(approxStep, MidpointRounding.AwayFromZero));
        var start = Math.Floor(range.Min / step) * step;
        for (var x = start; x <= range.Max; x += step)
            ticks.Add(x);

        return ticks;
    }
}
