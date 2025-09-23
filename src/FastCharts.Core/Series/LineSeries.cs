using System.Collections.Generic;
using System.Linq;

using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series;

public class LineSeries : SeriesBase
{

    public IList<PointD> Data { get; }

    public override bool IsEmpty => Equals(Data, null) || Data.Count == 0;

    public LineSeries()
    {
        Data = new List<PointD>();
    }

    public LineSeries(IEnumerable<PointD> points)
    {
        Data = new List<PointD>(points);
    }


    // UI-agnostic style hints
    public double StrokeThickness { get; set; } = 1.0;


    public FRange GetXRange()
    {
        if (Data.Count == 0)
        {
            return new FRange(0, 0);
        }
        var min = Data.Min(p => p.X);
        var max = Data.Max(p => p.X);
        return new FRange(min, max);
    }

    public FRange GetYRange()
    {
        if (Data.Count == 0)
        {
            return new FRange(0, 0);
        }
        var min = Data.Min(p => p.Y);
        var max = Data.Max(p => p.Y);
        return new FRange(min, max);
    }
}
