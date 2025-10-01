using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series;

public sealed class BandSeries : SeriesBase, ISeriesRangeProvider
{
    public IList<BandPoint> Data { get; }
    public double FillOpacity { get; set; }
    public override bool IsEmpty => Data == null || Data.Count == 0;
    public BandSeries()
    {
        Data = new List<BandPoint>();
        FillOpacity = 0.25;
    }
    public BandSeries(IEnumerable<BandPoint> points)
    {
        Data = new List<BandPoint>(points);
        FillOpacity = 0.25;
    }

    bool ISeriesRangeProvider.TryGetRanges(out FRange xRange, out FRange yRange)
    {
        if (IsEmpty)
        {
            xRange = default;
            yRange = default;
            return false;
        }
        var minX = Data.Min(p => p.X);
        var maxX = Data.Max(p => p.X);
        var minY = Data.Min(p => p.YLow);
        var maxY = Data.Max(p => p.YHigh);
        xRange = new FRange(minX, maxX);
        yRange = new FRange(minY, maxY);
        return true;
    }
}
