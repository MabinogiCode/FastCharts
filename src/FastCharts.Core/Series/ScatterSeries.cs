using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Series;

public sealed class ScatterSeries : SeriesBase, ISeriesRangeProvider
{
    public IList<PointD> Data { get; }
    public double MarkerSize { get; set; }
    public MarkerShape MarkerShape { get; set; }
    public override bool IsEmpty => Data == null || Data.Count == 0;

    public ScatterSeries()
    {
        Data = new List<PointD>();
        MarkerSize = 5.0;
        MarkerShape = MarkerShape.Circle;
    }

    public ScatterSeries(IEnumerable<PointD> points)
    {
        Data = new List<PointD>(points);
        MarkerSize = 5.0;
        MarkerShape = MarkerShape.Circle;
    }

    public FRange GetXRange()
    {
        if (IsEmpty)
        {
            return new FRange(0, 0);
        }
        var (min, max) = DataHelper.GetMinMax(Data, p => p.X);
        return new FRange(min, max);
    }

    public FRange GetYRange()
    {
        if (IsEmpty)
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
