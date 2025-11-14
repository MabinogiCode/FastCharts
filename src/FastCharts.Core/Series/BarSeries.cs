using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Series;

public sealed class BarSeries : SeriesBase, ISeriesRangeProvider
{
    public IList<BarPoint> Data { get; }
    public double? Width { get; set; }
    public double Baseline { get; set; }
    public double FillOpacity { get; set; }
    public int? GroupCount { get; set; }
    public int? GroupIndex { get; set; }
    public override bool IsEmpty => Data == null || Data.Count == 0;

    public BarSeries()
    {
        Data = new List<BarPoint>();
        Baseline = 0.0;
        FillOpacity = 0.85;
    }

    public BarSeries(IEnumerable<BarPoint> points)
    {
        Data = new List<BarPoint>(points);
        Baseline = 0.0;
        FillOpacity = 0.85;
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
        var (minX, maxX) = DataHelper.GetMinMax(Data, p => p.X);
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
        var (minY, maxY) = DataHelper.GetMinMax(
            Data,
            p => System.Math.Min(p.Y, Baseline),
            p => System.Math.Max(p.Y, Baseline));
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
