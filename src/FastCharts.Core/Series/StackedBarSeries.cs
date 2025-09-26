using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Stacked vertical bars at discrete X positions. Each data point contains multiple segment values
    /// that are stacked relative to Baseline. Positive values stack upwards, negative values stack downwards.
    /// Width can be specified per point or inferred (80% of min ΔX) or overridden globally by Width.
    /// </summary>
    public sealed class StackedBarSeries : SeriesBase
    {
        public IList<StackedBarPoint> Data { get; }

        /// <summary>Optional global width (data units). If set, overrides per-point widths and auto inference.</summary>
        public double? Width { get; set; }

        /// <summary>Baseline value for stacks (default 0).</summary>
        public double Baseline { get; set; }

        /// <summary>Fill opacity [0..1] used for each segment fill.</summary>
        public double FillOpacity { get; set; } = 0.85;

        /// <summary>
        /// Optional grouping for clustered stacks. If provided, multiple stacked series can be displayed side-by-side.
        /// </summary>
        public int? GroupCount { get; set; }
        public int? GroupIndex { get; set; }

        public override bool IsEmpty => Data == null || Data.Count == 0;

        public StackedBarSeries()
        {
            Data = new List<StackedBarPoint>();
            Baseline = 0.0;
        }

        public StackedBarSeries(IEnumerable<StackedBarPoint> points)
        {
            Data = new List<StackedBarPoint>(points);
            Baseline = 0.0;
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

        public FRange GetXRange()
        {
            if (IsEmpty)
            {
                return new FRange(0, 0);
            }
            double minX = Data.Min(p => p.X);
            double maxX = Data.Max(p => p.X);
            double w0 = GetWidthFor(0) * 0.5; double wN = GetWidthFor(Data.Count - 1) * 0.5;
            return new FRange(minX - w0, maxX + wN);
        }

        public FRange GetYRange()
        {
            if (IsEmpty)
            {
                return new FRange(0, 0);
            }
            double minY = Baseline, maxY = Baseline;
            foreach (var p in Data)
            {
                double pos = 0.0, neg = 0.0;
                if (p.Values != null)
                {
                    for (int i = 0; i < p.Values.Length; i++)
                    {
                        double v = p.Values[i];
                        if (v >= 0) pos += v; else neg += v;
                    }
                }
                double top = System.Math.Max(Baseline, Baseline + pos);
                double bot = System.Math.Min(Baseline, Baseline + neg);
                if (top > maxY) maxY = top;
                if (bot < minY) minY = bot;
            }
            return new FRange(minY, maxY);
        }
    }
}
