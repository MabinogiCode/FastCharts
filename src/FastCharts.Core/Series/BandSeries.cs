using System.Collections.Generic;

namespace FastCharts.Core.Series;

public sealed class BandSeries : SeriesBase
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
}
