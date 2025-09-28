using System.Collections.Generic;
using FastCharts.Core.Primitives;
using System.Linq;

namespace FastCharts.Core.Series;

public sealed class OhlcSeries : SeriesBase
{
    public IList<OhlcPoint> Data { get; }
    public double? Width { get; set; }
    public double WickThickness { get; set; } = 1.0;
    public double UpFillOpacity { get; set; } = 0.9;
    public double DownFillOpacity { get; set; } = 0.4;
    public bool Filled { get; set; } = true;
    public override bool IsEmpty => Data == null || Data.Count == 0;

    public OhlcSeries() => Data = new List<OhlcPoint>();
    public OhlcSeries(IEnumerable<OhlcPoint> points) => Data = new List<OhlcPoint>(points);

    public double GetWidthFor(int _)
    {
        if (Width.HasValue)
        {
            return Width.Value;
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
            return minDx * 0.6;
        }
        return 1.0;
    }

    public FRange GetXRange()
    {
        if (IsEmpty)
        {
            return new FRange(0, 0);
        }
        var minX = Data.Min(p => p.X);
        var maxX = Data.Max(p => p.X);
        var half = GetWidthFor(0) * 0.5;
        return new FRange(minX - half, maxX + half);
    }

    public FRange GetYRange()
    {
        if (IsEmpty)
        {
            return new FRange(0, 0);
        }
        var minY = Data.Min(p => p.Low);
        var maxY = Data.Max(p => p.High);
        return new FRange(minY, maxY);
    }
}
