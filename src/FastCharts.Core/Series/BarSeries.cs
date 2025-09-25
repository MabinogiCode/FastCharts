using System.Collections.Generic;
using System.Linq;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Vertical bar (column) series. Each data point draws a rectangle from Baseline to Y at X, with a given width in data units.
    /// If Width is not specified per point, a default width is inferred from neighbor spacing (80% of min delta X).
    /// </summary>
    public sealed class BarSeries : SeriesBase
    {
        public IList<BarPoint> Data { get; }

        /// <summary>
        /// Optional global width (in data units). If set, overrides per-point widths and auto inference.
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// Baseline value for bars (default 0). Bars extend from Baseline to Y.
        /// </summary>
        public double Baseline { get; set; }

        /// <summary>
        /// Fill opacity [0..1]. Renderer will combine series color with this alpha factor.
        /// </summary>
        public double FillOpacity { get; set; }

        /// <summary>
        /// Grouping for side-by-side bars (clustered bars). GroupCount = total series in the cluster, GroupIndex = 0..GroupCount-1.
        /// If null, series is treated as a single group (no clustering).
        /// </summary>
        public int? GroupCount { get; set; }
        public int? GroupIndex { get; set; }

        public override bool IsEmpty
        {
            get { return Data == null || Data.Count == 0; }
        }

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

        /// <summary>
        /// Returns the data-space width to use for the specified bar index, computing an inferred width if necessary.
        /// </summary>
        public double GetWidthFor(int index)
        {
            if (Width.HasValue)
            {
                return Width.Value;
            }
            if (index >= 0 && index < Data.Count)
            {
                var w = Data[index].Width;
                if (w.HasValue && w.Value > 0)
                {
                    return w.Value;
                }
            }

            // infer width from neighbor spacing (80% of min delta X)
            if (Data.Count >= 2)
            {
                double minDx = double.PositiveInfinity;
                for (int i = 1; i < Data.Count; i++)
                {
                    double dx = System.Math.Abs(Data[i].X - Data[i - 1].X);
                    if (dx > 0 && dx < minDx)
                    {
                        minDx = dx;
                    }
                }
                if (double.IsInfinity(minDx) || minDx <= 0)
                {
                    return 1.0; // fallback
                }
                return minDx * 0.8;
            }

            return 1.0;
        }

        public FastCharts.Core.Primitives.FRange GetXRange()
        {
            if (IsEmpty)
            {
                return new FastCharts.Core.Primitives.FRange(0, 0);
            }
            double minX = Data.Min(p => p.X);
            double maxX = Data.Max(p => p.X);
            // expand range by half width at edges
            double w0 = GetWidthFor(0) * 0.5;
            double wN = GetWidthFor(Data.Count - 1) * 0.5;
            return new FastCharts.Core.Primitives.FRange(minX - w0, maxX + wN);
        }

        public FastCharts.Core.Primitives.FRange GetYRange()
        {
            if (IsEmpty)
            {
                return new FastCharts.Core.Primitives.FRange(0, 0);
            }
            double minY = Data.Min(p => System.Math.Min(p.Y, Baseline));
            double maxY = Data.Max(p => System.Math.Max(p.Y, Baseline));
            return new FastCharts.Core.Primitives.FRange(minY, maxY);
        }
    }

    /// <summary>
    /// Bar datum: X position, Y value (height from baseline), and optional per-point width in data units.
    /// </summary>
    public struct BarPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double? Width { get; set; }

        public BarPoint(double x, double y, double? width = null)
        {
            X = x; Y = y; Width = width;
        }
    }
}
