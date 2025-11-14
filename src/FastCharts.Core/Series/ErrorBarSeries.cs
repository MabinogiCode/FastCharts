using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Abstractions;

namespace FastCharts.Core.Series;

/// <summary>
/// Error bar series around a central Y value at X, with symmetric or asymmetric errors.
/// </summary>
public sealed class ErrorBarSeries : SeriesBase, ISeriesRangeProvider
{
    public IList<ErrorBarPoint> Data { get; }
    public double? CapWidth { get; set; }
    public override bool IsEmpty => Data == null || Data.Count == 0;

    public ErrorBarSeries()
    {
        Data = new List<ErrorBarPoint>();
        StrokeThickness = 1.2;
    }

    public ErrorBarSeries(IEnumerable<ErrorBarPoint> points)
    {
        Data = new List<ErrorBarPoint>(points);
        StrokeThickness = 1.2;
    }

    public double GetCapWidth()
    {
        if (CapWidth.HasValue)
        {
            return CapWidth.Value;
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
            return minDx * 0.25;
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
        var half = GetCapWidth() * 0.5;
        return new FRange(minX - half, maxX + half);
    }

    public FRange GetYRange()
    {
        if (IsEmpty)
        {
            return new FRange(0, 0);
        }
        var minY = double.MaxValue;
        var maxY = double.MinValue;
        foreach (var point in Data)
        {
            var yMin = point.Y - (point.NegativeError ?? point.PositiveError);
            var yMax = point.Y + point.PositiveError;
            if (yMin < minY) minY = yMin;
            if (yMax > maxY) maxY = yMax;
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
