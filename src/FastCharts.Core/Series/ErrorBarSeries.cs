using System.Collections.Generic;
using System.Linq;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Error bar series around a central Y value at X, with symmetric or asymmetric errors.
    /// </summary>
    public sealed class ErrorBarSeries : SeriesBase
    {
        public IList<ErrorBarPoint> Data { get; }

        /// <summary>Cap width in data units (if null, inferred from min ΔX * 0.25).</summary>
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
            if (CapWidth.HasValue) return CapWidth.Value;
            if (Data.Count >= 2)
            {
                double minDx = double.PositiveInfinity;
                for (int i = 1; i < Data.Count; i++)
                {
                    double dx = System.Math.Abs(Data[i].X - Data[i - 1].X);
                    if (dx > 0 && dx < minDx) minDx = dx;
                }
                if (double.IsInfinity(minDx) || minDx <= 0) return 1.0;
                return minDx * 0.25;
            }
            return 1.0;
        }

        public FastCharts.Core.Primitives.FRange GetXRange()
        {
            if (IsEmpty) return new FastCharts.Core.Primitives.FRange(0, 0);
            double minX = Data.Min(p => p.X);
            double maxX = Data.Max(p => p.X);
            double half = GetCapWidth() * 0.5;
            return new FastCharts.Core.Primitives.FRange(minX - half, maxX + half);
        }

        public FastCharts.Core.Primitives.FRange GetYRange()
        {
            if (IsEmpty) return new FastCharts.Core.Primitives.FRange(0, 0);
            double minY = Data.Min(p => p.Y - (p.NegativeError ?? p.PositiveError));
            double maxY = Data.Max(p => p.Y + p.PositiveError);
            return new FastCharts.Core.Primitives.FRange(minY, maxY);
        }
    }

    public struct ErrorBarPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        /// <summary>Positive error (always required).</summary>
        public double PositiveError { get; set; }
        /// <summary>Optional negative error; if null, symmetric = PositiveError.</summary>
        public double? NegativeError { get; set; }

        public ErrorBarPoint(double x, double y, double positiveError, double? negativeError = null)
        {
            X = x; Y = y; PositiveError = positiveError; NegativeError = negativeError;
        }
    }
}
