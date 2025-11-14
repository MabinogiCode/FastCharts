using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Series;

public class LineSeries : SeriesBase, ISeriesRangeProvider
{
    public IList<PointD> Data { get; }
    public override bool IsEmpty => Data == null || Data.Count == 0;
    public LineSeries()
    {
        Data = new List<PointD>();
        StrokeThickness = 1.0;
    }
    public LineSeries(IEnumerable<PointD> points)
    {
        Data = new List<PointD>(points);
        StrokeThickness = 1.0;
    }
    public FRange GetXRange()
    {
        if (Data.Count == 0)
        {
            return new FRange(0, 0);
        }
        var (min, max) = DataHelper.GetMinMax(Data, p => p.X);
        return new FRange(min, max);
    }
    public FRange GetYRange()
    {
        if (Data.Count == 0)
        {
            return new FRange(0, 0);
        }
        var (min, max) = DataHelper.GetMinMax(Data, p => p.Y);
        return new FRange(min, max);
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
