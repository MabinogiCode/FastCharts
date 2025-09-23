using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series;

public class LineSeries : ISeries<PointD>
{
    private readonly IReadOnlyList<PointD> _data;

    public LineSeries(IEnumerable<PointD> points)
    {
        _data = points?.ToArray() ?? Array.Empty<PointD>();
    }

    public IReadOnlyList<PointD> Data => _data;

    // UI-agnostic style hints
    public double StrokeThickness { get; set; } = 1.0;
    public double? StrokeOpacity { get; set; } = 1.0;

    public bool IsEmpty => _data.Count == 0;

    public FRange GetXRange()
    {
        if (_data.Count == 0) return new FRange(0, 0);
        var min = _data.Min(p => p.X);
        var max = _data.Max(p => p.X);
        return new FRange(min, max);
    }

    public FRange GetYRange()
    {
        if (_data.Count == 0) return new FRange(0, 0);
        var min = _data.Min(p => p.Y);
        var max = _data.Max(p => p.Y);
        return new FRange(min, max);
    }
}
