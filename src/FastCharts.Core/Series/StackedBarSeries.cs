using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Abstractions;

namespace FastCharts.Core.Series;

public sealed class StackedBarSeries : SeriesBase, ISeriesRangeProvider
{
    public IList<StackedBarPoint> Data { get; }
    public double? Width { get; set; }
    public double Baseline { get; set; }
    public double FillOpacity { get; set; } = 0.85;
    public int? GroupCount { get; set; }
    public int? GroupIndex { get; set; }
    public override bool IsEmpty => Data == null || Data.Count == 0;
    public StackedBarSeries()
    {
        Data = new List<StackedBarPoint>();
        Baseline = 0.0;
    }
    public StackedBarSeries(IEnumerable<StackedBarPoint> points)
    {
        Data = new List<StackedBarPoint>(points);
        Baseline = 0.0;
    }
    public double GetWidthFor(int index)
    {
        if (Width.HasValue)
        {
            return Width.Value;
        }
        if (index >= 0 && index < Data.Count)
        {
            var w = Data[index].Width;
            if (w.HasValue && w.Value > 0)
            {
                return w.Value;
            }
        }
        if (Data.Count >= 2)
        {
            var minDx = double.PositiveInfinity;
            for (var i = 1; i < Data.Count; i++)
            {
                var dx = System.Math.Abs(Data[i].X - Data[i - 1].X);
                if (dx > 0 && dx < minDx)
                {
                    minDx = dx;
                }
            }
            if (double.IsInfinity(minDx) || minDx <= 0)
            {
                return 1.0;
            }
            return minDx * 0.8;
        }
        return 1.0;
    }
    public FRange GetXRange()
    {
        if (IsEmpty)
        {
            return new FRange(0, 0);
        }
        var minX = double.MaxValue;
        var maxX = double.MinValue;
        foreach (var point in Data)
        {
            if (point.X < minX) minX = point.X;
            if (point.X > maxX) maxX = point.X;
        }
        var w0 = GetWidthFor(0) * 0.5;
        var wN = GetWidthFor(Data.Count - 1) * 0.5;
        return new FRange(minX - w0, maxX + wN);
    }
    public FRange GetYRange()
    {
        if (IsEmpty)
        {
            return new FRange(0, 0);
        }
        var minY = Baseline;
        var maxY = Baseline;
        foreach (var p in Data)
        {
            var pos = 0.0;
            var neg = 0.0;
            if (p.Values != null)
            {
                for (var i = 0; i < p.Values.Length; i++)
                {
                    var v = p.Values[i];
                    if (v >= 0)
                    {
                        pos += v;
                    }
                    else
                    {
                        neg += v;
                    }
                }
            }
            var top = System.Math.Max(Baseline, Baseline + pos);
            var bot = System.Math.Min(Baseline, Baseline + neg);
            if (top > maxY)
            {
                maxY = top;
            }
            if (bot < minY)
            {
                minY = bot;
            }
        }
        return new FRange(minY, maxY);
    }
    bool ISeriesRangeProvider.TryGetRanges(out FRange xRange, out FRange yRange)
    {
        if (IsEmpty)
        {
            xRange = default;
            yRange = default;
            return false;
        }
        xRange = GetXRange();
        yRange = GetYRange();
        return true;
    }
}
