using System.Collections.Generic;
using System.Linq;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Vertical bar (column) series. Each data point draws a rectangle from Baseline to Y at X.
    /// Width is inferred if not specified (80% min ΔX).
    /// </summary>
    public sealed class BarSeries : SeriesBase
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
            if (Width.HasValue) return Width.Value;
            if (index >= 0 && index < Data.Count)
            {
                var w = Data[index].Width;
                if (w.HasValue && w.Value > 0) return w.Value;
            }
            if (Data.Count >= 2)
            {
                double minDx = double.PositiveInfinity;
                for (int i = 1; i < Data.Count; i++)
                {
                    double dx = System.Math.Abs(Data[i].X - Data[i - 1].X);
                    if (dx > 0 && dx < minDx) minDx = dx;
                }
                if (double.IsInfinity(minDx) || minDx <= 0) return 1.0;
                return minDx * 0.8;
            }
            return 1.0;
        }

        public FastCharts.Core.Primitives.FRange GetXRange()
        {
            if (IsEmpty) return new FastCharts.Core.Primitives.FRange(0, 0);
            double minX = Data.Min(p => p.X);
            double maxX = Data.Max(p => p.X);
            double w0 = GetWidthFor(0) * 0.5;
            double wN = GetWidthFor(Data.Count - 1) * 0.5;
            return new FastCharts.Core.Primitives.FRange(minX - w0, maxX + wN);
        }

        public FastCharts.Core.Primitives.FRange GetYRange()
        {
            if (IsEmpty) return new FastCharts.Core.Primitives.FRange(0, 0);
            double minY = Data.Min(p => System.Math.Min(p.Y, Baseline));
            double maxY = Data.Max(p => System.Math.Max(p.Y, Baseline));
            return new FastCharts.Core.Primitives.FRange(minY, maxY);
        }
    }
}
